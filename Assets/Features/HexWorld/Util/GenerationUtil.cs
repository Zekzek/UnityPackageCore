using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public static class GenerationUtil
    {
        // Should be a power of 2?
        private const int NOISE_SIZE = 512;

        private static bool _noiseInitialized;
        private static int[] _noiseBase = new int[NOISE_SIZE];

        private static List<TerrainType> _terrainTypes = new List<TerrainType>();
        private static int _regionSize;

        public static WorldObject InstantiateEntity(MovementSpeed speed, Vector2Int gridIndex, Vector2Int? facing = null)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            TileGenerationParams tileParams = CalcTileGenerationParams(gridIndex.x, gridIndex.y);
            instance.AddComponent(new LocationComponent(instance.Id, new Vector3Int(gridIndex.x, tileParams.GridHeight, gridIndex.y), facing, speed));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static WorldObject InstantiateTile(int x, int z)
        {
            TileGenerationParams tileParams = CalcTileGenerationParams(x, z);
            WorldObject tile = new WorldObject(WorldObjectType.Tile);
            tile.AddComponent(new LocationComponent(tile.Id, new Vector3Int(x, tileParams.GridHeight, z)));
            var heightRatio = tileParams.GridHeight / (float)WorldLocation.MAX_HEIGHT;
            tile.AddComponent(new PlatformComponent(tile.Id, heightRatio * new Vector3(tileParams.Temperature, tileParams.Fertility, tileParams.Moisture).normalized));
            tile.AddComponent(new TargetableComponent(tile.Id));
            HexWorld.Instance.Add(tile);

            InstantiateTileDecoration(tile.Location.GridPosition, tileParams);

            return tile;
        }

        private static void InstantiateTileDecoration(Vector3Int gridPosition, TileGenerationParams generationParams)
        {
            Vector2Int facing = FacingUtil.GetFacing(generationParams.Rotation * 360);

            if (generationParams.Fertility + generationParams.Moisture > 1.5f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Bush);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            } else if (generationParams.Fertility + generationParams.Moisture < 0.5f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Rock);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            }
        }

        public static void Init(int seed, int regionSize, params TerrainType[] terrains)
        {
            if (seed >= 0) { Random.InitState(seed); }
            if (regionSize >= 0) { _regionSize = regionSize; }
            InitNoise();
            _terrainTypes.Clear();
            _terrainTypes.AddRange(terrains);
        }

        private static TileGenerationParams CalcTileGenerationParams(TerrainType terrainType, int x, int z)
        {
            switch (terrainType) {
                case TerrainType.Flat: return new TileGenerationParams(5);
                case TerrainType.Chaos: return new TileGenerationParams(CalcSimpleNoise(x, z) * WorldLocation.MAX_HEIGHT / NOISE_SIZE);
                case TerrainType.Desert: return CalcDesertTerrain(x, z);
                case TerrainType.Forest: return CalcForestTerrain(x, z);
                case TerrainType.Hills: return CalcHillsTerrain(x, z);
            }
            throw new System.Exception("No tile generation logic found for " + terrainType);
        }

        private static TileGenerationParams CalcTileGenerationParams(int x, int z)
        {
            // Convert to region space
            float regionX = x / (float)_regionSize;
            int regionXFloor = Mathf.FloorToInt(regionX);
            int regionXCeil = Mathf.CeilToInt(regionX);
            float regionZ = z / (float)_regionSize;
            int regionZFloor = Mathf.FloorToInt(regionZ);
            int regionZCeil = Mathf.CeilToInt(regionZ);

            // Capture regions around the point
            TerrainType lowXLowZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXFloor, 0.3f * regionZFloor) * _terrainTypes.Count / NOISE_SIZE)];
            TerrainType lowXHighZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXFloor, 0.3f * regionZCeil) * _terrainTypes.Count / NOISE_SIZE)];
            TerrainType highXLowZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXCeil, 0.3f * regionZFloor) * _terrainTypes.Count / NOISE_SIZE)];
            TerrainType highXHighZTerrain = _terrainTypes[(int)(CalcLerpNoise(0.3f * regionXCeil, 0.3f * regionZCeil) * _terrainTypes.Count / NOISE_SIZE)];            

            // Lerp the lower X terrains, skip the lerp if terrain matches
            TileGenerationParams lowX;
            if (lowXLowZTerrain == lowXHighZTerrain) {
                lowX = CalcTileGenerationParams(lowXLowZTerrain, x, z);
            } else {
                lowX = TileGenerationParams.Lerp(
                    CalcTileGenerationParams(lowXLowZTerrain, x, z),
                    CalcTileGenerationParams(lowXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the higher X terrains, skip the lerp if terrain matches
            TileGenerationParams highX;
            if (highXLowZTerrain == highXHighZTerrain) {
                highX = CalcTileGenerationParams(highXLowZTerrain, x, z);
            } else {
                highX = TileGenerationParams.Lerp(
                    CalcTileGenerationParams(highXLowZTerrain, x, z),
                    CalcTileGenerationParams(highXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the final reult
            return TileGenerationParams.Lerp(lowX, highX, CalculateSmoothLerpTime(regionX - regionXFloor));
        }


        private static int CalcSimpleNoise(int x, int z, int modulo) { return CalcSimpleNoise(x, z) * modulo / NOISE_SIZE; }
        private static int CalcSimpleNoise(int x, int z) { return (_noiseBase[Mathf.Abs(x) % NOISE_SIZE] + _noiseBase[Mathf.Abs(z) % NOISE_SIZE]) % NOISE_SIZE; }
        

        private static TileGenerationParams CalcDesertTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.06f * x, 0.03f * z),
                        CalcLerpNoise(0.03f * x, 0.06f * z));

            int offset = 0;
            return new TileGenerationParams(
                height: heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                rotation: CalcPercent(x, z, offset++, 0.95f),
                moisture: DecreasePercent(CalcPercent(x, z, offset++, 0.96f)),
                temperature: IncreasePercent(CalcPercent(x, z, offset++, 0.97f)),
                fertility: DecreasePercent(CalcPercent(x, z, offset++, 0.98f))
            );
        }

        private static TileGenerationParams CalcForestTerrain(int x, int z)
        {
            float heightNoise = Combine(0.8f,
                        CalcLerpNoise(0.2f * x, 0.1f * z),
                        CalcLerpNoise(0.1f * x, 0.2f * z),
                        Combine(0.5f,
                            CalcLerpNoise(0.01f * x, 0.03f * z),
                            CalcLerpNoise(0.03f * x, 0.01f * z)));

            int offset = 0;
            return new TileGenerationParams(
                height: heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                rotation: CalcPercent(x, z, offset++, 0.95f),
                moisture: IncreasePercent(CalcPercent(x, z, offset++, 0.96f)),
                temperature: CalcPercent(x, z, offset++, 0.97f),
                fertility: IncreasePercent(CalcPercent(x, z, offset++, 0.98f))
            );
        }

        private static TileGenerationParams CalcHillsTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.04f * x, 0.08f * z),
                        CalcLerpNoise(0.08f * x, 0.04f * z));

            int offset = 0;
            return new TileGenerationParams(
                height: heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                rotation: CalcPercent(x, z, offset++, 0.95f),
                moisture: CalcPercent(x, z, offset++, 0.96f),
                temperature: CalcPercent(x, z, offset++, 0.97f),
                fertility: CalcPercent(x, z, offset++, 0.98f)
            );
        }

        private static float IncreasePercent(float value) { return 1f - (value * value); }
        private static float DecreasePercent(float value) { return value * value; }

        private static float CalcPercent(int x, int z, int offset, float clusterRatio) {
            float isolateRatio = 1.0001f - (clusterRatio * clusterRatio * clusterRatio);
            return CalcLerpNoise(offset + isolateRatio * x, offset + isolateRatio * z) / NOISE_SIZE;
        }

        private static float CalcLerpNoise(float x, float z)
        {
            int floorX = Mathf.FloorToInt(x);
            int floorZ = Mathf.FloorToInt(z);

            int lowXLowZ = CalcSimpleNoise(floorX, floorZ);
            int lowXHighZ = CalcSimpleNoise(floorX, floorZ + 1);
            int highXLowZ = CalcSimpleNoise(floorX + 1, floorZ);
            int highXHighZ = CalcSimpleNoise(floorX + 1, floorZ + 1);

            float lowX = Mathf.Lerp(lowXLowZ, lowXHighZ, CalculateSmoothLerpTime(z - floorZ));
            float highX = Mathf.Lerp(highXLowZ, highXHighZ, CalculateSmoothLerpTime(z - floorZ));
            return Mathf.Lerp(lowX, highX, CalculateSmoothLerpTime(x - floorX));
        }

        // https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise
        private static float CalculateSmoothLerpTime(float time)
        {
            return 6 * time * time * time * time * time - 15 * time * time * time * time + 10 * time * time * time;
        }

        private static float Scale(float value, float minPercent, float maxPercent)
        {
            return NOISE_SIZE * (minPercent + (maxPercent - minPercent) * (value / NOISE_SIZE));
        }

        // Combine values while working to avoid loosing peaks and valleys
        private static float Combine(float favorExtremes, params float[] values)
        {
            float total = 0;
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (float value in values) {
                total += value;
                if (value < min) { min = value; }
                if (value > max) { max = value; }
            }

            float extreme = min > NOISE_SIZE - max ? max : min;

            return Mathf.Lerp(total / values.Length, extreme, favorExtremes);
        }

        private static void InitNoise()
        {
            if (_noiseInitialized) { return; }
            _noiseInitialized = true;

            List<int> values = new List<int>(NOISE_SIZE);
            for (int i = 0; i < NOISE_SIZE; i++) { values.Add(i); }

            while (values.Count > 0) {
                int index = Random.Range(0, values.Count - 1);
                _noiseBase[values.Count - 1] = values[index];
                values.RemoveAt(index);
            }
        }

        private class TileGenerationParams
        {
            //TODO: Move moisture & temperature to weather object which changes over time
            //TODO: weather can effect decorations, entities, etc

            public int GridHeight => (int)Height;
            
            public float Height { get; private set; }
            public float Rotation { get; private set; }
            public float Moisture { get; private set; }
            public float Temperature { get; private set; }
            public float Fertility { get; private set; }

            public TileGenerationParams(float height = 0, float rotation = 0, float moisture = 0, float temperature = 0, float fertility = 0) {
                Height = height;
                Rotation = rotation;
                Moisture = moisture;
                Temperature = temperature;
                Fertility = fertility;
            }

            public static TileGenerationParams Lerp(TileGenerationParams a, TileGenerationParams b, float ratio) {
                return new TileGenerationParams(
                    height: Mathf.Lerp(a.Height, b.Height, ratio),
                    rotation: Mathf.Lerp(a.Rotation, b.Rotation, ratio),
                    moisture: Mathf.Lerp(a.Moisture, b.Moisture, ratio),
                    temperature: Mathf.Lerp(a.Temperature, b.Temperature, ratio),
                    fertility: Mathf.Lerp(a.Fertility, b.Fertility, ratio)
                );
            }
        }
    }
}
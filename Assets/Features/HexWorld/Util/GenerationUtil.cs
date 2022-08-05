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

        public static void Init(int seed, int regionSize, params TerrainType[] terrains)
        {
            if (seed >= 0) { Random.InitState(seed); }
            if (regionSize >= 0) { _regionSize = regionSize; }
            InitNoise();
            _terrainTypes.Clear();
            _terrainTypes.AddRange(terrains);
        }

        public static WorldObject InstantiateEntity(MovementSpeed speed, Vector2Int gridIndex, Vector2Int? facing = null)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            GenerationParams generationParams = CalcGenerationParams(gridIndex.x, gridIndex.y);
            instance.AddComponent(new LocationComponent(instance.Id, new Vector3Int(gridIndex.x, generationParams.GridHeight, gridIndex.y), facing, speed));
            instance.AddComponent(InstantiateRelationship(generationParams));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        private static RelationshipComponent InstantiateRelationship(GenerationParams generationParams)
        {
            RelationshipComponent relationship = new RelationshipComponent(0);
            relationship.AddDefaultAffinity(RelationshipType.Trust, generationParams.Trust);
            relationship.AddDefaultAffinity(RelationshipType.Affection, generationParams.Affection);
            return relationship;
        }

        public static void InstantiateAtGridIndex(int x, int z)
        {
            GenerationParams generationParams = CalcGenerationParams(x, z);
            Vector3Int gridPosition = new Vector3Int(x, generationParams.GridHeight, z);

            InstantiateTile(gridPosition, generationParams);
            CheckInstantiateTileDecoration(gridPosition, generationParams);
            CheckInstantiateTileEntity(gridPosition, generationParams);
        }

        private static void InstantiateTile(Vector3Int gridPosition, GenerationParams generationParams)
        {
            float heightRatio = generationParams.GridHeight / (float)WorldLocation.MAX_HEIGHT;
            WorldObject tile = new WorldObject(WorldObjectType.Tile);
            tile.AddComponent(new LocationComponent(tile.Id, gridPosition));
            tile.AddComponent(new PlatformComponent(tile.Id, heightRatio * new Vector3(generationParams.Temperature, generationParams.Fertility, generationParams.Moisture).normalized));
            tile.AddComponent(new TargetableComponent(tile.Id));
            HexWorld.Instance.Add(tile);
        }

        private static void CheckInstantiateTileDecoration(Vector3Int gridPosition, GenerationParams generationParams)
        {
            Vector2Int facing = FacingUtil.GetFacing(generationParams.Rotation * 360);

            float chaosFactor = CalcSimpleNoisePercent(gridPosition.x, gridPosition.z);
            if (chaosFactor * (generationParams.Fertility + generationParams.Moisture) > 0.75f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Bush);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            } else if (chaosFactor * (generationParams.Fertility + generationParams.Moisture) < 0.25f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Rock);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            }
        }

        private static void CheckInstantiateTileEntity(Vector3Int gridPosition, GenerationParams generationParams)
        {
            float chaosFactor = CalcSimpleNoisePercent(gridPosition.x, gridPosition.z);
            if (chaosFactor * (generationParams.Fertility + generationParams.Moisture) > 1.5f) {
                Vector2Int facing = FacingUtil.GetFacing(generationParams.Rotation * 360);
                InstantiateEntity(new MovementSpeed(1,1,1,1,1,1,1,1,1), new Vector2Int(gridPosition.x, gridPosition.z), facing);
            }
        }

        private static GenerationParams CalcTileGenerationParams(TerrainType terrainType, int x, int z)
        {
            switch (terrainType) {
                case TerrainType.Flat: return new GenerationParams() { Height = 5 };
                case TerrainType.Chaos: return new GenerationParams() { Height = CalcSimpleNoise(x, z) * WorldLocation.MAX_HEIGHT / NOISE_SIZE };
                case TerrainType.Desert: return CalcDesertTerrain(x, z);
                case TerrainType.Forest: return CalcForestTerrain(x, z);
                case TerrainType.Hills: return CalcHillsTerrain(x, z);
            }
            throw new System.Exception("No tile generation logic found for " + terrainType);
        }

        private static GenerationParams CalcGenerationParams(int x, int z)
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
            GenerationParams lowX;
            if (lowXLowZTerrain == lowXHighZTerrain) {
                lowX = CalcTileGenerationParams(lowXLowZTerrain, x, z);
            } else {
                lowX = GenerationParams.Lerp(
                    CalcTileGenerationParams(lowXLowZTerrain, x, z),
                    CalcTileGenerationParams(lowXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the higher X terrains, skip the lerp if terrain matches
            GenerationParams highX;
            if (highXLowZTerrain == highXHighZTerrain) {
                highX = CalcTileGenerationParams(highXLowZTerrain, x, z);
            } else {
                highX = GenerationParams.Lerp(
                    CalcTileGenerationParams(highXLowZTerrain, x, z),
                    CalcTileGenerationParams(highXHighZTerrain, x, z),
                    CalculateSmoothLerpTime(regionZ - regionZFloor));
            }

            // Lerp the final reult
            return GenerationParams.Lerp(lowX, highX, CalculateSmoothLerpTime(regionX - regionXFloor));
        }


        private static float CalcSimpleNoisePercent(int x, int z) { return CalcSimpleNoise(x, z) / (float)NOISE_SIZE; }
        private static int CalcSimpleNoise(int x, int z, int modulo) { return CalcSimpleNoise(x, z) * modulo / NOISE_SIZE; }
        private static int CalcSimpleNoise(int x, int z) { return (_noiseBase[Mathf.Abs(x) % NOISE_SIZE] + _noiseBase[Mathf.Abs(z) % NOISE_SIZE]) % NOISE_SIZE; }
        

        private static GenerationParams CalcDesertTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.06f * x, 0.03f * z),
                        CalcLerpNoise(0.03f * x, 0.06f * z));

            int offset = 0;
            return new GenerationParams() {
                Height = heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                Rotation = CalcPercent(x, z, offset++, 0.95f),
                Moisture = DecreasePercent(CalcPercent(x, z, offset++, 0.96f)),
                Temperature = IncreasePercent(CalcPercent(x, z, offset++, 0.97f)),
                Fertility = DecreasePercent(CalcPercent(x, z, offset++, 0.98f)),
                Trust = CalcPercent(x, z, offset++, 0.5f),
                Affection = CalcPercent(x, z, offset++, 0.6f)
            };
        }

        private static GenerationParams CalcForestTerrain(int x, int z)
        {
            float heightNoise = Combine(0.8f,
                        CalcLerpNoise(0.2f * x, 0.1f * z),
                        CalcLerpNoise(0.1f * x, 0.2f * z),
                        Combine(0.5f,
                            CalcLerpNoise(0.01f * x, 0.03f * z),
                            CalcLerpNoise(0.03f * x, 0.01f * z)));

            int offset = 0;
            return new GenerationParams() {
                Height = heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                Rotation = CalcPercent(x, z, offset++, 0.95f),
                Moisture = IncreasePercent(CalcPercent(x, z, offset++, 0.96f)),
                Temperature = CalcPercent(x, z, offset++, 0.97f),
                Fertility = IncreasePercent(CalcPercent(x, z, offset++, 0.98f)),
                Trust = CalcPercent(x, z, offset++, 0.5f),
                Affection = CalcPercent(x, z, offset++, 0.6f)
            };
        }

        private static GenerationParams CalcHillsTerrain(int x, int z)
        {
            float heightNoise = Combine(0f,
                        CalcLerpNoise(0.04f * x, 0.08f * z),
                        CalcLerpNoise(0.08f * x, 0.04f * z));

            int offset = 0;
            return new GenerationParams() {
                Height = heightNoise * WorldLocation.MAX_HEIGHT / NOISE_SIZE,
                Rotation = CalcPercent(x, z, offset++, 0.95f),
                Moisture = CalcPercent(x, z, offset++, 0.96f),
                Temperature = CalcPercent(x, z, offset++, 0.97f),
                Fertility = CalcPercent(x, z, offset++, 0.98f),
                Trust = CalcPercent(x, z, offset++, 0.5f),
                Affection = CalcPercent(x, z, offset++, 0.6f)
            };
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

        private class GenerationParams
        {
            //TODO: Move moisture & temperature to weather object which changes over time
            //TODO: weather can effect decorations, entities, etc

            public int GridHeight => (int)Height;
            
            public float Height { get; set; }
            public float Rotation { get; set; }
            public float Moisture { get; set; }
            public float Temperature { get; set; }
            public float Fertility { get; set; }
            public float Trust { get; set; }
            public float Affection { get; set; }

            public static GenerationParams Lerp(GenerationParams a, GenerationParams b, float ratio) {
                return new GenerationParams() {
                    Height = Mathf.Lerp(a.Height, b.Height, ratio),
                    Rotation = Mathf.Lerp(a.Rotation, b.Rotation, ratio),
                    Moisture = Mathf.Lerp(a.Moisture, b.Moisture, ratio),
                    Temperature = Mathf.Lerp(a.Temperature, b.Temperature, ratio),
                    Fertility = Mathf.Lerp(a.Fertility, b.Fertility, ratio),
                    Trust = Mathf.Lerp(a.Trust, b.Trust, ratio),
                    Affection = Mathf.Lerp(a.Affection, b.Affection, ratio)
                };
            }
        }
    }
}
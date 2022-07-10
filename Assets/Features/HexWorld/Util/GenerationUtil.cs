using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public static class GenerationUtil
    {
        public enum WorldType
        {
            Flat,
            Standard
        }

        // Should be a power of 2
        private const int NOISE_SIZE = 512;

        private static bool _noiseInitialized;
        private static int[] _noiseBase = new int[NOISE_SIZE];

        //TODO: define regions with different generation types? They should be fairly large and have lerp overlap to support smooth transitions
        private static TerrianType terrainType = TerrianType.Forest;


        private static int _seed;
        public static int Seed { get => _seed; set { Random.InitState(value); _seed = value; } }
        public static WorldType WorldGenerationType {get; set;}

        public static WorldObject InstantiateEntity(MovementSpeed speed, Vector2Int gridIndex, Vector2Int? facing = null)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            instance.AddComponent(new LocationComponent(instance.Id, GetTileGridPosition(gridIndex.x, gridIndex.y), facing, speed));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static WorldObject InstantiateTile(int x, int z)
        {
            Vector3Int gridPosition = GetTileGridPosition(x, z);
            WorldObject tile = new WorldObject(WorldObjectType.Tile);
            tile.AddComponent(new LocationComponent(tile.Id, gridPosition));
            tile.AddComponent(new PlatformComponent(tile.Id));
            tile.AddComponent(new TargetableComponent(tile.Id));
            HexWorld.Instance.Add(tile);

            InstantiateTileDecoration(gridPosition, CalculateBaseNoise(x, z));

            return tile;
        }

        private static void InstantiateTileDecoration(Vector3Int gridPosition, int noiseValue)
        {
            Vector2Int facing = FacingUtil.GetFacing(GetRotationAngle(gridPosition.x + 1, gridPosition.z + 1));

            if (noiseValue > NOISE_SIZE * 0.8f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Rock);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            } else if (noiseValue > NOISE_SIZE * 0.75f) {
                WorldObject decoration = new WorldObject(WorldObjectType.Bush);
                decoration.AddComponent(new LocationComponent(decoration.Id, gridPosition, facing));
                HexWorld.Instance.Add(decoration);
            }
        }

        private static Vector3Int GetTileGridPosition(int x, int z) { return new Vector3Int(x, GetTileGridHeightAt(x, z), z); }
        private static int GetTileGridHeightAt(int x, int y) { return (int)(CalculateNoise(x, y) * WorldLocation.MAX_HEIGHT / NOISE_SIZE); }
        private static int GetRotationAngle(int x, int y) { return (int)(CalculateBaseNoise(x, y) * 360 / NOISE_SIZE - 180); }


        private static float CalculateNoise(int x, int y) { return CalculateNoise(x, y, WorldGenerationType); }

        // Guaranteed to return a value between 0 and NOISE_SIZE
        private static float CalculateNoise(int x, int y, GenerationUtil.WorldType worldType)
        {
            if (worldType == GenerationUtil.WorldType.Flat) { return 1; }

            InitNoise();
            return CalculateRollingTerrain(x, y);
        }

        private static float CalculateCragsTerrain(int x, int y)
        {
            return Combine(1,
                    Combine(0.25f,
                        CalculateLerpNoise(0.01f * x, 0.01f * y),
                        CalculateLerpNoise(0.5f * x, 0.5f * y)),
                    Combine(1f,
                        CalculateLerpNoise(0.03f * x, 0.03f * y),
                        CalculateLerpNoise(0.01f * x, 0.01f * y)));
        }

        private static float CalculateCliffsAndPlainsTerrain(int x, int y)
        {
            return Combine(0.8f,
                    CalculateLerpNoise(0.2f * x, 0.1f * y),
                    CalculateLerpNoise(0.1f * x, 0.2f * y),
                    Combine(0.5f,
                        CalculateLerpNoise(0.01f * x, 0.03f * y),
                        CalculateLerpNoise(0.03f * x, 0.01f * y)));
        }

        private static float CalculateRollingTerrain(int x, int y)
        {
            return Combine(0.5f,
                        CalculateLerpNoise(0.04f * x, 0.08f * y),
                        CalculateLerpNoise(0.08f * x, 0.04f * y));
        }

        private static float CalculateGentleRollingTerrain(int x, int y)
        {
            return Combine(0f,
                        CalculateLerpNoise(0.06f * x, 0.03f * y),
                        CalculateLerpNoise(0.03f * x, 0.06f * y));
        }

        private static int CalculateBaseNoise(int x, int y)
        {
            return _noiseBase[Mathf.Abs(x * x + y) % NOISE_SIZE];
        }

        private static float CalculateLerpNoise(float x, float y)
        {
            int floorX = Mathf.FloorToInt(x);
            int floorY = Mathf.FloorToInt(y);

            int lowXLowY = CalculateBaseNoise(floorX, floorY);
            int lowXHighY = CalculateBaseNoise(floorX, floorY + 1);
            int highXLowY = CalculateBaseNoise(floorX + 1, floorY);
            int highXHighY = CalculateBaseNoise(floorX + 1, floorY + 1);

            float lowX = Mathf.Lerp(lowXLowY, lowXHighY, CalculateSmoothLerpTime(y - floorY));
            float highX = Mathf.Lerp(highXLowY, highXHighY, CalculateSmoothLerpTime(y - floorY));
            return Mathf.Lerp(lowX, highX, CalculateSmoothLerpTime(x - floorX));
        }


        // https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise
        private static float CalculateSmoothLerpTime(float time)
        {
            return time;//6 * time * time * time * time * time - 15 * time * time * time * time + 10 * time * time * time;
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
    }
}
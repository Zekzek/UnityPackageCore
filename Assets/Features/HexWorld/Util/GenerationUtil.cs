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

        // Must be a power of 2
        private const int NOISE_SIZE = 512;

        private static int _seed;
        public static int Seed { get => _seed; set { Random.InitState(value); _seed = value; } }
        public static WorldType WorldGenerationType {get; set;}

        private static bool _noiseInitialized;
        private static int[] _noiseBase = new int[NOISE_SIZE];

        public static WorldObject CreateTile(Vector2Int gridIndex)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Tile);
            instance.AddComponent(new LocationComponent(instance.Id, GetTileGridPosition(gridIndex.x, gridIndex.y)));
            instance.AddComponent(new PlatformComponent(instance.Id));
            instance.AddComponent(new TargetableComponent(instance.Id));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static WorldObject CreateEntity(MovementSpeed speed, Vector2Int gridIndex, Vector2Int? facing = null)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            instance.AddComponent(new LocationComponent(instance.Id, GetTileGridPosition(gridIndex.x, gridIndex.y), facing, speed));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static Vector3Int GetTileGridPosition(Vector2Int index) { return GetTileGridPosition(index.x, index.y); }
        public static Vector3Int GetTileGridPosition(int x, int z) { return new Vector3Int(x, GetTileGridHeightAt(x, z), z); }
        private static int GetTileGridHeightAt(int x, int y) {return CalculateNoise(x, y) * WorldLocation.MAX_HEIGHT / NOISE_SIZE; }

        // Guaranteed to return a value between 0 and NOISE_SIZE
        private static int CalculateNoise(int x, int y)
        {
            if (WorldGenerationType == WorldType.Flat) { return 1; }

            InitNoise();
            //TODO: make this more sophisticated

            return Combine(0.8f, 0, NOISE_SIZE, CalculateLerpNoise(x * 0.3f, y * 0.3f), CalculateLerpNoise(0.1f * x, 0.1f * y));
        }

        private static int CalculateLerpNoise(float x, float y)
        {
            int floorX = Mathf.FloorToInt(x);
            int floorY = Mathf.FloorToInt(y);

            int lowXLowY = CalculateBaseNoise(floorX, floorY);
            int lowXHighY = CalculateBaseNoise(floorX, floorY + 1);
            int highXLowY = CalculateBaseNoise(floorX + 1, floorY);
            int highXHighY = CalculateBaseNoise(floorX + 1, floorY + 1);

            float lowX = Mathf.Lerp(lowXLowY, lowXHighY, CalculateSmoothLerpTime(y - floorY));
            float highX = Mathf.Lerp(highXLowY, highXHighY, CalculateSmoothLerpTime(y - floorY));
            return Mathf.RoundToInt(Mathf.Lerp(lowX, highX, CalculateSmoothLerpTime(x - floorX)));
        }

        private static int CalculateBaseNoise(int x, int y)
        {
            return _noiseBase[x * x + Mathf.Abs(y) % NOISE_SIZE];
        }

        // https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise
        private static float CalculateSmoothLerpTime(float time)
        {
            return time;//6 * time * time * time * time * time - 15 * time * time * time * time + 10 * time * time * time;
        }

        // Combine values while working to avoid loosing peaks and valleys
        private static int Combine(float favorExtremes, float absoluteMin, float absoluteMax, params float[] values)
        {
            float total = 0;
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach(float value in values) {
                total += value;
                if (value < min) { min = value; }
                if (value > max) { max = value; }
            }

            float extreme = min - absoluteMin > absoluteMax - max ? max : min;

            return (int)Mathf.Lerp(total/values.Length, extreme, favorExtremes);
        }

        private static void InitNoise()
        {
            if (_noiseInitialized) { return; }
            _noiseInitialized = true;
            for (int i = 0; i < NOISE_SIZE; i++) {
                _noiseBase[i] = Random.Range(0, NOISE_SIZE - 1);
            }
        }
    }
}
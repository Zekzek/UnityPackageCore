using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public static class GenerationUtil
    {
        private static int _seed;
        public static int Seed { get => _seed; set { Random.InitState(value); _seed = value; } }

        public static void CreateRandomTilesAround(Vector2Int centerIndex)
        {
            WorldObject centerTile = HexWorld.Instance.GetFirstAt(centerIndex, WorldObjectType.Tile);
            if (centerTile == null) { centerTile = CreateTile(centerIndex, CreateRandomGridHeightNear(-1)); }

            IEnumerable<Vector2Int> adjacentIndices = WorldUtil.GetBurstIndicesAround(centerIndex, 1, false);
            foreach (Vector2Int adjacentIndex in adjacentIndices) {
                WorldObject adjacentTile = HexWorld.Instance.GetFirstAt(adjacentIndex, WorldObjectType.Tile);
                if (adjacentTile == null) { adjacentTile = CreateTile(adjacentIndex, CreateRandomGridHeightNear(centerTile.Location.GridHeight)); }
            }
        }

        private static int CreateRandomGridHeightNear(int height)
        {
            if (height < 0) { return Random.Range(0, WorldLocation.MAX_HEIGHT); }
            int min = Mathf.Max(height - 2, 0);
            int max = Mathf.Min(height + 2, WorldLocation.MAX_HEIGHT);
            return Random.Range(min, max);
        }

        public static WorldObject CreateTile(Vector2Int gridIndex, int height) { return CreateTile(new Vector3Int(gridIndex.x, height, gridIndex.y)); }

        public static WorldObject CreateTile(Vector3Int gridPosition)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Tile);
            instance.AddComponent(new LocationComponent(instance.Id, gridPosition));
            instance.AddComponent(new PlatformComponent(instance.Id));
            instance.AddComponent(new TargetableComponent(instance.Id));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static WorldObject CreateEntity(MovementSpeed speed, Vector3Int gridPosition, Vector2Int? facing = null)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            instance.AddComponent(new LocationComponent(instance.Id, gridPosition, facing, speed));
            HexWorld.Instance.Add(instance);
            return instance;
        }

    }
}
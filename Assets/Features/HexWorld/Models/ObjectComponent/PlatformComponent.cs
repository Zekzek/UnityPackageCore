using UnityEngine;

namespace Zekzek.HexWorld
{
    public class PlatformComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Platform;

        public PlatformComponent(uint worldObjectId, Vector3 colorCode) : base(worldObjectId) { ColorCode = colorCode; }

        // Lazy load and cache neighbors. No need to update unless new tiles are created.
        private LocationComponent ne, e, se, sw, w, nw;
        public LocationComponent NE => ne ??= GetNeighbor(FacingUtil.NE);
        public LocationComponent E => e ??= GetNeighbor(FacingUtil.E);
        public LocationComponent SE => se ??= GetNeighbor(FacingUtil.SE);
        public LocationComponent SW => sw ??= GetNeighbor(FacingUtil.SW);
        public LocationComponent W => w ??= GetNeighbor(FacingUtil.W);
        public LocationComponent NW => nw ??= GetNeighbor(FacingUtil.NW);
        public Vector3 ColorCode { get; private set; }

        private LocationComponent GetNeighbor(Vector2Int offset)
        {
            LocationComponent locationComponent = (LocationComponent)GetSibling(WorldComponentType.Location);
            return (LocationComponent)(HexWorld.Instance.GetFirstAt(locationComponent.GridIndex + offset, WorldComponentType.Platform)?.GetComponent(WorldComponentType.Location));
        }

        public bool Raise()
        {
            LocationComponent locationComponent = (LocationComponent)GetSibling(WorldComponentType.Location);
            if (locationComponent != null && locationComponent.GridPosition.y < WorldLocation.MAX_HEIGHT) {
                locationComponent.ScheduleGridShift(Vector3Int.up, 0.2f);
                MoveOthers();
                return true;
            }
            return false;
        }

        public bool Lower()
        {
            LocationComponent locationComponent = (LocationComponent)GetSibling(WorldComponentType.Location);
            if (locationComponent != null && locationComponent.GridPosition.y > 0) {
                locationComponent.ScheduleGridShift(Vector3Int.down, 0.2f); ;
                MoveOthers();
                return true;
            }
            return false;
        }

        private void MoveOthers() 
        {
            //TODO
            Debug.LogWarning("MoveOthers is not yet implemented");
        }
    }
}
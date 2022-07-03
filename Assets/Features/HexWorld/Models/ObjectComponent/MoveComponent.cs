using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class MoveComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Moveable;
        
        public MovementSpeed Speed { get; private set; }

        public MoveComponent(uint worldObjectId, MovementSpeed speed) : base(worldObjectId) { }


        public void NavigateTo(Vector3Int targetGridPos, MovementSpeed speed)
        {
            WorldScheduler.Instance.TryGetLocation(WorldObjectId, out WorldLocation previous, out WorldLocation next, out float percent);
            NavStep lastStep = new NavStep(MoveType.NONE, previous.GridPosition, FacingUtil.GetFacing(previous.RotationAngle), WorldScheduler.Instance.Time);
            WorldUtil.FindShortestPathAsync(lastStep, targetGridPos, speed, (path) => { UpdateGoalPath(path); });
        }

        private void UpdateGoalPath(List<NavStep> path)
        {
            WorldScheduler.Instance.TryGetLocation(WorldObjectId, out WorldLocation previous, out WorldLocation next, out float percent);
            WorldLocation currentLocation = WorldLocation.Lerp(previous, next, percent);
            WorldScheduler.Instance.Unregister(WorldObjectId);
            WorldScheduler.Instance.RegisterIn(0, WorldObjectId, currentLocation);
            if (path == null) { return; }
            foreach (NavStep step in path) {
                WorldScheduler.Instance.RegisterAt(step.WorldTime, WorldObjectId, new WorldLocation(step.GridPos, step.Facing));
            }
        }

        public override string ToString()
        {
            return $"Speed:{Speed}";
        }
    }
}
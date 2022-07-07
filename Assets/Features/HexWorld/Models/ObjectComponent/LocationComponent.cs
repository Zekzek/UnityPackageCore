using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class LocationComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Location;

        private readonly OrderedList<TimedLocation> _scheduledLocations = new OrderedList<TimedLocation>();

        public MovementSpeed Speed { get; private set; }
        public WorldLocation Current { 
            get {
                //TODO: cache a copy keyed to the time?
                ForgetPastLocations();
                return GetAt(); 
            }
        }

        public WorldLocation Previous {
            get {
                ForgetPastLocations();
                return _scheduledLocations.First.location;
            }
        }

        public Vector3 Position => Current.Position;
        public Vector3Int GridPosition => Current.GridPosition;
        public Vector2Int GridIndex => Current.GridIndex;
        public int GridHeight => Current.GridHeight;
        public float RotationAngle => Current.RotationAngle;
        public Vector2Int Facing => Current.Facing;

        public LocationComponent(uint worldObjectId, Vector3Int gridPosition, Vector2Int? facing = null, MovementSpeed speed = null) : base(worldObjectId)
        {
            if (facing == null) { facing = FacingUtil.E; }
            Schedule(new WorldLocation(gridPosition, facing.Value));
            Speed = speed;
        }

        public WorldLocation GetAt(float atTime = -1)
        {
            float now = WorldScheduler.Instance.Time;
            if (atTime < now) { atTime = now; }
            lock (WorldUtil.SYNC_TARGET) {
                _scheduledLocations.TryGetAround(atTime, out TimedLocation before, out TimedLocation after);
                if (after.location == null) {
                    return before.location;
                }
                float percentComplete = (atTime - before.time) / (after.time - before.time);
                return WorldLocation.Lerp(before.location, after.location, percentComplete);
            }
        }

        public void Schedule(WorldLocation location, float delay = 0)
        {
            float atTime = WorldScheduler.Instance.Time + delay;
            ClearScheduleAfter(atTime);
            lock (WorldUtil.SYNC_TARGET) {
                _scheduledLocations.Add(atTime, new TimedLocation { time = atTime, location = location });
                HexWorld.Instance.AddPositionToExistingItem(WorldObjectId, location.GridIndex);
            }
        }

        public void Schedule(List<NavStep> path)
        {
            ClearSchedule();
            lock (WorldUtil.SYNC_TARGET) {
                if (path == null) { return; }
                foreach (NavStep step in path) {
                    _scheduledLocations.Add(step.WorldTime, new TimedLocation { time = step.WorldTime, location = step.Location });
                    HexWorld.Instance.AddPositionToExistingItem(WorldObjectId, step.Location.GridIndex);
                }
            }
        }

        public void ScheduleGridShift(Vector3Int gridOffset, float delay = 0)
        {
            Schedule(new WorldLocation(GridPosition + gridOffset, RotationAngle), delay);
        }

        public void ClearSchedule()
        {
            WorldLocation current = Current;
            lock (WorldUtil.SYNC_TARGET) {
                ClearScheduleAfter(0);
                _scheduledLocations.Clear();
                Schedule(current);
            }
        }

        public void ClearScheduleAfter(float time)
        {
            lock (WorldUtil.SYNC_TARGET) {
                for (int i = _scheduledLocations.Count - 1; i >= 0; i--) {
                    if (_scheduledLocations[i].time > time) {
                        HexWorld.Instance.RemovePositionFromExistingItem(WorldObjectId, _scheduledLocations[i].location.GridIndex);
                        _scheduledLocations.RemoveAt(i);
                    } else {
                        break;
                    }
                }
            }
        }

        private void ForgetPastLocations()
        {
            float now = WorldScheduler.Instance.Time;
            lock (WorldUtil.SYNC_TARGET) {
                while (_scheduledLocations.Count > 1 && _scheduledLocations[1].time < now) {
                    HexWorld.Instance.RemovePositionFromExistingItem(WorldObjectId, _scheduledLocations[0].location.GridIndex);
                    _scheduledLocations.RemoveAt(0);
                }
            }
        }

        public void NavigateTo(Vector3Int targetGridPos, MovementSpeed speed)
        {
            NavStep lastStep = new NavStep(MoveType.NONE, Previous, WorldScheduler.Instance.Time);
            WorldUtil.FindShortestPathAsync(WorldObjectId, this, targetGridPos, (path) => { Schedule(path); });
        }

        public override string ToString()
        {
            return $"Position:{Current.GridPosition}, Facing:{Current.Facing}, Speed:{Speed}";
        }

        private struct TimedLocation
        {
            public float time;
            public WorldLocation location;
        }
    }
}
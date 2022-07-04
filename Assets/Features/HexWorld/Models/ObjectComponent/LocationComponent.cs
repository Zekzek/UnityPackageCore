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
        public float RotationAngle => Current.RotationAngle;
        public Vector2Int Facing => Current.Facing;

        public LocationComponent(uint worldObjectId, Vector3Int gridPosition, float rotationAngle = 0, MovementSpeed speed = null) : base(worldObjectId)
        {
            Schedule(new WorldLocation(gridPosition, rotationAngle));
            Speed = speed;
        }

        public WorldLocation GetAt(float atTime = -1)
        {
            float now = WorldScheduler.Instance.Time;
            if (atTime < now) { atTime = now; }
            lock(_scheduledLocations) {
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
            lock (_scheduledLocations) {
                _scheduledLocations.Add(atTime, new TimedLocation { time = atTime, location = location });
            }
        }

        public void Schedule(List<NavStep> path)
        {
            ClearSchedule();
            float now = WorldScheduler.Instance.Time;
            lock (_scheduledLocations) {
                foreach (NavStep step in path) {
                    _scheduledLocations.Add(step.WorldTime, new TimedLocation { time = step.WorldTime, location = new WorldLocation(step.GridPos, step.Facing) });
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
            lock (_scheduledLocations) {
                _scheduledLocations.Clear();
                Schedule(current);
            }
        }

        public void ClearScheduleAfter(float time)
        {
            lock (_scheduledLocations) {
                for (int i = _scheduledLocations.Count - 1; i >= 0; i--) {
                    if (_scheduledLocations[i].time > time) {
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
            lock (_scheduledLocations) {
                while (_scheduledLocations.Count > 1 && _scheduledLocations[1].time < now) {
                    _scheduledLocations.RemoveAt(0);
                }
            }
        }

        public void NavigateTo(Vector3Int targetGridPos, MovementSpeed speed)
        {
            var previous = Previous;
            NavStep lastStep = new NavStep(MoveType.NONE, previous.GridPosition, FacingUtil.GetFacing(previous.RotationAngle), WorldScheduler.Instance.Time);
            WorldUtil.FindShortestPathAsync(lastStep, targetGridPos, speed, (path) => { Schedule(path); });
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
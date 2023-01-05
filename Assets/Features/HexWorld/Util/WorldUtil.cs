using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public static class WorldUtil
    {
        private const float HORIZONTAL_DISTANCE = 1f;
        private const float VERTICAL_DISTANCE = 0.85f;
        private const float PESSIMIST_PATHING_MULTIPLIER = 10f;

        public const float HEIGHT = 0.25f;
        public readonly static object SYNC_TARGET = new object();

        public static Vector3 GridPosToPosition(Vector3Int gridPos)
        {
            return new Vector3((gridPos.x + gridPos.z / 2f) * HORIZONTAL_DISTANCE, gridPos.y * HEIGHT, gridPos.z * VERTICAL_DISTANCE);
        }

        public static Vector3 GridIndexToPosition(Vector2Int gridIndex, int gridHeight)
        {
            return new Vector3((gridIndex.x + gridIndex.y / 2f) * HORIZONTAL_DISTANCE, HEIGHT * gridHeight, gridIndex.y * VERTICAL_DISTANCE);
        }

        public static Vector3Int GridIndexToGridPos(Vector2Int gridIndex, int height)
        {
            return new Vector3Int(gridIndex.x, height, gridIndex.y);
        }

        public static Vector2Int PositionToGridIndex(Vector3 position)
        {
            int gridIndexY = Mathf.RoundToInt(position.z / VERTICAL_DISTANCE);
            int gridIndexX = Mathf.RoundToInt(position.x / HORIZONTAL_DISTANCE - gridIndexY / 2f);

            return new Vector2Int(gridIndexX, gridIndexY);
        }

        public static Vector2Int GridPosToGridIndex(Vector3Int gridPos)
        {
            return new Vector2Int(gridPos.x, gridPos.z);
        }

        public static Vector3Int PositionToGridPos(Vector3 position)
        {
            Vector2Int gridIndex = PositionToGridIndex(position);
            int gridHeight = Mathf.RoundToInt(position.y / HEIGHT);

            return new Vector3Int(gridIndex.x, gridHeight, gridIndex.y);
        }

        public static Vector2Int GridIndexToGridRegion(Vector2Int index, int regionScale = 1)
        {
            if(regionScale == 0)
                return index;
            float x = (0.5f + (3 * index.x + index.y) / 7f);
            float y = (0.5f + (2 * index.y - index.x) / 7f);
            Vector2Int region = new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
            return GridIndexToGridRegion(region, regionScale - 1);
        }

        public static bool IsGridIndexCenter(Vector2Int index, int regionSize)
        {   
            return index.x % regionSize == 0 && index.y % regionSize == 0;
        }

        public static float FindFastestTravelTime(Vector3Int start, Vector3Int end, MovementSpeed speed)
        {
            return FindDistance(start, end) / speed.FastestHorizontal + FindVerticalDistance(start.y, end.y) / speed.FastestVertical;
        }

        public static int FindDistance(Vector3Int start, Vector3Int end)
        {
            return FindHorizontalDistance(start, end) + FindVerticalDistance(start.y, end.y);
        }

        public static int FindHorizontalDistance(Vector3Int start, Vector3Int end)
        {
            int deltaX = end.x - start.x;
            int deltaZ = end.z - start.z;

            if ((deltaX > 0 && deltaZ > 0) || (deltaX < 0 && deltaZ < 0)) {
                return Mathf.Abs(deltaX) + Mathf.Abs(deltaZ);
            } else {
                return Math.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaZ));
            }
        }

        public static int FindDistance(Vector2Int start, Vector2Int end)
        {
            int deltaX = end.x - start.x;
            int deltaY = end.y - start.y;

            if ((deltaX > 0 && deltaY > 0) || (deltaX < 0 && deltaY < 0)) {
                return Mathf.Abs(deltaX) + Mathf.Abs(deltaY);
            } else {
                return Math.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaY));
            }
        }

        public static int FindVerticalDistance(int start, int end)
        {
            return Mathf.Abs(end - start);
        }


        public static IEnumerable<Vector2Int> GetRectangleIndicesAround(Vector2Int center, int width, int height)
        {
            List<Vector2Int> indices = new List<Vector2Int>();

            int halfWidth = width / 2;
            int halfHeight = height / 2;

            for (int y = -halfHeight; y <= halfHeight; y++) {
                for (int x = -halfWidth; x <= halfWidth; x++) {
                    indices.Add(new Vector2Int(center.x + x - y / 2, center.y + y));
                }
            }

            return indices;
        }

        public static IEnumerable<Vector2Int> GetBurstIndicesAround(Vector2Int center, int radius, bool fill)
        {
            List<Vector2Int> indices = new List<Vector2Int>();

            for (int y = -radius; y <= radius; y++) {
                for (int x = -radius; x <= radius; x++) {
                    int absoluteSum = Mathf.Abs(y + x);
                    if (absoluteSum > radius) { continue; } // Skip the corners to leave just a hex burst
                    if (fill || Mathf.Abs(x) == radius || Mathf.Abs(y) == radius || absoluteSum == radius) {
                        indices.Add(new Vector2Int(center.x + x, center.y + y));
                    }
                }
            }

            return indices;
        }

        public static IEnumerable<Vector2Int> GetIndicesAround(Vector2Int center, float rotation, int spread, int reach)
        {
            List<Vector2Int> indices = new List<Vector2Int>();

            foreach (Vector2Int gridIndex in GetBurstIndicesAround(center, reach, true)) {
                float angle = FacingUtil.GetOffsetAngle(center, gridIndex);
                if (angle > 0 && angle <= 60 && spread < 1) { continue; }
                if (angle > -60 && angle < 0 && spread < 2) { continue; }
                if (angle > 60 && angle <= 121 && spread < 3) { continue; }
                if (angle > -121 && angle <= -60 && spread < 4) { continue; }
                if ((angle > 121 || angle <= -121) && spread < 5) { continue; }

                Vector2Int rotated = FacingUtil.RotateAround(gridIndex, center, rotation);

                indices.Add(rotated);
            }
            return indices;
        }


        public static async void FindShortestPathAsync(uint moverId, LocationComponent moverLocation, Vector3Int end, Action<List<NavStep>> callback)
        {
            await Task.Run(() => callback(FindShortestPath(moverId, moverLocation, end, out _)));
        }

        public static List<NavStep> FindShortestPath(uint moverId, LocationComponent moverLocation, Vector3Int end, out int loopCount)
        {
            NavStep start = new NavStep(MoveType.NONE, new WorldLocation(moverLocation.GridPosition, moverLocation.Facing), WorldScheduler.Instance.Time);
            MovementSpeed movementSpeed = moverLocation.Speed;

            loopCount = 0;
            if (start == null) { return null; }
            if (start.Location.GridPosition == end) { return null; }

            DateTime startTime = DateTime.Now;
            DateTime endTime;

            float startBestCaseScore = FindFastestTravelTime(start.Location.GridPosition, end, movementSpeed);
            NavStep lastStep = null;
            OrderedList<NavStep> openSet = new OrderedList<NavStep>();
            openSet.Add(startBestCaseScore, start);
            Dictionary<NavStep, NavStep> cameFrom = new Dictionary<NavStep, NavStep>();
            Dictionary<NavStep, float> knownCostToStep = new Dictionary<NavStep, float>() { { start, 0 } };
            Dictionary<NavStep, float> bestCaseScore = new Dictionary<NavStep, float>() { { start, startBestCaseScore } };
            float pessimisticBestScore = PESSIMIST_PATHING_MULTIPLIER * startBestCaseScore;


            while (openSet.Count > 0) {
                // Timeout when we get stuck here too long 
                if (loopCount++ > 100000) { break; }

                // Grab the most promising hex
                NavStep currentStep = openSet.First;
                openSet.RemoveAt(0);

                // If the most promising hex seems significantly worse than known shortest path, stop looking
                if (bestCaseScore[currentStep] > pessimisticBestScore) { break; }

                // Mark progress if found, but finish processing candidates
                float pessimisticCurrentScore = knownCostToStep[currentStep] + PESSIMIST_PATHING_MULTIPLIER * FindFastestTravelTime(currentStep.Location.GridPosition, end, movementSpeed);
                if (pessimisticCurrentScore < pessimisticBestScore) {
                    lastStep = currentStep;
                    pessimisticBestScore = pessimisticCurrentScore;
                }

                // Process all neighbors of the most promising hex
                float costToCurrentStep = knownCostToStep[currentStep];
                foreach (NavStep nextStep in FindNeighbors(currentStep, movementSpeed, moverId)) {
                    float costToNeighbor = costToCurrentStep + CalcAdjacentTravelCost(currentStep, nextStep, movementSpeed);
                    if (!knownCostToStep.ContainsKey(nextStep) || costToNeighbor < knownCostToStep[nextStep]) {
                        cameFrom[nextStep] = currentStep;
                        knownCostToStep[nextStep] = costToNeighbor;
                        bestCaseScore[nextStep] = costToNeighbor + FindFastestTravelTime(nextStep.Location.GridPosition, end, movementSpeed);
                        if (costToNeighbor < pessimisticBestScore) {
                            openSet.Add(pessimisticBestScore, nextStep);
                        }
                    }
                }
            }

            endTime = DateTime.Now;
            if (lastStep == null) {
                Debug.Log($"Pathfinding FAILED ({start.Location.GridPosition} -> {end}) after {(endTime - startTime).TotalMilliseconds}ms and {loopCount} attempts");
                return null;
            }

            //Debug.Log($"Pathfinding complete after {(endTime - startTime).TotalMilliseconds}ms and {loopCount} attempts");

            return BuildPath(cameFrom, start, lastStep);
        }

        public static NavStep GetNavDrop(WorldLocation unitLocation, WorldLocation tileLocation, WorldLocation forwardLocation, MovementSpeed speed, float time)
        {
            int unitHeight = unitLocation.GridHeight;
            int tileHeight = tileLocation.GridHeight;
            int forwardHeight = forwardLocation.GridHeight;

            if (unitHeight > tileHeight && unitHeight > forwardHeight) {
                int dropHeight = unitHeight - tileHeight;
                return new NavStep(MoveType.DROP_DOWN, unitLocation.MoveDown(dropHeight), time + dropHeight / speed.Drop);
            }
            return null;
        }

        public static NavStep GetNavForward(WorldLocation unitLocation, WorldLocation tileLocation, WorldLocation forwardLocation, MovementSpeed speed, float time) 
        {
            int unitHeight = unitLocation.GridHeight;
            int tileHeight = tileLocation.GridHeight;
            int forwardHeight = forwardLocation.GridHeight;

            bool onSolidGround = unitHeight == tileHeight;

            if (forwardLocation == null) { return null; }

            // walk if able
            if (unitHeight == forwardHeight || onSolidGround && unitHeight >= forwardHeight) {
                return new NavStep(MoveType.WALK_FORWARD, forwardLocation, time + 1f / speed.Walk);
            }

            // climb if able
            if (unitHeight < forwardHeight) {
                if (onSolidGround && speed.MaxJump > 0) { // jump
                    int jumpHeight = Mathf.Min(forwardHeight - unitHeight, speed.MaxJump);
                    return new NavStep(MoveType.JUMP_UP, unitLocation.MoveUp(jumpHeight), time + jumpHeight / speed.Jump);
                } else { // climb
                    return new NavStep(MoveType.CLIMB_UP, unitLocation.MoveUp(1), time + 1f / speed.Climb);
                }
            }

            return null;
        }

        public static NavStep GetNavBackward(WorldLocation unitLocation, WorldLocation tileLocation, WorldLocation forwardLocation, WorldLocation backwardLocation, MovementSpeed speed, float time)
        {
            int unitHeight = unitLocation.GridHeight;
            int tileHeight = tileLocation.GridHeight;
            int forwardHeight = forwardLocation.GridHeight;
            int backwardHeight = backwardLocation.GridHeight;

            bool onSolidGround = unitHeight == tileHeight;

            // walk if able
            if (onSolidGround && unitHeight >= backwardHeight) {
                return new NavStep(MoveType.WALK_BACKWARD, backwardLocation, time + 1f / speed.Backstep);
            }

            // climb if able
            if (!onSolidGround && unitHeight <= forwardHeight) {
                int dropHeight = unitHeight - tileHeight;
                if (dropHeight <= speed.MaxDrop) { // drop
                    return new NavStep(MoveType.DROP_DOWN, unitLocation.MoveDown(dropHeight), time + dropHeight / speed.Drop);
                } else { // climb
                    return new NavStep(MoveType.CLIMB_DOWN, unitLocation.MoveDown(1), time + 1f / speed.Climb);
                }
            }

            return null;
        }

        public static NavStep GetNavLeft(WorldLocation unitLocation, WorldLocation tileLocation, MovementSpeed speed, float time)
        {
            int unitHeight = unitLocation.GridHeight;
            int tileHeight = tileLocation.GridHeight;

            if (unitHeight == tileHeight) {
                return new NavStep(MoveType.TURN_LEFT, unitLocation.RotateLeft(), time + 1 / speed.Drop);
            }

            return null;
        }

        public static NavStep GetNavRight(WorldLocation unitLocation, WorldLocation tileLocation, MovementSpeed speed, float time)
        {
            int unitHeight = unitLocation.GridHeight;
            int tileHeight = tileLocation.GridHeight;

            if (unitHeight == tileHeight) {
                return new NavStep(MoveType.TURN_RIGHT, unitLocation.RotateRight(), time + 1 / speed.Drop);
            }

            return null;
        }

        public static List<NavStep> FindNeighbors(NavStep currentStep, MovementSpeed movementSpeed, uint moverId)
        {
            List<NavStep> neighbors = new List<NavStep>();
            FindNeighbors(moverId, currentStep.Location, movementSpeed, currentStep.WorldTime, out NavStep forcedStep, out NavStep forwardStep, out NavStep backwardStep, out NavStep leftStep, out NavStep rightStep);
            if (forcedStep != null) { neighbors.Add(forcedStep); }
            if (forwardStep != null) { neighbors.Add(forwardStep); }
            if (backwardStep != null) { neighbors.Add(backwardStep); }
            if (leftStep != null) { neighbors.Add(leftStep); }
            if (rightStep != null) { neighbors.Add(rightStep); }
            return neighbors;
        }

        public static void FindNeighbors(uint moverId, WorldLocation location, MovementSpeed speed, float time, out NavStep forcedStep, out NavStep forwardStep, out NavStep backwardStep, out NavStep leftStep, out NavStep rightStep)
        {
            Vector2Int facing = location.Facing;
            Vector3Int? tilePosition = HexWorld.Instance.GetFirstAt(location.GridIndex, WorldObjectType.Tile, time)?.Location.GridPosition;
            Vector3Int? forwardPosition = HexWorld.Instance.GetFirstAt(location.GridIndex + facing, WorldObjectType.Tile, time)?.Location.GridPosition;
            Vector3Int? backwardPosition = HexWorld.Instance.GetFirstAt(location.GridIndex - facing, WorldObjectType.Tile, time)?.Location.GridPosition;

            WorldLocation tileLocation = tilePosition.HasValue ? new WorldLocation(tilePosition.Value, facing) : null;
            WorldLocation forwardLocation = forwardPosition.HasValue ? new WorldLocation(forwardPosition.Value, facing) : null;
            WorldLocation backwardLocation = backwardPosition.HasValue ? new WorldLocation(backwardPosition.Value, facing) : null;

            NavStep dropStep = GetNavDrop(location, tileLocation, forwardLocation, speed, time);
            if (dropStep != null) {
                forcedStep = dropStep;
                forwardStep = backwardStep = leftStep = rightStep = null;
            } else {
                forcedStep = null;
                forwardStep = NullIfObstructed(GetNavForward(location, tileLocation, forwardLocation, speed, time), moverId);
                backwardStep = NullIfObstructed(GetNavBackward(location, tileLocation, forwardLocation, backwardLocation, speed, time), moverId);
                leftStep = NullIfObstructed(GetNavLeft(location, tileLocation, speed, time), moverId);
                rightStep = NullIfObstructed(GetNavRight(location, tileLocation, speed, time), moverId);
            }
        }

        public static NavStep NullIfObstructed(NavStep step, uint moverId)
        {
            if (step == null) { return null; }
            ICollection<WorldObject> entities = HexWorld.Instance.GetAt(step.Location.GridIndex, WorldObjectType.Entity, step.WorldTime);
            foreach (WorldObject entity in entities) {
                if (entity.Id != moverId) {
                    return null;
                }
            }
            return step;
        }

        private static List<NavStep> BuildPath(Dictionary<NavStep, NavStep> cameFrom, NavStep start, NavStep end)
        {
            List<NavStep> path = new List<NavStep>() { end };
            NavStep fromPos = end;
            while (!fromPos.Equals(start)) {
                fromPos = cameFrom[fromPos];
                path.Insert(0, fromPos);
            }

            return path;
        }

        private static void PrintCostPath(float total, MovementSpeed movementSpeed, Dictionary<NavStep, NavStep> cameFrom, NavStep start, NavStep end)
        {
            string travelCostString = "Total travel cost: " + total + "\n";
            NavStep fromPos = end;
            while (!fromPos.Equals(start)) {
                travelCostString += "+" + CalcAdjacentTravelCost(cameFrom[fromPos], fromPos, movementSpeed) + ": " + fromPos + ": " + "\n";
                fromPos = cameFrom[fromPos];
            }
            Debug.Log(string.Format(travelCostString));
        }

        public static float CalcAdjacentTravelCost(NavStep start, NavStep end, MovementSpeed movementSpeed)
        {
            if (end.MoveType == MoveType.WALK_FORWARD) { return 1f / movementSpeed.Walk; }
            if (end.MoveType == MoveType.WALK_BACKWARD) { return 1f / movementSpeed.Backstep; }
            if (end.MoveType == MoveType.TURN_LEFT) { return 1f / movementSpeed.Rotate; }
            if (end.MoveType == MoveType.TURN_RIGHT) { return 1f / movementSpeed.Rotate; }
            if (end.MoveType == MoveType.NONE) { return 1f / movementSpeed.Wait; }

            int heightDelta = end.Location.GridHeight - start.Location.GridHeight;
            if (end.MoveType == MoveType.JUMP_UP) { return heightDelta / movementSpeed.Jump; }
            if (end.MoveType == MoveType.CLIMB_UP) { return heightDelta / movementSpeed.Climb; }
            if (end.MoveType == MoveType.DROP_DOWN) { return -heightDelta / movementSpeed.Drop; }
            if (end.MoveType == MoveType.CLIMB_DOWN) { return -heightDelta / movementSpeed.Climb; }

            return -1;
        }
    }
}
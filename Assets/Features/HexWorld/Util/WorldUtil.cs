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
        public const float HEIGHT = 0.25f;

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

        public static int FindDistance(Vector3Int start, Vector3Int end)
        {
            int deltaX = end.x - start.x;
            int deltaY = end.y - start.y;
            int deltaZ = end.z - start.z;

            if ((deltaX > 0 && deltaZ > 0) || (deltaX < 0 && deltaZ < 0)) {
                return Mathf.Abs(deltaX) + Mathf.Abs(deltaZ) + Mathf.Abs(deltaY);
            } else {
                return Math.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaZ)) + Mathf.Abs(deltaY);
            }
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

        public static async void FindShortestPathAsync(NavStep start, Vector3Int end, MovementSpeed movementSpeed, Action<List<NavStep>> callback)
        {
            await Task.Run(() => callback(FindShortestPath(start, end, movementSpeed, out _)));
        }

        public static List<NavStep> FindShortestPath(NavStep start, Vector3Int end, MovementSpeed movementSpeed, out int loopCount)
        {
            loopCount = 0;
            if (start == null) { return null; }
            if (start.GridPos == end) { return null; }

            DateTime startTime = DateTime.Now;
            DateTime endTime;

            float knownBestScore = float.MaxValue;
            NavStep lastStep = null;
            OrderedList<NavStep> openSet = new OrderedList<NavStep>();
            int shortestKnownDistanceToEnd = FindDistance(start.GridPos, end);
            openSet.Add(shortestKnownDistanceToEnd, start);
            Dictionary<NavStep, NavStep> cameFrom = new Dictionary<NavStep, NavStep>();
            Dictionary<NavStep, float> knownCostToStep = new Dictionary<NavStep, float>() { { start, 0 } };
            Dictionary<NavStep, float> estimatedBestScore = new Dictionary<NavStep, float>() { { start, FindDistance(start.GridPos, end) } };

            while (openSet.Count > 0) {
                // Timeout when we get stuck here too long 
                if (loopCount++ > 100000) { break; }

                // Grab the most promising hex
                NavStep currentStep = openSet.First;
                openSet.RemoveAt(0);

                // If the most promising hex seems worse than known shortest path, stop looking
                if (estimatedBestScore[currentStep] > knownBestScore) { break; }

                // Mark progress if found, but finish processing candidates
                int currentDistanceToEnd = FindDistance(currentStep.GridPos, end);
                if (currentDistanceToEnd < shortestKnownDistanceToEnd) {
                    lastStep = currentStep;
                    shortestKnownDistanceToEnd = currentDistanceToEnd;
                    float currentScore = knownCostToStep[currentStep] + 10 * shortestKnownDistanceToEnd;
                    if (currentScore < knownBestScore)
                        knownBestScore = currentScore;
                }

                // Process all neighbors of the most promising hex
                float costToCurrentStep = knownCostToStep[currentStep];
                foreach (NavStep nextStep in FindNeighbors(currentStep, movementSpeed)) {
                    float costToNeighbor = costToCurrentStep + CalcAdjacentTravelCost(currentStep, nextStep, movementSpeed);
                    if (!knownCostToStep.ContainsKey(nextStep) || costToNeighbor < knownCostToStep[nextStep]) {
                        cameFrom[nextStep] = currentStep;
                        knownCostToStep[nextStep] = costToNeighbor;
                        estimatedBestScore[nextStep] = costToNeighbor + FindDistance(nextStep.GridPos, end) / movementSpeed.FastestHorizontal;
                        if (costToNeighbor < knownBestScore) { openSet.Add(estimatedBestScore[nextStep], nextStep); }
                    }
                }
            }

            endTime = DateTime.Now;
            if (lastStep == null) {
                Debug.Log($"Pathfinding FAILED ({start.GridPos} -> {end}) after {(endTime - startTime).TotalMilliseconds}ms and {loopCount} attempts");
                return null;
            }

            //Debug.Log($"Pathfinding complete after {(endTime - startTime).TotalMilliseconds}ms and {loopCount} attempts");

            return BuildPath(cameFrom, start, lastStep);
        }

        private static List<NavStep> FindNeighbors(NavStep currentStep, MovementSpeed movementSpeed)
        {
            List<NavStep> neighbors = new List<NavStep>();

            LocationComponent currentTile = (LocationComponent)(HexWorld.Instance.GetFirstAt(GridPosToGridIndex(currentStep.GridPos), WorldComponentType.Platform)?.GetComponent(WorldComponentType.Location));
            Vector2Int forwardGridIndex = currentStep.GridIndex + currentStep.Facing;
            Vector2Int backwardGridIndex = currentStep.GridIndex - currentStep.Facing;
            LocationComponent forwardTile = (LocationComponent)(HexWorld.Instance.GetFirstAt(forwardGridIndex, WorldComponentType.Platform)?.GetComponent(WorldComponentType.Location));
            LocationComponent backwardTile = (LocationComponent)(HexWorld.Instance.GetFirstAt(backwardGridIndex, WorldComponentType.Platform)?.GetComponent(WorldComponentType.Location));

            // waiting is always an option
            TryAddStep(new NavStep(MoveType.NONE, currentStep.GridPos, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Wait), ref neighbors);

            // walk if path forward is unobstructed
            if (forwardTile != null && currentStep.Height >= forwardTile.GridPosition.y) {
                TryAddStep(new NavStep(MoveType.WALK_FORWARD, forwardGridIndex, currentStep.Height, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Walk), ref neighbors);
            }

            // take a step back if path is unobstructed
            if (backwardTile != null && currentStep.Height >= backwardTile.GridPosition.y) {
                TryAddStep(new NavStep(MoveType.WALK_BACKWARD, backwardGridIndex, currentStep.Height, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Backstep), ref neighbors);
            }

            // rotate if on solid ground
            if (currentTile.GridPosition.y == currentStep.Height) {
                TryAddStep(new NavStep(MoveType.TURN_LEFT, currentStep.GridPos, FacingUtil.GetLeft(currentStep.Facing), currentStep.WorldTime + 1f / movementSpeed.Rotate), ref neighbors);
                TryAddStep(new NavStep(MoveType.TURN_RIGHT, currentStep.GridPos, FacingUtil.GetRight(currentStep.Facing), currentStep.WorldTime + 1f / movementSpeed.Rotate), ref neighbors);
            }

            // climb/drop down if not on solid ground
            if (currentTile.GridPosition.y < currentStep.Height) {
                if (currentStep.Height - currentTile.GridPosition.y <= movementSpeed.MaxDrop) { // drop
                    TryAddStep(new NavStep(MoveType.DROP_DOWN, currentStep.GridIndex, currentTile.GridPosition.y, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Drop), ref neighbors);
                } else { // climb
                    TryAddStep(new NavStep(MoveType.CLIMB_DOWN, currentStep.GridIndex, currentTile.GridPosition.y + movementSpeed.MaxDrop, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Climb), ref neighbors);
                }
            }

            // climb/jump up if path forward is higher
            if (forwardTile != null && currentStep.Height < forwardTile.GridPosition.y) {
                if (currentStep.Height == currentTile.GridPosition.y && movementSpeed.MaxJump > 0) { // jump
                    int maxJump = currentStep.Height + movementSpeed.MaxJump;
                    TryAddStep(new NavStep(MoveType.JUMP_UP, currentStep.GridIndex, maxJump >= forwardTile.GridPosition.y ? forwardTile.GridPosition.y : maxJump, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Jump), ref neighbors);
                } else { // climb
                    TryAddStep(new NavStep(MoveType.CLIMB_UP, currentStep.GridIndex, forwardTile.GridPosition.y, currentStep.Facing, currentStep.WorldTime + 1f / movementSpeed.Climb), ref neighbors);
                }
            }

            return neighbors;
        }

        private static void TryAddStep(NavStep step, ref List<NavStep> neighbors)
        {
            //TODO: compare entity position at step time instead of current
            if (!HexWorld.Instance.IsOccupied(step.GridIndex, WorldObjectType.Entity)) {
                neighbors.Add(step);
            }
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

            int heightDelta = end.Height - start.Height;
            if (end.MoveType == MoveType.JUMP_UP) { return heightDelta / movementSpeed.Jump; }
            if (end.MoveType == MoveType.CLIMB_UP) { return heightDelta / movementSpeed.Climb; }
            if (end.MoveType == MoveType.DROP_DOWN) { return -heightDelta / movementSpeed.Drop; }
            if (end.MoveType == MoveType.CLIMB_DOWN) { return -heightDelta / movementSpeed.Climb; }

            return -1;
        }
    }
}
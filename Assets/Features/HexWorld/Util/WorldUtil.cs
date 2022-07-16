﻿using System;
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

        public static List<NavStep> FindNeighbors(NavStep currentStep, MovementSpeed movementSpeed, uint moverId)
        {
            List<NavStep> neighbors = new List<NavStep>();

            LocationComponent currentTileLocation = (LocationComponent)(HexWorld.Instance.GetFirstAt(
                currentStep.Location.GridIndex, 
                WorldObjectType.Tile, 
                currentStep.WorldTime)?.GetComponent(WorldComponentType.Location));
            LocationComponent forwardTileLocation = (LocationComponent)HexWorld.Instance.GetFirstAt(
                currentStep.Location.GridIndex + currentStep.Location.Facing, 
                WorldObjectType.Tile, 
                currentStep.WorldTime)?.GetComponent(WorldComponentType.Location);
            LocationComponent backwardTileLocation = (LocationComponent)HexWorld.Instance.GetFirstAt(
                currentStep.Location.GridIndex - currentStep.Location.Facing, 
                WorldObjectType.Tile, 
                currentStep.WorldTime)?.GetComponent(WorldComponentType.Location);
            bool onSolidGround = currentTileLocation.GridPosition.y == currentStep.Location.GridPosition.y;

            // waiting is always an option
            //TryAddStep(new NavStep(MoveType.NONE, currentStep.Location, currentStep.WorldTime + 1f / movementSpeed.Wait), ref neighbors);

            // walk if path forward is unobstructed
            if (forwardTileLocation != null && currentStep.Location.GridPosition.y >= forwardTileLocation.GridPosition.y) {
                TryAddStep(new NavStep(MoveType.WALK_FORWARD, currentStep.Location.MoveForward(1), currentStep.WorldTime + 1f / movementSpeed.Walk), ref neighbors, moverId);
            }

            // take a step back if path is unobstructed
            if (backwardTileLocation != null && currentStep.Location.GridPosition.y >= backwardTileLocation.GridPosition.y) {
                TryAddStep(new NavStep(MoveType.WALK_BACKWARD, currentStep.Location.MoveBack(1), currentStep.WorldTime + 1f / movementSpeed.Backstep), ref neighbors, moverId);
            }

            // rotate if on solid ground
            if (onSolidGround) {
                TryAddStep(new NavStep(MoveType.TURN_LEFT, currentStep.Location.RotateLeft(), currentStep.WorldTime + 1f / movementSpeed.Rotate), ref neighbors, moverId);
                TryAddStep(new NavStep(MoveType.TURN_RIGHT, currentStep.Location.RotateRight(), currentStep.WorldTime + 1f / movementSpeed.Rotate), ref neighbors, moverId);
            }

            // climb/drop down if not on solid ground
            if (!onSolidGround) {
                int dropHeight = currentStep.Location.GridHeight - currentTileLocation.GridHeight;
                if (dropHeight <= movementSpeed.MaxDrop) { // drop
                    TryAddStep(new NavStep(MoveType.DROP_DOWN, currentStep.Location.MoveDown(dropHeight), currentStep.WorldTime + 1f / movementSpeed.Drop), ref neighbors, moverId);
                } else { // climb
                    TryAddStep(new NavStep(MoveType.CLIMB_DOWN, currentStep.Location.MoveDown(1), currentStep.WorldTime + 1f / movementSpeed.Climb), ref neighbors, moverId);
                }
            }

            // climb/jump up if path forward is higher
            if (forwardTileLocation != null && currentStep.Location.GridHeight < forwardTileLocation.GridHeight) {
                if (currentStep.Location.GridHeight == currentTileLocation.GridHeight && movementSpeed.MaxJump > 0) { // jump
                    int jumpHeight = Mathf.Min(forwardTileLocation.GridHeight - currentStep.Location.GridHeight, movementSpeed.MaxJump);
                    TryAddStep(new NavStep(MoveType.JUMP_UP, currentStep.Location.MoveUp(jumpHeight), currentStep.WorldTime + 1f / movementSpeed.Jump), ref neighbors, moverId);
                } else { // climb
                    TryAddStep(new NavStep(MoveType.CLIMB_UP, currentStep.Location.MoveUp(1), currentStep.WorldTime + 1f / movementSpeed.Climb), ref neighbors, moverId);
                }
            }

            return neighbors;
        }

        private static void TryAddStep(NavStep step, ref List<NavStep> neighbors, uint moverId)
        {
            ICollection<WorldObject> entities = HexWorld.Instance.GetAt(step.Location.GridIndex, WorldObjectType.Entity, step.WorldTime);
            foreach (WorldObject entity in entities) {
                if (entity.Id != moverId) {
                    return;
                }
            }
            neighbors.Add(step);
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
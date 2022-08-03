using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Zekzek.HexWorld;

public class PathingTest
{
    [SetUp]
    public void ClearWorld() {
        HexWorld.Instance.Clear();
        WorldScheduler.Instance.Clear();
        GenerationUtil.Init(0, 10, TerrainType.Flat);
        foreach (Vector2Int index in WorldUtil.GetBurstIndicesAround(Vector2Int.zero, 10, true)) {
            GenerationUtil.InstantiateAtGridIndex(index.x, index.y);
        }
    }

    [Test]
    public void DistanceEquality() {
        Assert.AreEqual(0, WorldUtil.FindDistance(Vector3Int.zero, Vector3Int.zero));
    }

    [Test]
    public void Distance1Step() {
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, 0, 0)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, 0, 1)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-1, 0, 1)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-1, 0, 0)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, 0, -1)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, 0, -1)));
    }

    [Test]
    public void Distance2Steps() {
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(2, 0, 0)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, 0, 1)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, 0, 2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-1, 0, 2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-2, 0, 2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-2, 0, 1)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-2, 0, 0)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(-1, 0, -1)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, 0, -2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, 0, -2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(2, 0, -2)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(2, 0, -1)));
    }

    [Test]
    public void DistanceVertical() {
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, 1, 0)));
        Assert.AreEqual(1, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(0, -1, 0)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, 1, 0)));
        Assert.AreEqual(2, WorldUtil.FindDistance(Vector3Int.zero, new Vector3Int(1, -1, 0)));
    }

    [Test]
    public void Move()
    {
        WorldLocation location = new WorldLocation(new Vector3Int(0, 5, 0), FacingUtil.E);

        Assert.AreEqual(new Vector2Int(1, 0), location.MoveForward(1).GridIndex, "Move forward grid index");
        Assert.AreEqual(new Vector3Int(1, 5, 0), location.MoveForward(1).GridPosition, "Move forward grid position");
        Assert.AreEqual(FacingUtil.E, location.MoveForward(1).Facing, "Move forward facing");

        Assert.AreEqual(new Vector2Int(-1, 0), location.MoveBack(1).GridIndex, "Move backward grid index");
        Assert.AreEqual(new Vector3Int(-1, 5, 0), location.MoveBack(1).GridPosition, "Move backward grid position");
        Assert.AreEqual(FacingUtil.E, location.MoveForward(1).Facing, "Move backward facing");

        Assert.AreEqual(new Vector2Int(0, 0), location.MoveUp(1).GridIndex, "Move up grid index");
        Assert.AreEqual(new Vector3Int(0, 6, 0), location.MoveUp(1).GridPosition, "Move up grid position");
        Assert.AreEqual(FacingUtil.E, location.MoveUp(1).Facing, "Move up facing");

        Assert.AreEqual(new Vector2Int(0, 0), location.MoveDown(1).GridIndex, "Move down grid index");
        Assert.AreEqual(new Vector3Int(0, 4, 0), location.MoveDown(1).GridPosition, "Move down grid position");
        Assert.AreEqual(FacingUtil.E, location.MoveDown(1).Facing, "Move down facing");

        WorldLocation location2 = new WorldLocation(new Vector3Int(0, 5, 0), FacingUtil.W);

        Assert.AreEqual(new Vector2Int(-1, 0), location2.MoveForward(1).GridIndex, "Move forward grid index");
        Assert.AreEqual(new Vector3Int(-1, 5, 0), location2.MoveForward(1).GridPosition, "Move forward grid position");
        Assert.AreEqual(FacingUtil.W, location2.MoveForward(1).Facing, "Move forward facing");

        Assert.AreEqual(new Vector2Int(1, 0), location2.MoveBack(1).GridIndex, "Move backward grid index");
        Assert.AreEqual(new Vector3Int(1, 5, 0), location2.MoveBack(1).GridPosition, "Move backward grid position");
        Assert.AreEqual(FacingUtil.W, location2.MoveForward(1).Facing, "Move backward facing");

        Assert.AreEqual(new Vector2Int(0, 0), location2.MoveUp(1).GridIndex, "Move up grid index");
        Assert.AreEqual(new Vector3Int(0, 6, 0), location2.MoveUp(1).GridPosition, "Move up grid position");
        Assert.AreEqual(FacingUtil.W, location2.MoveUp(1).Facing, "Move up facing");

        Assert.AreEqual(new Vector2Int(0, 0), location2.MoveDown(1).GridIndex, "Move down grid index");
        Assert.AreEqual(new Vector3Int(0, 4, 0), location2.MoveDown(1).GridPosition, "Move down grid position");
        Assert.AreEqual(FacingUtil.W, location2.MoveDown(1).Facing, "Move down facing");
    }

    [Test]
    public void Rotate()
    {
        WorldLocation location = new WorldLocation(new Vector3Int(0, 5, 0), FacingUtil.E);

        Assert.AreEqual(new Vector2Int(0, 0), location.RotateLeft().GridIndex, "Rotate left grid index");
        Assert.AreEqual(new Vector3Int(0, 5, 0), location.RotateLeft().GridPosition, "Rotate left grid position");
        Assert.AreEqual(FacingUtil.NE, location.RotateLeft().Facing, "Rotate left facing");

        Assert.AreEqual(new Vector2Int(0, 0), location.RotateRight().GridIndex, "Rotate right grid index");
        Assert.AreEqual(new Vector3Int(0, 5, 0), location.RotateRight().GridPosition, "Rotate right grid position");
        Assert.AreEqual(FacingUtil.SE, location.RotateRight().Facing, "Rotate right facing");
    }

    [Test]
    public void WalkSpeed() {
        MovementSpeed speed = new MovementSpeed(2, 0, 0, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.E), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(1,0,0), FacingUtil.E), 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NE), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(0, 0, 1), FacingUtil.NE), 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NW), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(-1, 0, 1), FacingUtil.NW), 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.W), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(-1, 0, 0), FacingUtil.W), 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SW), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(0, 0, -1), FacingUtil.SW), 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SE), 0),
            new NavStep(MoveType.WALK_FORWARD, new WorldLocation(new Vector3Int(1, 0, -1), FacingUtil.SE), 0),
            speed));
    }

    [Test]
    public void TurnSpeedRight() {
        MovementSpeed speed = new MovementSpeed(0, 0, 2, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.E), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.SE), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SE), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.SW), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SW), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.W), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.W), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.NW), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NW), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.NE), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NE), 0),
            new NavStep(MoveType.TURN_RIGHT, new WorldLocation(Vector3Int.zero, FacingUtil.E), 0),
            speed));
    }

    [Test]
    public void TurnSpeedLeft() {
        MovementSpeed speed = new MovementSpeed(0, 0, 2, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.E), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.NE), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NE), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.NW), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.NW), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.W), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.W), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.SW), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SW), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.SE), 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, new WorldLocation(Vector3Int.zero, FacingUtil.SE), 0),
            new NavStep(MoveType.TURN_LEFT, new WorldLocation(Vector3Int.zero, FacingUtil.E), 0),
            speed));
    }

    [Test]
    public void Neighbors()
    {
        float startTime = WorldScheduler.Instance.Time;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        WorldObject entity = GenerationUtil.InstantiateEntity(speed, Vector2Int.zero, FacingUtil.E);
        Debug.Log($"{entity.Location.GridPosition} & {entity.Location.Facing}");
        List<NavStep> neighbors = WorldUtil.FindNeighbors(new NavStep(MoveType.NONE, entity.Location.Current, startTime), speed, entity.Id);
        Assert.AreEqual(4, neighbors.Count);

        foreach (NavStep neighbor in neighbors) {
            if (neighbor.MoveType == MoveType.WALK_FORWARD) { 
                Assert.AreEqual(FacingUtil.E, neighbor.Location.GridIndex);
                Assert.AreEqual(FacingUtil.E, neighbor.Location.Facing); 
            } else if (neighbor.MoveType == MoveType.WALK_BACKWARD) {
                Assert.AreEqual(FacingUtil.W, neighbor.Location.GridIndex);
                Assert.AreEqual(FacingUtil.E, neighbor.Location.Facing);
            } else if (neighbor.MoveType == MoveType.TURN_LEFT) {
                Assert.AreEqual(Vector2Int.zero, neighbor.Location.GridIndex);
                Assert.AreEqual(FacingUtil.NE, neighbor.Location.Facing);
            } else if (neighbor.MoveType == MoveType.TURN_RIGHT) {
                Assert.AreEqual(Vector2Int.zero, neighbor.Location.GridIndex);
                Assert.AreEqual(FacingUtil.SE, neighbor.Location.Facing);
            }
        }
    }


    [Test]
    public void PathingStraightEastLine() {
        float startTime = WorldScheduler.Instance.Time;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        Vector2Int position1 = new Vector2Int(0, 2);
        Vector2Int position2 = new Vector2Int(5, 2);
        WorldObject player = GenerationUtil.InstantiateEntity(speed, position1, FacingUtil.E);

        Debug.Log($"Find path from {player.Location.GridPosition} to {position2}");
        List<NavStep> path = WorldUtil.FindShortestPath(player.Id, player.Location, new Vector3Int(position2.x, player.Location.GridHeight, position2.y), out int loopCount);

        Assert.AreEqual(6, path.Count, "Path length");
        for (int i = 0; i < path.Count; i++) {
            Assert.AreEqual(FacingUtil.E, path[i].Location.Facing);
            Assert.AreEqual(i, path[i].Location.GridPosition.x);
            Assert.AreEqual(player.Location.GridHeight, path[i].Location.GridPosition.y);
            Assert.AreEqual(2, path[i].Location.GridPosition.z);
            Assert.AreEqual(startTime + i, path[i].WorldTime);
            if (i > 0) {
                Assert.AreEqual(MoveType.WALK_FORWARD, path[i].MoveType);
            }
        }
        Assert.LessOrEqual(loopCount, 7, "Search steps");
    }

    [Test]
    public void PathingStraightWestLine()
    {
        float startTime = WorldScheduler.Instance.Time;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        Vector2Int position1 = new Vector2Int(0, 2);
        Vector2Int position2 = new Vector2Int(5, 2);
        WorldObject player = GenerationUtil.InstantiateEntity(speed, position2, FacingUtil.W);
        Assert.AreEqual(player.Location.Facing, FacingUtil.W);

        Debug.Log($"Find path from {player.Location.GridPosition}({player.Location.Facing}) to {position1}");
        List<NavStep> path = WorldUtil.FindShortestPath(player.Id, player.Location, new Vector3Int(position1.x, player.Location.GridHeight, position1.y), out int loopCount);

        Assert.AreEqual(6, path.Count, "Path length");
        for (int i = 0; i < path.Count; i++) {
            Assert.AreEqual(FacingUtil.W, path[i].Location.Facing);
            Assert.AreEqual(5 - i, path[i].Location.GridPosition.x);
            Assert.AreEqual(player.Location.GridHeight, path[i].Location.GridPosition.y);
            Assert.AreEqual(2, path[i].Location.GridPosition.z);
            Assert.AreEqual(startTime + i, path[i].WorldTime);
            if (i > 0) {
                Assert.AreEqual(MoveType.WALK_FORWARD, path[i].MoveType);
            }
        }
        Assert.LessOrEqual(loopCount, 7, "Search steps");
    }

    [Test]
    public void PathingColliders() {
        float startTime = WorldScheduler.Instance.Time;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        
        WorldObject player = GenerationUtil.InstantiateEntity(speed, new Vector2Int(0, 2));
        WorldObject obstacle = GenerationUtil.InstantiateEntity(speed, new Vector2Int(3, 2));
        Vector3Int goal = new Vector3Int(5, player.Location.GridHeight, 2);

        Debug.Log($"Find path from {player.Location.GridPosition} to {goal} while avoiding {obstacle.Location.GridPosition}");
        List<NavStep> path = WorldUtil.FindShortestPath(player.Id, player.Location, goal, out int loopCount);

        //Assert.AreEqual(10, path.Count, "Path length");
        int walkCount = 0;
        int rotateCount = 0;
        for (int i = 0; i < path.Count; i++) {
            if (path[i].MoveType == MoveType.WALK_FORWARD) {
                walkCount++;
            } else if (path[i].MoveType == MoveType.TURN_LEFT || path[i].MoveType == MoveType.TURN_RIGHT) {
                rotateCount++;
            }
            Assert.AreEqual(startTime + i, path[i].WorldTime);
            Debug.Log($"{path[i].MoveType} -> {path[i].Location.GridPosition} ({path[i].Location.Facing})");
        }
        Assert.AreEqual(6, walkCount, "Walk steps");
        Assert.AreEqual(3, rotateCount, "Rotate steps");
        Assert.Less(loopCount, 65, "Search steps");
    }
}
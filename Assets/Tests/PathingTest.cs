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
    public void WalkSpeed() {
        MovementSpeed speed = new MovementSpeed(2, 0, 0, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.E, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(1,0,0), FacingUtil.E, 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NE, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(0, 0, 1), FacingUtil.NE, 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NW, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(-1, 0, 1), FacingUtil.NW, 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.W, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(-1, 0, 0), FacingUtil.W, 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SW, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(0, 0, -1), FacingUtil.SW, 0),
            speed));
        Assert.AreEqual(1 / speed.Walk, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SE, 0),
            new NavStep(MoveType.WALK_FORWARD, new Vector3Int(1, 0, -1), FacingUtil.SE, 0),
            speed));
    }

    [Test]
    public void TurnSpeedRight() {
        MovementSpeed speed = new MovementSpeed(0, 0, 2, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.E, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.SE, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SE, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.SW, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SW, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.W, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.W, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.NW, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NW, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.NE, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NE, 0),
            new NavStep(MoveType.TURN_RIGHT, Vector3Int.zero, FacingUtil.E, 0),
            speed));
    }

    [Test]
    public void TurnSpeedLeft() {
        MovementSpeed speed = new MovementSpeed(0, 0, 2, 0, 0, 0, 0, 0, 0);
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.E, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.NE, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NE, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.NW, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.NW, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.W, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.W, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.SW, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SW, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.SE, 0),
            speed));
        Assert.AreEqual(1 / speed.Rotate, WorldUtil.CalcAdjacentTravelCost(
            new NavStep(MoveType.NONE, Vector3Int.zero, FacingUtil.SE, 0),
            new NavStep(MoveType.TURN_LEFT, Vector3Int.zero, FacingUtil.E, 0),
            speed));
    }

    [Test]
    public void PathingStraightLine() {
        float startTime = 1;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        for (int x = 0; x <= 5; x++) {
            for (int z = 0; z <= 5; z++) {
                WorldObject.CreateTile(new Vector3Int(x, 0, z));
            }
        }

        NavStep start = new NavStep(MoveType.NONE, new Vector3Int(0, 0, 2), FacingUtil.E, startTime);

        List<NavStep> path = WorldUtil.FindShortestPath(start, new Vector3Int(5, 0, 2), speed, out int loopCount);
        Assert.AreEqual(6, path.Count, "Path length");
        for (int i = 0; i < path.Count; i++) {
            Assert.AreEqual(FacingUtil.E, path[i].Facing);
            Assert.AreEqual(i, path[i].GridPos.x);
            Assert.AreEqual(0, path[i].GridPos.y);
            Assert.AreEqual(2, path[i].GridPos.z);
            Assert.AreEqual(startTime + i, path[i].WorldTime);
            if (i > 0) {
                Assert.AreEqual(MoveType.WALK_FORWARD, path[i].MoveType);
            }
        }
        Assert.LessOrEqual(loopCount, 7, "Search steps");
    }

    [Test]
    public void PathingColliders() {
        float startTime = 1;
        MovementSpeed speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        for (int x = 0; x <= 5; x++) {
            for (int z = 0; z <= 5; z++) {
                WorldObject.CreateTile(new Vector3Int(x, 0, z));
            }
        }
        WorldObject.CreateEntity(new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1), new Vector3Int(3, 0, 2));

        NavStep start = new NavStep(MoveType.NONE, new Vector3Int(0, 0, 2), FacingUtil.E, startTime);
        List<NavStep> path = WorldUtil.FindShortestPath(start, new Vector3Int(5, 0, 2), speed, out int loopCount);

        Assert.AreEqual(10, path.Count, "Path length");
        int walkCount = 0;
        int rotateCount = 0;
        for (int i = 0; i < path.Count; i++) {
            if (path[i].MoveType == MoveType.WALK_FORWARD) {
                walkCount++;
            } else if (path[i].MoveType == MoveType.TURN_LEFT || path[i].MoveType == MoveType.TURN_RIGHT) {
                rotateCount++;
            }
            Assert.AreEqual(startTime + i, path[i].WorldTime);
        }
        Assert.AreEqual(6, walkCount, "Walk steps");
        Assert.AreEqual(3, rotateCount, "Rotate steps");
        Assert.Less(loopCount, 210, "Search steps");
    }
}
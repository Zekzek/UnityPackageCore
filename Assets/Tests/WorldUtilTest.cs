using NUnit.Framework;
using UnityEngine;
using Zekzek.HexWorld;

public class WorldUtilTest
{
    [Test]
    public void FindDistance()
    {
        Assert.AreEqual(3, WorldUtil.FindDistance(
            new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1)));
    }

    [Test]
    public void FindHorizontalDistance()
    {
        Assert.AreEqual(2, WorldUtil.FindHorizontalDistance(
            new Vector3Int(0, 0, 0), new Vector3Int(1, 5, 1)));
    }

    [Test]
    public void GetRectangleIndicesAround()
    {
    }

    [Test]
    public void GetBurstIndicesAround()
    {
    }

    [Test]
    public void GetIndicesAround()
    {
    }

    [Test]
    public void GridIndexToGridRegion1()
    {
        // scaling x-values
        Assert.AreEqual(new Vector2Int(0, 0),  WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 0), 1));
        Assert.AreEqual(new Vector2Int(0, 0),  WorldUtil.GridIndexToGridRegion(new Vector2Int(1, 0), 1));
        Assert.AreEqual(new Vector2Int(1, 0),  WorldUtil.GridIndexToGridRegion(new Vector2Int(2, 0), 1));
        Assert.AreEqual(new Vector2Int(1, 0),  WorldUtil.GridIndexToGridRegion(new Vector2Int(3, 0), 1));
        Assert.AreEqual(new Vector2Int(2, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(4, 0), 1));
        Assert.AreEqual(new Vector2Int(2, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(5, 0), 1));
        Assert.AreEqual(new Vector2Int(3, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(6, 0), 1));
        Assert.AreEqual(new Vector2Int(3, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(7, 0), 1));
        Assert.AreEqual(new Vector2Int(3, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(8, 0), 1));
        Assert.AreEqual(new Vector2Int(4, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(9, 0), 1));
        Assert.AreEqual(new Vector2Int(4, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(10, 0), 1));
        Assert.AreEqual(new Vector2Int(5, -2), WorldUtil.GridIndexToGridRegion(new Vector2Int(11, 0), 1));

        // scaling y-values
        Assert.AreEqual(new Vector2Int(0, 0), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 0), 1));
        Assert.AreEqual(new Vector2Int(0, 0), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 1), 1));
        Assert.AreEqual(new Vector2Int(0, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 2), 1));
        Assert.AreEqual(new Vector2Int(0, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 3), 1));
        Assert.AreEqual(new Vector2Int(1, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 4), 1));
        Assert.AreEqual(new Vector2Int(1, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 5), 1));
        Assert.AreEqual(new Vector2Int(1, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 6), 1));
        Assert.AreEqual(new Vector2Int(1, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 7), 1));
        Assert.AreEqual(new Vector2Int(1, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 8), 1));
        Assert.AreEqual(new Vector2Int(1, 3), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 9), 1));
        Assert.AreEqual(new Vector2Int(1, 3), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 10), 1));
        Assert.AreEqual(new Vector2Int(2, 3), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 11), 1));

        // scaling both values
        Assert.AreEqual(new Vector2Int(3, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(5, 5), 1));
        Assert.AreEqual(new Vector2Int(3, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(6, 6), 1));
        Assert.AreEqual(new Vector2Int(4, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(7, 7), 1));
        Assert.AreEqual(new Vector2Int(5, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(8, 8), 1));
        Assert.AreEqual(new Vector2Int(5, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(9, 9), 1));
        Assert.AreEqual(new Vector2Int(6, 1), WorldUtil.GridIndexToGridRegion(new Vector2Int(10, 10), 1));

        // region 2,2
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(2, 8), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(1, 9), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(2, 9), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(3, 8), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(3, 7), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(2, 7), 1));
        Assert.AreEqual(new Vector2Int(2, 2), WorldUtil.GridIndexToGridRegion(new Vector2Int(1, 8), 1));
    }

    [Test]
    public void GridIndexToGridRegion2()
    {
        // region 0,0
        Assert.AreEqual(new Vector2Int(0, 0), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 0), 2));
        Assert.AreEqual(new Vector2Int(0, 0), WorldUtil.GridIndexToGridRegion(new Vector2Int(0, 3), 2));
        Assert.AreEqual(new Vector2Int(0, 0), WorldUtil.GridIndexToGridRegion(new Vector2Int(3, 0), 2));

        // region 0,0
        Assert.AreEqual(new Vector2Int(1, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(7, 1), 2));
        Assert.AreEqual(new Vector2Int(1, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(4, 1), 2));
        Assert.AreEqual(new Vector2Int(1, -1), WorldUtil.GridIndexToGridRegion(new Vector2Int(10, 1), 2));
    }
}

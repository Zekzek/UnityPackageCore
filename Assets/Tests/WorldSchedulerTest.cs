using NUnit.Framework;
using UnityEngine;
using Zekzek.HexWorld;

public class WorldSchedulerTest
{
    private int count;

    [SetUp] public void Reset() {
        WorldScheduler.Instance.Reset();
        count = 0;
    }

    [Test]
    public void IncrementTime() {
        Assert.AreEqual(0, WorldScheduler.Instance.Time);
        WorldScheduler.Instance.Time += 1f;
        Assert.AreEqual(1, WorldScheduler.Instance.Time);
        WorldScheduler.Instance.Time = 0f;
        Assert.AreEqual(1, WorldScheduler.Instance.Time);
        WorldScheduler.Instance.Time += 4f;
        Assert.AreEqual(5, WorldScheduler.Instance.Time);
    }

    [Test]
    public void RegisterAtOnce() {
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter);
        Assert.AreEqual(0, count);
        WorldScheduler.Instance.Time += 1f;
        Assert.AreEqual(0, count);
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(1, count);
        WorldScheduler.Instance.Time += 7f;
        Assert.AreEqual(1, count);
    }

    [Test]
    public void RegisterAtRecurring() {
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(1, count);
        WorldScheduler.Instance.Time += 1f;
        Assert.AreEqual(2, count);
        WorldScheduler.Instance.Time += 7f;
        Assert.AreEqual(9, count);
    }

    [Test]
    public void RegisterAtMultiple() {
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.RegisterAt(3f, CallbackCounter, 1f);
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(1, count);
        WorldScheduler.Instance.Time += 1f;
        Assert.AreEqual(3, count);
        WorldScheduler.Instance.Time += 7f;
        Assert.AreEqual(17, count);
    }

    [Test]
    public void Unregister() {
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(2, count);
        Assert.IsTrue(WorldScheduler.Instance.Unregister(CallbackCounter));
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(4, count);
        Assert.IsTrue(WorldScheduler.Instance.Unregister(CallbackCounter));
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(4, count);
        Assert.IsFalse(WorldScheduler.Instance.Unregister(CallbackCounter));
    }

    [Test]
    public void UnregisterAll() {
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.RegisterAt(2f, CallbackCounter, 1f);
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(2, count);
        WorldScheduler.Instance.UnregisterAll(CallbackCounter);
        Assert.IsFalse(WorldScheduler.Instance.Unregister(CallbackCounter));
        WorldScheduler.Instance.Time += 2f;
        Assert.AreEqual(2, count);
    }

    [Test]
    public void GetPercentComplete() {
        LocationComponent location = new LocationComponent(0, new Vector3Int(1, 0, 1), 0);
        location.ScheduleGridShift(new Vector3Int(2, 0, 2), 2);
        Assert.AreEqual(new Vector3Int(1, 0, 1), location.Current.GridPosition, "Starts at specified location");
        WorldScheduler.Instance.Time += 1;
        Assert.AreEqual(new Vector3Int(2, 0, 2), location.Current.GridPosition, "Half way between scheduled locations");
        location.ScheduleGridShift(new Vector3Int(2, 0, 2), 2);
        location.ScheduleGridShift(new Vector3Int(3, 0, 3), 3);
        Assert.AreEqual(new Vector3Int(2, 0, 2), location.Current.GridPosition, "Half way between scheduled events with more in the queue");
        WorldScheduler.Instance.Time += 1;
        Assert.AreEqual(new Vector3Int(3, 0, 3), location.Current.GridPosition, "Complete a step");
        WorldScheduler.Instance.Time += 1;
        Assert.AreEqual(new Vector3Int(4, 0, 4), location.Current.GridPosition, "Complete another step");
    }

    private void CallbackCounter() { count++; }
}

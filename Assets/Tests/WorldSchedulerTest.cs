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
    public void GetUnregisteredState() {
        WorldScheduler.Instance.RegisterAt(1, new WorldLocation(0, new Vector3(1, 0, 1), 0));
        WorldScheduler.Instance.TryGetLocation(0, out WorldLocation previous, out WorldLocation next, out float percentComplete);
        Assert.IsNull(previous);
        Assert.IsNull(next);
        Assert.AreEqual(-1, percentComplete);
    }

    [Test]
    public void GetFutureState() {
        WorldScheduler.Instance.RegisterAt(1, new WorldLocation(0, new Vector3(1, 0, 1), 0));
        WorldScheduler.Instance.TryGetLocation(0, out WorldLocation previous, out WorldLocation next, out float percentComplete);
        Assert.IsNull(previous);
        Assert.IsNull(next);
        Assert.AreEqual(-1, percentComplete);
        WorldScheduler.Instance.RegisterAt(2, new WorldLocation(0, new Vector3(1, 0, 1), 0));
        WorldScheduler.Instance.TryGetLocation(0, out previous, out next, out percentComplete);
        Assert.IsNull(previous);
        Assert.IsNull(next);
        Assert.AreEqual(-1, percentComplete);
    }

    [Test]
    public void GetPastState() {
        WorldLocation inState = new WorldLocation(0, new Vector3(1, 0, 1), 0);
        WorldScheduler.Instance.RegisterAt(1, inState);
        WorldScheduler.Instance.Time += 2;
        WorldScheduler.Instance.TryGetLocation(0, out WorldLocation previous, out WorldLocation next, out float percentComplete);
        Assert.AreEqual(inState.Position.x, previous.Position.x);
        Assert.AreEqual(inState.Position.x, next.Position.x);
        Assert.AreEqual(0, percentComplete);
    }

    [Test]
    public void ForgetPastState() {
        WorldLocation inState = new WorldLocation(0, new Vector3(2, 0, 2), 0);
        WorldScheduler.Instance.RegisterAt(1, new WorldLocation(0, new Vector3(1, 0, 1), 0));
        WorldScheduler.Instance.RegisterAt(2, inState);
        WorldScheduler.Instance.Time += 3;
        WorldScheduler.Instance.TryGetLocation(0, out WorldLocation previous, out WorldLocation next, out float percentComplete);
        Assert.AreEqual(inState.Position.x, previous.Position.x);
        Assert.AreEqual(inState.Position.x, next.Position.x);
        Assert.AreEqual(0, percentComplete);
    }

    [Test]
    public void GetPercentComplete() {
        WorldLocation inState1 = new WorldLocation(0, new Vector3(1, 0, 1), 0);
        WorldLocation inState2 = new WorldLocation(0, new Vector3(2, 0, 2), 0);
        WorldScheduler.Instance.RegisterAt(1, inState1);
        WorldScheduler.Instance.RegisterAt(3, inState2);
        WorldScheduler.Instance.Time += 2;
        WorldScheduler.Instance.TryGetLocation(0, out WorldLocation previous, out WorldLocation next, out float percentComplete);
        Assert.AreEqual(inState1.Position.x, previous.Position.x);
        Assert.AreEqual(inState2.Position.x, next.Position.x);
        Assert.AreEqual(0.5f, percentComplete);
        WorldScheduler.Instance.RegisterAt(4, inState1);
        WorldScheduler.Instance.RegisterAt(5, inState2);
        WorldScheduler.Instance.TryGetLocation(0, out previous, out next, out percentComplete);
        Assert.AreEqual(inState1.Position.x, previous.Position.x);
        Assert.AreEqual(inState2.Position.x, next.Position.x);
        Assert.AreEqual(0.5f, percentComplete);
    }

    private void CallbackCounter() { count++; }
}

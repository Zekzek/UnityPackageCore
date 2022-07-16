using NUnit.Framework;
using Zekzek.HexWorld;

public class FacingUtilTest
{
    [Test]
    public void LerpRotationAroundUpAxis()
    {
        Assert.AreEqual(50, FacingUtil.LerpRotationAroundUpAxis(0, 100, 0.5f));
        Assert.AreEqual(25, FacingUtil.LerpRotationAroundUpAxis(0, 100, 0.25f));
        Assert.AreEqual(75, FacingUtil.LerpRotationAroundUpAxis(0, 100, 0.75f));

        Assert.AreEqual(-50, FacingUtil.LerpRotationAroundUpAxis(0, -100, 0.5f));
        Assert.AreEqual(-25, FacingUtil.LerpRotationAroundUpAxis(0, -100, 0.25f));
        Assert.AreEqual(-75, FacingUtil.LerpRotationAroundUpAxis(0, -100, 0.75f));

        Assert.AreEqual(0, FacingUtil.LerpRotationAroundUpAxis(10, -10, 0.5f));
        Assert.AreEqual(5, FacingUtil.LerpRotationAroundUpAxis(10, -10, 0.25f));
        Assert.AreEqual(-5, FacingUtil.LerpRotationAroundUpAxis(10, -10, 0.75f));

        Assert.AreEqual(0, FacingUtil.LerpRotationAroundUpAxis(-10, 10, 0.5f));
        Assert.AreEqual(-5, FacingUtil.LerpRotationAroundUpAxis(-10, 10, 0.25f));
        Assert.AreEqual(5, FacingUtil.LerpRotationAroundUpAxis(-10, 10, 0.75f));

        Assert.AreEqual(180, FacingUtil.LerpRotationAroundUpAxis(170, -170, 0.5f));
        Assert.AreEqual(175, FacingUtil.LerpRotationAroundUpAxis(170, -170, 0.25f));
        Assert.AreEqual(-175, FacingUtil.LerpRotationAroundUpAxis(170, -170, 0.75f));

        Assert.AreEqual(180, FacingUtil.LerpRotationAroundUpAxis(-170, 170, 0.5f));
        Assert.AreEqual(-175, FacingUtil.LerpRotationAroundUpAxis(-170, 170, 0.25f));
        Assert.AreEqual(175, FacingUtil.LerpRotationAroundUpAxis(-170, 170, 0.75f));
    }

    [Test]
    public void FacingAngleReversible()
    {
        Assert.AreEqual(FacingUtil.NE, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.NE)));
        Assert.AreEqual(FacingUtil.E, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.E)));
        Assert.AreEqual(FacingUtil.SE, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.SE)));
        Assert.AreEqual(FacingUtil.SW, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.SW)));
        Assert.AreEqual(FacingUtil.W, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.W)));
        Assert.AreEqual(FacingUtil.NW, FacingUtil.GetFacing(FacingUtil.GetRotationAroundUpAxis(FacingUtil.NW)));
    }
}

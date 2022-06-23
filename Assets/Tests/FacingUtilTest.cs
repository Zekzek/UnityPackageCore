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
}

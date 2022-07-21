using NUnit.Framework;
using UnityEngine;
using Zekzek.Stats;

public class StatBlockTest : MonoBehaviour
{
    [Test]
    public void AmountMultiplier()
    {
        StatBlock statBlock = new StatBlock();
        statBlock.AddAmount(StatType.Health, 100);
        statBlock.AddMultiplier(StatType.Health, 0.2f);

        Assert.AreEqual(120, statBlock.GetTotalValue(StatType.Health), 0.01f);
    }

    [Test]
    public void NestedAmountMultiplier()
    {
        StatBlock statBlock = new StatBlock();
        statBlock.AddSlot(SlotType.Inherent);
        statBlock.AddAmount(StatType.Health, 100);
        statBlock.AddMultiplier(StatType.Health, 0.2f);

        StatBlock innerStatBlock = new StatBlock();
        innerStatBlock.AddAmount(StatType.Health, 100);
        innerStatBlock.AddMultiplier(StatType.Health, 0.2f);
        statBlock.Equip(SlotType.Inherent, innerStatBlock);

        Assert.AreEqual(264, statBlock.GetTotalValue(StatType.Health), 0.01f);
    }
}
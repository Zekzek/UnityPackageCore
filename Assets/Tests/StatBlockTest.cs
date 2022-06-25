using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Zekzek.Stats;

public class StatBlockTest : MonoBehaviour
{
    [Test]
    public void AmountMultiplier()
    {
        IDictionary<StatType, float> amounts = new Dictionary<StatType, float>() { { StatType.Life, 100 } };
        IDictionary<StatType, float> multipliers = new Dictionary<StatType, float>() { { StatType.Life, 0.2f } };
        StatBlock statBlock = new StatBlock(amounts, multipliers, new HashSet<SlotType>());

        Assert.AreEqual(120, statBlock.GetTotalValue(StatType.Life), 0.01f);
    }

    [Test]
    public void NestedAmountMultiplier()
    {
        IDictionary<StatType, float> amounts = new Dictionary<StatType, float>() { { StatType.Life, 100 } };
        IDictionary<StatType, float> multipliers = new Dictionary<StatType, float>() { { StatType.Life, 0.2f } };
        StatBlock statBlock = new StatBlock(amounts, multipliers, new HashSet<SlotType>() { SlotType.Inherent });

        StatBlock innerStatBlock = new StatBlock(amounts, multipliers, new HashSet<SlotType>());
        statBlock.Equip(SlotType.Inherent, innerStatBlock);

        Assert.AreEqual(280, statBlock.GetTotalValue(StatType.Life), 0.01f);
    }

}

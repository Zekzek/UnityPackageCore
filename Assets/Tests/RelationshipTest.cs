using NUnit.Framework;

public class RelationshipTest
{
    [Test]
    public void DefaultAffinity()
    {
        RelationshipComponent relationship = new RelationshipComponent(1);

        relationship.AddDefaultAffinity(RelationshipType.Affection, 0.5f);
        Assert.AreEqual(0.5f, relationship.GetAffinity(RelationshipType.Affection, 35));

        relationship.AddDefaultAffinity(RelationshipType.Trust, -0.5f);
        Assert.AreEqual(-0.5f, relationship.GetAffinity(RelationshipType.Trust, 35));
    }

    [Test]
    public void Add()
    {
        RelationshipComponent relationship = new RelationshipComponent(1);

        relationship.Add(RelationshipType.Trust, 1, -0.5f);
        Assert.AreEqual(-0.5f, relationship.Get(RelationshipType.Trust, 1));

        relationship.Add(RelationshipType.Affection, 1, 0.5f);
        Assert.AreEqual(0.5f, relationship.Get(RelationshipType.Affection, 1));
        relationship.Add(RelationshipType.Affection, 1, 0.25f);
        Assert.AreEqual(0.75f, relationship.Get(RelationshipType.Affection, 1));    
    }

    [Test]
    public void Clamp()
    {
        RelationshipComponent relationship = new RelationshipComponent(1);
        
        relationship.Add(RelationshipType.Affection, 1, 5f);
        Assert.AreEqual(1f, relationship.Get(RelationshipType.Affection, 1));
        
        relationship.Add(RelationshipType.Trust, 1, -5f);
        Assert.AreEqual(-1f, relationship.Get(RelationshipType.Trust, 1));
    }

    [Test]
    public void StartingBonusFromAffinity()
    {
        RelationshipComponent relationship = new RelationshipComponent(1);

        relationship.AddAffinity(RelationshipType.Affection, 1, 1f);
        relationship.Add(RelationshipType.Affection, 1, 0);
        Assert.AreEqual(0.5f, relationship.Get(RelationshipType.Affection, 1));
        
        relationship.AddAffinity(RelationshipType.Trust, 1, -1f);
        relationship.Add(RelationshipType.Trust, 1, 0);
        Assert.AreEqual(-0.5f, relationship.Get(RelationshipType.Trust, 1));
    }

    [Test]
    public void AffinityScaling()
    {
        RelationshipComponent relationship = new RelationshipComponent(1);

        relationship.AddAffinity(RelationshipType.Affection, 1, 1f);
        relationship.Add(RelationshipType.Affection, 1, 0.2f);
        Assert.AreEqual(0.8f, relationship.Get(RelationshipType.Affection, 1));
        relationship.Add(RelationshipType.Affection, 1, -1f);
        Assert.AreEqual(0.3f, relationship.Get(RelationshipType.Affection, 1));

        relationship.AddAffinity(RelationshipType.Trust, 1, -1f);
        relationship.Add(RelationshipType.Trust, 1, 0.5f);
        Assert.AreEqual(-0.25f, relationship.Get(RelationshipType.Trust, 1));
        relationship.Add(RelationshipType.Trust, 1, -0.2f);
        Assert.AreEqual(-0.55f, relationship.Get(RelationshipType.Trust, 1));
    }
}
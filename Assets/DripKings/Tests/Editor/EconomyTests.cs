using NUnit.Framework;

public class EconomyTests
{
    [Test]
    public void CurrencyAndMissionInvariants() => Assert.That(EconomyChecks.Run(), Is.GreaterThanOrEqualTo(30));
}

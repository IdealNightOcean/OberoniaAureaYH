using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompProperties_ChargedExplosion : CompProperties_Explosive
{
	public int chargeTicks = 100000;

	[MustTranslate]
	public string detonateLabel = "detonate";

	[MustTranslate]
	public string noPowerReason = "no power";

	[MustTranslate]
	public string noFuelReason = "no fuel";

	[MustTranslate]
	public string noChargeReason = "not charged";

	[MustTranslate]
	public string breakdownReason = "broken";

	[NoTranslate]
	public string detonateIcon = "";

	public CompProperties_ChargedExplosion()
	{
		compClass = typeof(CompChargedExplosion);
	}
}

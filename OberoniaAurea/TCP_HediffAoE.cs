using Verse;

namespace OberoniaAurea;

public class TCP_HediffAoE : CompProperties
{
	public HediffDef hediff;

	public bool InBrain = false;

	public float range;

	public int checkInterval = 10;

	public bool onlyTargetMechs;

	public bool onlyTargetOwnSide;

	public TCP_HediffAoE()
	{
		compClass = typeof(TC_HediffAoE);
	}
}

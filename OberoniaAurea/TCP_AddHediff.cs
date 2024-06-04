using RimWorld;
using Verse;

namespace OberoniaAurea;

public class TCP_AddHediff : CompProperties_UseEffect
{
	public HediffDef hediff;

	public TCP_AddHediff()
	{
		compClass = typeof(TC_AddHediff);
	}
}

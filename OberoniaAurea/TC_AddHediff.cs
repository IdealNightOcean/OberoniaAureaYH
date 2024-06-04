using RimWorld;
using Verse;

namespace OberoniaAurea;

public class TC_AddHediff : CompTargetEffect
{
	public TCP_AddHediff Props => (TCP_AddHediff)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		Pawn pawn = (Pawn)target;
		if (!pawn.Dead)
		{
			Hediff hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
			pawn.health.AddHediff(hediff);
		}
	}
}

using Verse;

namespace OberoniaAurea;

public class HC_ConditionIncrease : HediffComp
{
	public HCP_ConditionIncrease Props => (HCP_ConditionIncrease)props;

	public override void CompPostMake()
	{
		base.CompPostMake();
		if (base.Pawn.Map != null && base.Pawn != null && base.Pawn.health != null && Props.hediff != null)
		{
			Hediff firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
			if (firstHediffOfDef != null)
			{
				firstHediffOfDef.Severity = Props.endSeverity;
			}
		}
	}

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		if (base.Pawn.Map != null && base.Pawn != null && base.Pawn.health != null && Props.hediff != null)
		{
			Hediff firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
			if (firstHediffOfDef != null)
			{
				firstHediffOfDef.Severity = 1f;
			}
		}
	}
}

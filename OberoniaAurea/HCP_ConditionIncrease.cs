using Verse;

namespace OberoniaAurea;

public class HCP_ConditionIncrease : HediffCompProperties
{
	public HediffDef hediff;

	public float endSeverity = 2f;

	public HCP_ConditionIncrease()
	{
		compClass = typeof(HC_ConditionIncrease);
	}
}

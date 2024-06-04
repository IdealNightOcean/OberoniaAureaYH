using RimWorld;
using Verse;

namespace OberoniaAurea;

public class InteractionWorker_OAInsult : InteractionWorker
{
	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		float num = ((!initiator.IsSlave || recipient.IsSlave) ? (0.007f * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient)) : 0f);
		Ideo ideo = initiator.Ideo;
		if (ideo != null)
		{
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Utopia))
			{
				num *= 0.5f;
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Harmonious))
			{
				num *= 0.75f;
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Turbulent))
			{
				num *= 1.25f;
			}
		}
		return num;
	}
}

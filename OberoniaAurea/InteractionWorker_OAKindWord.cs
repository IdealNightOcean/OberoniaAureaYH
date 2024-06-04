using RimWorld;
using Verse;

namespace OberoniaAurea;

public class InteractionWorker_OAKindWord : InteractionWorker
{
	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		float num = ((!initiator.story.traits.HasTrait(TraitDefOf.Kind)) ? 0f : 0.01f);
		Ideo ideo = initiator.Ideo;
		if (ideo != null)
		{
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Utopia))
			{
				num *= 1.1f;
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Turbulent))
			{
				num *= 0.75f;
			}
		}
		return num;
	}
}

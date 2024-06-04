using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_TraitOpinion_B : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
	{
		if (other.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(pawn, other))
		{
			Ideo ideo = pawn.Ideo;
			if (ideo != null && ideo.HasMeme(OADefOf.OA_RK_meme_Friendly) && other.story.traits.HasTrait(TraitDefOf.Bloodlust))
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		return false;
	}
}

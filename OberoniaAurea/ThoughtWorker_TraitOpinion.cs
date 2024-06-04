using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_TraitOpinion : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
	{
		if (other.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(pawn, other))
		{
			Ideo ideo = pawn.Ideo;
			if (ideo != null && ideo.HasMeme(OADefOf.OA_RK_meme_Friendly) && other.story.traits.HasTrait(TraitDefOf.Kind))
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		return false;
	}
}

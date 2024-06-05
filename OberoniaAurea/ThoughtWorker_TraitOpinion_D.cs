using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_TraitOpinion_D : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		if (other.RaceProps.Humanlike && RelationsUtility.PawnsKnowEachOther(pawn, other))
		{
			Ideo ideo = pawn.Ideo;
			if (ideo != null && ideo.HasMeme(OADefOf.OA_RK_meme_Friendly) && other.story.traits.HasTrait(OADefOf.Cannibal))
			{
				return ThoughtState.ActiveDefault;
			}
		}
		return false;
	}
}

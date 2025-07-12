using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_MemeTraitOpinion : ThoughtWorker
{
    protected MemeTraitOpinionExtension modEx_MTO;
    public MemeTraitOpinionExtension ModEx_MTO => modEx_MTO ??= def.GetModExtension<MemeTraitOpinionExtension>();
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (!ModsConfig.IdeologyActive || !other.RaceProps.Humanlike)
        {
            return ThoughtState.Inactive;
        }
        if (RelationsUtility.PawnsKnowEachOther(pawn, other))
        {
            Ideo ideo = pawn.Ideo;
            if (ideo is not null && ideo.HasMeme(ModEx_MTO.meme) && other.story.traits.HasTrait(ModEx_MTO.trait))
            {
                return ThoughtState.ActiveAtStage(0);
            }
        }
        return ThoughtState.Inactive;
    }
}

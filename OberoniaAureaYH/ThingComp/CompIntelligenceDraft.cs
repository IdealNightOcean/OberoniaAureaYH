using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompIntelligenceDraft : CompHackable
{
    public override void FinishHack(Pawn hackPawn)
    {
        hackPoint = 0f;
        if (parent.stackCount > 1)
        {
            parent.stackCount--;
        }

        float rand = Rand.Value;
        if (rand <= 0.3f)
        {
            bool isTopSecret = rand < 0.02f;
            float pawnAbility = hackPawn.GetStatValue(StatDefOf.ReadingSpeed);
            float baselineAbility = (isTopSecret ? 2.5f : 1.5f) * Rand.Range(0.5f, 1.5f);
            InteractionOutcome outcome = MultiPartyTalksUtility.DetermineOutcomeCommon(baselineAbility, pawnAbility);
        }

        if (parent.stackCount <= 1)
        {
            base.FinishHack(hackPawn);
        }
    }
}

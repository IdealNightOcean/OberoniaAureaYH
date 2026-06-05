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
        if (rand <= 0.03f)
        {
            bool isSecret = rand < 0.002f;
            float pawnAbility = hackPawn.GetStatValue(StatDefOf.ReadingSpeed);
            float baselineAbility = (isSecret ? 2.5f : 1.5f) * Rand.Range(0.5f, 1.5f);
            InteractionOutcome outcome = DiplomacyUtility.DetermineOutcomeCommon(baselineAbility, pawnAbility);
            if (outcome > InteractionOutcome.Neutral)
            {
                ThingDef intelligenceDef = isSecret ? OARK_ThingDefOf.OARK_IntelligenceSecret : OARK_ThingDefOf.OARK_IntelligenceNormal;
                Thing intelligence = ThingMaker.MakeThing(intelligenceDef);
                GenPlace.TryPlaceThing(intelligence, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
                Messages.Message(
                    text: "OARK_IntelligenceDraft_DecodeSucceed".Translate(
                        hackPawn.Named(KeyLibrary_NamedArgument.PAWN),
                        GenLabel.ThingLabel(intelligenceDef, null).Named("ThingLabel")),
                    lookTargets: intelligence,
                    def: MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message(
                    text: "OARK_IntelligenceDraft_DecodeFailed".Translate(hackPawn.Named(KeyLibrary_NamedArgument.PAWN)),
                    lookTargets: hackPawn,
                    def: MessageTypeDefOf.NegativeEvent,
                    historical: false);
            }
        }

        if (parent.stackCount <= 1)
        {
            base.FinishHack(hackPawn);
        }
    }
}

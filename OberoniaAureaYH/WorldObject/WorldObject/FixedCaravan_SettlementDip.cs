using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;
public enum NegotiatingTeamLevel
{
    Beginner,
    Standard,
    Sophistication,
    Excellence
}

[StaticConstructorOnStartup]
public class FixedCaravan_DiplomaticSummit : FixedCaravan
{
    public static WorldObjectDef AssociateObjectDef = OA_WorldObjectDefOf.OA_FixedCaravan_DiplomaticSummit;
    public static readonly int CheckInterval = 5000;

    public Pawn negotiant;
    public Settlement associateSettlement;
    public NegotiatingTeamLevel curNegotiatingTeamLevel = NegotiatingTeamLevel.Standard;
    public SettlementDipComp AssociateSettlementComp => associateSettlement?.GetComponent<SettlementDipComp>();

    public override void Tick()
    {
        base.Tick();
        ticksRemaining--;
        if (ticksRemaining <= 0)
        {
            DiplomaticSummitOutcome();
        }
    }
    protected override void PreConvertToCaravanByPlayer()
    {
        DiplomaticSummitUtility.Outcome_LeaveHalfway(associateSettlement);
    }
    public override void Notify_ConvertToCaravan()
    {
        AssociateSettlementComp?.DiplomaticSummitEnd();
    }
    private void DiplomaticSummitOutcome()
    {
        DiplomaticSummitUtility.ApplyEffect(curNegotiatingTeamLevel, this, associateSettlement, negotiant);
        FixedCaravanUtility.ConvertToCaravan(this);
    }
    public override string GetInspectString()
    {
        StringBuilder stringBuilder = new(base.GetInspectString());
        stringBuilder.AppendInNewLine("OA_FixedCaravanRSAssistWork_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod()));
        return stringBuilder.ToString();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curNegotiatingTeamLevel, "curNegotiatingTeamLevel", NegotiatingTeamLevel.Standard);
        Scribe_References.Look(ref negotiant, "negotiant");
        Scribe_References.Look(ref associateSettlement, "associateSettlement");
    }
}

[StaticConstructorOnStartup]
public static class DiplomaticSummitUtility
{
    public static readonly Dictionary<NegotiatingTeamLevel, float> NegotiatingTeamLevelWeight = new()
    {
        {NegotiatingTeamLevel.Beginner,0.15f},
        {NegotiatingTeamLevel.Standard,0.5f},
        {NegotiatingTeamLevel.Sophistication,0.25f},
        {NegotiatingTeamLevel.Excellence,0.1f}
    };
    private static readonly Dictionary<NegotiatingTeamLevel, float> NegotiatingLevelInfluenceFactor = new()
    {
        {NegotiatingTeamLevel.Beginner,1.3f},
        {NegotiatingTeamLevel.Standard,1.0f},
        {NegotiatingTeamLevel.Sophistication,0.65f},
        {NegotiatingTeamLevel.Excellence,0.25f}
    };

    private static readonly int LeaveHalfwayGoodwill = -50;

    private static readonly int DisasterGoodwill = -50;
    private static readonly int DisasterStoppageDays = 30;

    private static readonly int FlounderGoodwill = -15;
    private static readonly int FlounderAssistPoints = 5;
    private static readonly float FlounderXP = 3000f;

    private static readonly int SuccessAssistPoints = 25;
    private static readonly float SuccessXP = 6000f;

    private static readonly int TriumphGoodwill = 10;
    private static readonly int TriumphAssistPoints = 60;
    private static readonly float TriumphXP = 8000f;

    private readonly static List<Pair<Action, float>> tmpPossibleOutcomes = [];

    public static void ApplyEffect(NegotiatingTeamLevel negotiatingTeamLevel, FixedCaravan_DiplomaticSummit fixedCaravan, Settlement settlement, Pawn pawn)
    {
        float influenceFactor = NegotiatingLevelInfluenceFactor[negotiatingTeamLevel];
        float negotiationAbility = pawn.GetStatValue(StatDefOf.NegotiationAbility);
        tmpPossibleOutcomes.Clear();
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Disaster(settlement);
        }, 40f));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Flounder(settlement, pawn);
        }, 40f + negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Success(settlement, pawn);
        }, 10f + negotiationAbility * 40f * influenceFactor));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Triumph(fixedCaravan, settlement, pawn);
        }, negotiationAbility * 20f * influenceFactor));
        tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First();
    }
    public static void Outcome_LeaveHalfway(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, LeaveHalfwayGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_LeaveHalfway);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_LeaveHalfway".Translate(), "OA_LetterDiplomaticSummit_LeaveHalfway".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, LeaveHalfwayGoodwill), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Disaster(Settlement settlement)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, DisasterGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Disaster);
        OberoniaAureaYHUtility.OA_GCOA.assistPointsStoppageDays = DisasterStoppageDays;
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Disaster".Translate(), "OA_LetterDiplomaticSummit_Disaster".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, DisasterGoodwill, DisasterStoppageDays), LetterDefOf.NegativeEvent, settlement, faction);
    }
    private static void Outcome_Flounder(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        Faction.OfPlayer.TryAffectGoodwillWith(faction, FlounderGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Flounder);
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(FlounderAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Flounder".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, FlounderGoodwill, FlounderAssistPoints);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, FlounderXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, FlounderXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Flounder".Translate(), text, LetterDefOf.NeutralEvent, settlement, faction);
    }
    private static void Outcome_Success(Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(SuccessAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Success".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, SuccessAssistPoints);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, SuccessXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, SuccessXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Success".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);

    }
    private static void Outcome_Triumph(FixedCaravan_DiplomaticSummit fixedCaravan, Settlement settlement, Pawn pawn)
    {
        Faction faction = settlement.Faction;
        List<Thing> things = OberoniaAureaFrameUtility.TryGenerateThing(RimWorld.ThingDefOf.Silver, 1500);
        foreach (Thing thing in things)
        {
            FixedCaravanUtility.GiveThing(fixedCaravan, thing);
        }
        Faction.OfPlayer.TryAffectGoodwillWith(faction, TriumphGoodwill, canSendMessage: false, canSendHostilityLetter: false, OA_HistoryEventDefOf.OA_DiplomaticSummit_Triumph);
        OberoniaAureaYHUtility.OA_GCOA?.GetAssistPoints(TriumphAssistPoints);
        TaggedString text = "OA_LetterDiplomaticSummit_Triumph".Translate(settlement.Named("SETTLEMENT"), faction.NameColored, TriumphGoodwill, TriumphAssistPoints, 1500);
        if (pawn != null)
        {
            pawn.skills.Learn(SkillDefOf.Intellectual, TriumphXP, direct: true);
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, TriumphXP.ToString("F0"), pawn.Named("PAWN"));
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticSummit_Triumph".Translate(), text, LetterDefOf.PositiveEvent, settlement, faction);

    }

}
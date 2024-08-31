using LudeonTK;
using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;
public class WorldObject_MultiPartyTalks : WorldObject_WithMutiFactions
{

    private static readonly SimpleCurve BadOutcomeChanceFactorByNegotiationAbility =
    [
        new CurvePoint(0f, 4f),
        new CurvePoint(1f, 1f),
        new CurvePoint(1.5f, 0.4f)
    ];

    public static readonly float LeaderOffset = 0.05f;

    private static readonly int SuccessAssistPoints = 10;
    private static readonly int TriumphAssistPoints = 25;


    private readonly static List<Pair<Action, float>> tmpPossibleOutcomes = [];
    private static GameComponent_OberoniaAurea GC_OA => Current.Game.GetComponent<GameComponent_OberoniaAurea>();

    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn == null)
        {
            Messages.Message("OAFrame_MessageNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        float negotiationAbility = pawn.GetStatValue(StatDefOf.NegotiationAbility);
        float leaderWeightFactor = GetLeaderWeightFactor(caravan);
        tmpPossibleOutcomes.Clear();
        if (negotiationAbility < 2.4)
        {
            tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
            {
                Outcome_Disaster(caravan);
            }, 20f));
        }
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Backfire(caravan);
        }, 50f));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_TalksFlounder(caravan);
        }, GetFinalWeight(60f, 12f, negotiationAbility, leaderWeightFactor)));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Success(caravan);
        }, GetFinalWeight(25f, 9f, negotiationAbility, leaderWeightFactor)));
        tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
        {
            Outcome_Triumph(caravan);
        }, GetFinalWeight(5f, 9f, negotiationAbility, leaderWeightFactor)));
        tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First();
        pawn.skills.Learn(SkillDefOf.Social, 6000f, direct: true);
        QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named("SUBJECT"));
        Destroy();
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this))
        {
            yield return floatMenuOption2;
        }
    }

    private void Outcome_Disaster(Caravan caravan) //大失败事件
    {
        Faction.OfPlayer.TryAffectGoodwillWith(base.Faction, -50, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksDisaster);
        foreach (Faction faction in participantFactions)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(faction, -40, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksDisaster);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelMultiPartyTalks_Disaster".Translate(), GetLetterText("OA_LetterMultiPartyTalks_Disaster".Translate(base.Faction.NameColored, -40, -50), caravan, -40, -50, this.participantFactions), LetterDefOf.NegativeEvent, caravan, base.Faction);
    }

    private void Outcome_Backfire(Caravan caravan) //失败事件
    {
        Faction.OfPlayer.TryAffectGoodwillWith(base.Faction, -20, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksBackfire);
        foreach (Faction faction in participantFactions)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(faction, -20, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksBackfire);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelMultiPartyTalks_Backfire".Translate(), GetLetterText("OA_LetterMultiPartyTalks_Backfire".Translate(base.Faction.NameColored, -20), caravan, -20, -20, this.participantFactions), LetterDefOf.NegativeEvent, caravan, base.Faction);
    }

    private void Outcome_TalksFlounder(Caravan caravan) //中立事件
    {
        Faction.OfPlayer.TryAffectGoodwillWith(base.Faction, 5, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelMultiPartyTalks_Flounder".Translate(), GetLetterText("OA_LetterMultiPartyTalks_Flounder".Translate(base.Faction.NameColored, 5), caravan, 0, 5), LetterDefOf.NeutralEvent, caravan, base.Faction);
    }

    private void Outcome_Success(Caravan caravan) //成功事件
    {
        Faction.OfPlayer.TryAffectGoodwillWith(base.Faction, 30, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
        List<Faction> targetFactions = this.participantFactions.Take(3).ToList();
        foreach (Faction faction in targetFactions)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(faction, 20, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
        }
        GC_OA?.GetAssistPoints(SuccessAssistPoints);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelMultiPartyTalks_Success".Translate(), GetLetterText("OA_LetterMultiPartyTalks_Success".Translate(base.Faction.NameColored, 20, 30, SuccessAssistPoints), caravan, 20, 30, targetFactions), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private void Outcome_Triumph(Caravan caravan) //大成功事件
    {
        Faction.OfPlayer.TryAffectGoodwillWith(base.Faction, 60, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksTriumph);
        foreach (Faction faction in participantFactions)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(faction, 40, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksTriumph);
        }
        ThingSetMakerParams parms = default;
        parms.makingFaction = base.Faction;
        parms.techLevel = base.Faction.def.techLevel;
        parms.maxTotalMass = 20f;
        parms.totalMarketValueRange = new FloatRange(500f, 1200f);
        parms.tile = base.Tile;
        List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
        for (int i = 0; i < list.Count; i++)
        {
            caravan.AddPawnOrItem(list[i], addCarriedPawnToWorldPawnsIfAny: true);
        }
        GC_OA?.GetAssistPoints(TriumphAssistPoints);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelMultiPartyTalks_Triumph".Translate(), GetLetterText("OA_LetterMultiPartyTalks_Triumph".Translate(base.Faction.NameColored, 40, 60, GenLabel.ThingsLabel(list), TriumphAssistPoints), caravan, 40, 60, this.participantFactions, TryGainRoyalFavor(caravan)), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private int TryGainRoyalFavor(Caravan caravan) //获取帝国贡献
    {
        int num = 0;
        if (base.Faction.def.HasRoyalTitles)
        {
            num = 3;
            BestCaravanPawnUtility.FindBestDiplomat(caravan)?.royalty.GainFavor(base.Faction, num);
        }
        return num;
    }
    private string GetLetterText(string baseText, Caravan caravan, int goodWill, int oAGoodWill, List<Faction> factions = null, int royalFavorGained = 0)
    {
        TaggedString text = baseText;
        text += "\n\n" + "OA_GoodWillChange".Translate(base.Faction.NameColored, oAGoodWill);
        if (factions != null)
        {
            foreach (Faction f in factions)
            {
                text += "\n" + "OA_GoodWillChange".Translate(f.NameColored, goodWill);
            }
        }
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn != null)
        {
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, 6000f.ToString("F0"), pawn.Named("PAWN"));
            if (royalFavorGained > 0)
            {
                text += "\n\n" + "PeaceTalksRoyalFavorGain".Translate(pawn.LabelShort, royalFavorGained.ToString(), base.Faction.Named("FACTION"), pawn.Named("PAWN"));
            }
        }
        return text;
    }

    private static float GetLeaderWeightFactor(Caravan caravan)
    {
        float num = 1f;
        if (ModsConfig.IdeologyActive)
        {
            bool flag = false;
            foreach (Pawn pawn in caravan.pawns)
            {
                if (pawn == caravan.Faction.leader)
                {
                    flag = true;
                    break;
                }
            }
            num += (flag ? (-0.05f) : 0.05f);
        }
        return num;
    }

    private static float GetFinalWeight(float baseWeight, float negotiationFactor, float negotiationAbility, float leaderWeightFactor)
    {
        return (baseWeight + negotiationFactor * negotiationAbility) * leaderWeightFactor;
    }
    private static float GetBadOutcomeWeightFactor(Pawn diplomat, Caravan caravan)
    {
        float statValue = diplomat.GetStatValue(StatDefOf.NegotiationAbility);
        float num = 0f;
        if (ModsConfig.IdeologyActive)
        {
            bool flag = false;
            foreach (Pawn pawn in caravan.pawns)
            {
                if (pawn == caravan.Faction.leader)
                {
                    flag = true;
                    break;
                }
            }
            num = (flag ? (-0.05f) : 0.05f);
        }
        return GetBadOutcomeWeightFactor(statValue) * (1f + num);
    }

    private static float GetBadOutcomeWeightFactor(float negotationAbility)
    {
        return BadOutcomeChanceFactorByNegotiationAbility.Evaluate(negotationAbility);
    }

    [DebugOutput("Incidents", false)]
    private static void MultiPartyTalksChances()
    {
        StringBuilder stringBuilder = new();
        AppendDebugChances(stringBuilder, 0f);
        AppendDebugChances(stringBuilder, 1f);
        AppendDebugChances(stringBuilder, 1.5f);
        Log.Message(stringBuilder.ToString());
    }

    private static void AppendDebugChances(StringBuilder sb, float negotiationAbility)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }
        sb.AppendLine("--- NegotiationAbility = " + negotiationAbility.ToStringPercent() + " ---");
        float badOutcomeWeightFactor = GetBadOutcomeWeightFactor(negotiationAbility);
        float num = 1f / badOutcomeWeightFactor;
        sb.AppendLine("Bad outcome weight factor: " + badOutcomeWeightFactor.ToString("0.##"));
        float num2 = 0.05f * badOutcomeWeightFactor;
        float num3 = 0.1f * badOutcomeWeightFactor;
        float num4 = 0.2f;
        float num5 = 0.55f * num;
        float num6 = 0.1f * num;
        float num7 = num2 + num3 + num4 + num5 + num6;
        sb.AppendLine("Disaster: " + (num2 / num7).ToStringPercent());
        sb.AppendLine("Backfire: " + (num3 / num7).ToStringPercent());
        sb.AppendLine("Talks flounder: " + (num4 / num7).ToStringPercent());
        sb.AppendLine("Success: " + (num5 / num7).ToStringPercent());
        sb.AppendLine("Triumph: " + (num6 / num7).ToStringPercent());
    }
}

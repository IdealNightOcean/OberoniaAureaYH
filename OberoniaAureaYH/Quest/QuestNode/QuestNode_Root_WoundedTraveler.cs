using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_WoundedTraveler : QuestNode_Root_RefugeeBase
{
    private const int AssistPoints = 15;
    private int curPawnIndex;

    public override PawnKindDef FixedPawnKind => OARK_PawnGenerateDefOf.OA_RK_Traveller;

    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = ModUtility.OAFaction;
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.TestRunInt(slate);
    }

    protected override Faction GetOrGenerateFaction()
    {
        QuestGen.slate.Set(IsMainFactionSlate, true);
        return ModUtility.OAFaction;
    }

    protected override void InitQuestParameter()
    {
        curPawnIndex = 0;
        int lodgerCount = Rand.RangeInclusive(2, 3);
        questParameter = new QuestParameter()
        {
            allowAssaultColony = false,
            allowBadThought = false,
            LodgerCount = lodgerCount,
            ChildCount = 0,

            goodwillSuccess = lodgerCount + 8,
            goodwillFailure = -lodgerCount - 8,
            rewardValueRange = new FloatRange(1200f, 1800f) * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor,
            questDurationTicks = Rand.RangeInclusive(6, 8) * 60000
        };
    }

    protected override void ClearQuestParameter()
    {
        base.ClearQuestParameter();
        curPawnIndex = 0;
    }

    protected override void PostPawnGenerated(Pawn pawn)
    {
        if (curPawnIndex++ == 1)
        {
            pawn.health.AddHediff(OARK_HediffDefOf.OARK_Hediff_SeriousInjury);
            OAFrame_PawnUtility.TakeNonLethalDamage(pawn, Rand.RangeInclusive(2, 3), DamageDefOf.Blunt);
        }
    }

    protected override void AddQuestAward(QuestPart_Choice.Choice choice)
    {
        Reward_AssistPoint reward_AssistPoint = new()
        {
            amount = AssistPoints
        };
        choice.rewards.Add(reward_AssistPoint);
    }

    protected override void SetPawnsLeaveComp(string lodgerArrivalSignal, string inSignalRemovePawn)
    {
        Quest quest = QuestGen.quest;
        List<Pawn> pawns = questParameter.pawns;

        string travelerCanLeaveNowSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.TravelerCanLeaveNow");
        QuestPart_WoundedTravelerCanLeaveNow questPart_WoundedTravelerCanLeaveNow = new()
        {
            inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
            outSignal = travelerCanLeaveNowSignal,
            map = questParameter.map,
        };
        questPart_WoundedTravelerCanLeaveNow.pawns.AddRange(pawns);
        quest.AddPart(questPart_WoundedTravelerCanLeaveNow);
        //提前离开
        quest.SignalPassWithFaction(faction: questParameter.faction, outAction: delegate
        {
            quest.Letter(letterDef: LetterDefOf.PositiveEvent, text: "[lodgersLeavingEarlyLetterText]", label: "[lodgersLeavingEarlyLetterLabel]");
        }, inSignal: travelerCanLeaveNowSignal);
        quest.Leave(pawns, travelerCanLeaveNowSignal, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);

        if (questParameter.questDurationTicks > 0)
        {
            DefaultDelayLeaveComp(lodgerArrivalSignal, inSignalDisable: travelerCanLeaveNowSignal, inSignalRemovePawn);
        }
    }

    protected override void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string delayFailSignal, string successSignal)
    {
        QuestGen.quest.AddPart(new QuestPart_OaAssistPointsChange(successSignal, AssistPoints));
    }
}
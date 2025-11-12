using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_Root_EconomyMinistryReview : QuestNode_Root_RefugeeBase
{
    private string outSignalSettled;
    protected override PawnKindDef FixedPawnKind => OARK_PawnGenerateDefOf.OA_RK_Elite_Court_Member_B;
    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsInteractAvailable()
            && ScienceDepartmentInteractHandler.Instance.IsInitGravQuestCompleted
            && ModUtility.IsOAFactionAlly()
            && base.TestRunInt(slate);
    }

    protected override bool InitQuestParameter()
    {
        questParameter = new QuestParameter()
        {
            allowAssaultColony = false,
            allowBadThought = false,
            allowJoinOffer = false,
            allowFutureReward = false,

            LodgerCount = 1,
            ChildCount = 0,

            questDurationTicks = 10 * 60000,
            arrivalDelayTicks = 2500,

            goodwillFailure = -50
        };

        QuestGen.slate.Set(UniqueQuestDescSlate, true);
        QuestGen.slate.Set(UniqueLeavingLetterSlate, true);
        outSignalSettled = QuestGenUtility.HardcodedSignalWithQuestID("Review_Settlde");

        return true;
    }

    protected override void ClearQuestParameter()
    {
        base.ClearQuestParameter();
        outSignalSettled = null;
    }

    protected override Faction GetOrGenerateFaction()
    {
        QuestGen.slate.Set(IsMainFactionSlate, true);
        return ModUtility.OAFaction;
    }

    protected override void PostPawnGenerated(Pawn pawn, string lodgerRecruitedSignal)
    {
        base.PostPawnGenerated(pawn, lodgerRecruitedSignal);
        pawn.health.AddHediff(OARK_HediffDefOf.OARK_Hediff_Referendary);
    }

    protected override void PawnArrival(string lodgerArrivalSignal)
    {
        QuestGen.quest.Letter(LetterDefOf.NeutralEvent, text: "OARK_Letter_EconomyMinistryReviewRemind".Translate(), label: "OARK_LetterLabel_EconomyMinistryReviewRemind".Translate());
        base.PawnArrival(lodgerArrivalSignal);
    }

    protected override void SetPawnsLeaveComp(string lodgerArrivalSignal, string inSignalRemovePawn)
    {
        QuestGen.quest.Leave(questParameter.pawns, inSignal: outSignalSettled, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);
        base.SetPawnsLeaveComp(lodgerArrivalSignal, inSignalRemovePawn);
    }

    protected override void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string delayFailSignal, string successSignal)
    {
        Quest quest = QuestGen.quest;
        string inSignalReviewDisable = QuestGenUtility.HardcodedSignalWithQuestID("Review_Disabled");
        quest.SignalPassAny(inSignals: [failSignal, delayFailSignal], outSignal: inSignalReviewDisable);
        QuestPart_EconomyMinistryReview_Watcher questPart_EconomyMinistryReview_Watcher = new()
        {
            recordTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("record"),

            inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
            inSignalDisable = inSignalReviewDisable,
            inSignaRecordDecode = QuestGenUtility.HardcodedSignalWithQuestID("record.HackCompleted"),
            inSiganlSettle = successSignal,

            outSignalSettled = outSignalSettled,

            map = questParameter.map,
            referendary = questParameter.pawns[0],
            reactivatable = false
        };
        quest.AddPart(questPart_EconomyMinistryReview_Watcher);

        base.SetQuestEndComp(questPart_Interactions, failSignal, delayFailSignal, successSignal);
    }
}
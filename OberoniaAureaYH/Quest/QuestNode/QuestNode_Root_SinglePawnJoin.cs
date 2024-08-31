using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;
public abstract class QuestNode_Root_SinglePawnJoin : QuestNode
{
    protected string signalAccept;
    protected string signalReject;

    public abstract Pawn GeneratePawn();

    public abstract void SendLetter(Quest quest, Pawn pawn);

    protected virtual void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
    {
        signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
        signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
        quest.Signal(signalAccept, delegate
        {
            quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
            quest.PawnsArrive(Gen.YieldSingle(pawn), null, map.Parent);
            quest.End(QuestEndOutcome.Success);
        });
        quest.Signal(signalReject, delegate
        {
            quest.End(QuestEndOutcome.Fail);
        });
    }

    protected override void RunInt()
    {
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;
        if (!slate.TryGet<Map>("map", out Map map))
        {
            map = QuestGen_Get.GetMap();
        }
        Pawn pawn = GeneratePawn();
        AddSpawnPawnQuestParts(quest, map, pawn);
        slate.Set("pawn", pawn);
        SendLetter(quest, pawn);
        string inSignal1 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Killed");
        string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.PlayerTended");
        string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.LeftMap");
        string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Recruited");
        quest.End(QuestEndOutcome.Fail, 0, null, inSignal1);
        quest.End(QuestEndOutcome.Success, 0, null, inSignal2);
        quest.Signal(inSignal3, delegate
        {
            AddLeftMapQuestParts(quest, pawn);
        });
        quest.End(QuestEndOutcome.Success, 0, null, inSignal4);
    }

    protected virtual void AddLeftMapQuestParts(Quest quest, Pawn pawn)
    {
        quest.AnyPawnUnhealthy(Gen.YieldSingle(pawn), delegate
        {
            quest.End(QuestEndOutcome.Fail);
        },
        delegate
        {
            quest.End(QuestEndOutcome.Success);
        });
    }

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }
}

using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_InitGravQuestCompleted : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsScienceDepartmentInteractAvailable();
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;

        QuestPart_InitGravQuestCompleted questPart_InitGravQuestCompleted = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
        };
        QuestGen.quest.AddPart(questPart_InitGravQuestCompleted);
    }
}

public class QuestPart_InitGravQuestCompleted : QuestPart
{
    public string inSignal;
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            ScienceDepartmentInteractHandler.Instance?.Notify_InitGravQuestCompleted();
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
    }
}
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_ScienceShipRecycleWatcher : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_ScienceShipRecycleWatcher questPart_ScienceShipRecycleWatcher = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_ScienceShipRecycleWatcher);
    }
}


public class QuestPart_ScienceShipRecycleWatcher : QuestPart
{
    private bool enabled;
    public string inSignal;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            enabled = true;
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (enabled)
        {
            ScienceDepartmentInteractHandler.Instance.ScienceShipRecord = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref enabled, "enabled", defaultValue: false);
    }

}

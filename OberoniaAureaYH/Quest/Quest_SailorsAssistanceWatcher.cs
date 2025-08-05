using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_SailorsAssistanceWatcher : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;
    [NoTranslate]
    public SlateRef<string> outSignal;
    public SlateRef<int> sailorsCount;
    public SlateRef<Thing> shuttle;

    protected override bool TestRunInt(Slate slate)
    {
        return shuttle.GetValue(slate) is not null;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_SailorsAssistanceWatcher questPart_SailorsAssistanceWatcher = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
            outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate)),
            sailorsCount = sailorsCount.GetValue(slate),
            shuttle = shuttle.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_SailorsAssistanceWatcher);
    }
}


public class QuestPart_SailorsAssistanceWatcher : QuestPart
{
    public string inSignal;
    public string outSignal;
    public int sailorsCount;
    private int invitedCount;
    public Thing shuttle;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Values.Look(ref sailorsCount, "sailorsCount", 0);
        Scribe_Values.Look(ref invitedCount, "invitedCount", 0);
        Scribe_References.Look(ref shuttle, "shuttle");
    }

    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        outSignal = null;
        shuttle = null;
    }


    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            signal.args.TryGetArg("SUBJECT", out Pawn sailor);
            if (sailor is not null)
            {
                shuttle?.TryGetComp<CompShuttle>()?.requiredPawns?.Remove(sailor);
            }
            if (++invitedCount >= sailorsCount)
            {
                Find.SignalManager.SendSignal(new Signal(outSignal));
            }
        }
    }

}

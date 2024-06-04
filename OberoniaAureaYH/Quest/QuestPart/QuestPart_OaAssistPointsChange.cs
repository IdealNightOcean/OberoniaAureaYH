using RimWorld;
using Verse;

namespace OberoniaAurea;

public class QuestPart_OaAssistPointsChange : QuestPart
{
    public string inSignal;
    public int changePoints;
    public QuestPart_OaAssistPointsChange()
    { }

    public QuestPart_OaAssistPointsChange(string inSignal, int changePoints)
    {
        this.inSignal = inSignal;
        this.changePoints = changePoints;
    }
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            GameComponent_OberoniaAurea GC_OA = OberoniaAureaYHUtility.OA_GCOA;
            if (changePoints > 0)
            {
                GC_OA?.GetAssistPoints(changePoints);
            }
            else
            {
                GC_OA.UseAssistPoints(-changePoints);
            }
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref changePoints, "changePoints", 0);
    }
}

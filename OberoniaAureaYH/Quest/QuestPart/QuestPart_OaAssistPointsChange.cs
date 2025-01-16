using RimWorld;
using Verse;

namespace OberoniaAurea;

public class QuestPart_OaAssistPointsChange : QuestPart
{
    public string inSignal;
    public int changePoints;
    public QuestPart_OaAssistPointsChange() { }

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
            GameComponent_OberoniaAurea oaGameComp = OARatkin_MiscUtility.OAGameComp;
            if (oaGameComp != null)
            {
                if (changePoints > 0)
                {
                    oaGameComp.GetAssistPoints(changePoints);
                }
                else
                {
                    oaGameComp.UseAssistPoints(-changePoints);
                }
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

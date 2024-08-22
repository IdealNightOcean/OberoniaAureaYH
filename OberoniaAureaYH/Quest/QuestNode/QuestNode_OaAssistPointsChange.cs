using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;
public class QuestNode_OaAssistPointsChange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<int> changePoints;

    protected override bool TestRunInt(Slate slate)
    {
        return OberoniaAureaYHUtility.OAFaction != null;
    }
    protected override void RunInt()
    {
        if(OberoniaAureaYHUtility.OAFaction == null)
        {
            return;
        }
        Slate slate = QuestGen.slate;
        QuestPart_OaAssistPointsChange questPart_OaAssistPointsChange = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"),
            changePoints = changePoints.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_OaAssistPointsChange);
        QuestGen.slate.Set(storeAs.GetValue(slate), changePoints.GetValue(slate));

    }
}
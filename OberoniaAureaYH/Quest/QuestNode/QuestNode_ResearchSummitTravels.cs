using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//QuestNode: 研究峰会 - 参会旅行者 
public class QuestNode_ResearchSummitTravels : QuestNode_Incident
{
    protected override bool TestRunInt(Slate slate)
    {
        if (!slate.Exists("map"))
        {
            return false;
        }
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Map target = QuestGen.slate.Get<Map>("map");
        float points = QuestGen.slate.Get("points", 0f);
        QuestPart_Incident questPart_Incident = new()
        {
            incident = incidentDef.GetValue(slate)
        };
        IncidentParms incidentParms = new()
        {
            forced = true,
            target = target,
            points = points,
            faction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false),
        };
        if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
        {
            QuestGen.AddTextRequest("root", delegate (string x)
            {
                incidentParms.customLetterLabel = x;
            }, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
        }
        if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
        {
            QuestGen.AddTextRequest("root", delegate (string x)
            {
                incidentParms.customLetterText = x;
            }, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
        }
        questPart_Incident.SetIncidentParmsAndRemoveTarget(incidentParms);
        questPart_Incident.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
        QuestGen.quest.AddPart(questPart_Incident);
    }

}
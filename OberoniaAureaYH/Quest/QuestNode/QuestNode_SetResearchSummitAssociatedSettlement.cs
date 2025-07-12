using RimWorld.Planet;
using RimWorld.QuestGen;

namespace OberoniaAurea;

public class QuestNode_SetResearchSummitAssociatedSettlement : QuestNode
{
    public SlateRef<WorldObject_ResearchSummit> researchSummit;
    public SlateRef<Settlement> settlement;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;

        WorldObject_ResearchSummit researchSummit = this.researchSummit.GetValue(slate);
        Settlement settlement = this.settlement.GetValue(slate);
        if (researchSummit is not null && settlement is not null)
        {
            researchSummit.SetAssociateSettlement(settlement);
        }
    }
}
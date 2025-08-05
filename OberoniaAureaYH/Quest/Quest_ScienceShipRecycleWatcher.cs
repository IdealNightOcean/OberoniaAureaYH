using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_ScienceShipRecycleWatcher : QuestNode
{
    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsInteractAvailable() && ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.HasValue;
    }

    protected override void RunInt()
    {
        QuestGen.quest.AddPart(new QuestPart_ScienceShipRecycleWatcher());
    }
}


public class QuestPart_ScienceShipRecycleWatcher : QuestPart
{
    public string shipName;

    public override void PostQuestAdded()
    {
        base.PostQuestAdded();
        shipName = ScienceDepartmentInteractHandler.Instance?.ScienceShipRecord?.ShipName;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (!shipName.NullOrEmpty() && shipName == ScienceDepartmentInteractHandler.Instance?.ScienceShipRecord?.ShipName)
        {
            ScienceDepartmentInteractHandler.Instance.ScienceShipRecord = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref shipName, "shipName");
    }

}

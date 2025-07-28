using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_SetScienceShipName : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    public SlateRef<WorldObject> worldObject;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        string shipName = ScienceShipRecord.GenerateShipName();
        slate.Set(storeAs.GetValue(slate) ?? "shipName", shipName);

        ScienceShipLaunchSite launchSite = worldObject.GetValue(slate) as ScienceShipLaunchSite;
        launchSite?.SetShipName(shipName);
    }
}

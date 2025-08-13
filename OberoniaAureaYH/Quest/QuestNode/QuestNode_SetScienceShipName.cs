using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_InitScienceShip : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeShipNameAs;
    public SlateRef<WorldObject> worldObject;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        string shipName = ScienceShipRecord.GenerateShipName();
        slate.Set(storeShipNameAs.GetValue(slate) ?? "shipName", shipName);

        ScienceShipLaunchSite launchSite = worldObject.GetValue(slate) as ScienceShipLaunchSite;
        launchSite?.InitShip(shipName);
    }
}

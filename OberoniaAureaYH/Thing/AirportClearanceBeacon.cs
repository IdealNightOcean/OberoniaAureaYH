using RimWorld;
using Verse;

namespace OberoniaAurea;

public class AirportClearanceBeacon : Building
{
    public override void PostSwapMap()
    {
        base.PostSwapMap();
        if (Spawned)
        {
            Map map = Map;
            IntVec3 position = Position;
            Rot4 rotation = Rotation;

            Destroy();

            Thing thing = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_AirportClearanceBeaconDeactive);
            thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Direct, placedAction: null, extraValidator: null, rot: rotation);
        }
    }
}

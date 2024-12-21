using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class GameComponent_Hyacinth : GameComponent
{
    public Dictionary<Thing, float> antiStealth = [];
    private List<Thing> antiStealthThings;
    private List<float> antiStealthRadiusSquared;

    public Dictionary<Thing, float> ratkinHarmTurret = [];
    private List<Thing> ratkinHarmTurretThings;
    private List<float> ratkinHarmTurretRadiusSquared;


    public GameComponent_Hyacinth(Game game) { }

    //·´ÒþÉí¼ì²â
    public bool AntiStealthCheck(Thing thing)
    {
        Map map = thing.Map;
        IntVec3 thingPos = thing.Position;
        foreach (KeyValuePair<Thing, float> antiStealthThing in antiStealth)
        {
            Thing asThing = antiStealthThing.Key;
            if (ThingValidator(thing, asThing, map) && thingPos.DistanceToSquared(asThing.Position) <= antiStealthThing.Value)
            {
                return true;
            }
        }
        return false;
    }
    //¸ÉÈÅÅÚÌ¨¼ì²â
    public bool RatkinHarmTurretCheck(Thing thing)
    {
        Map map = thing.Map;
        IntVec3 thingPos = thing.Position;
        foreach (KeyValuePair<Thing, float> ratkinHarmTurretThing in ratkinHarmTurret)
        {
            Thing rhThing = ratkinHarmTurretThing.Key;
            if (ThingValidator(thing, rhThing, map) && thingPos.DistanceToSquared(rhThing.Position) <= ratkinHarmTurretThing.Value)
            {
                return true;
            }
        }
        return false;
    }

    protected static bool ThingValidator(Thing ct, Thing t, Map map)
    {
        if (t == null || !t.Spawned || t.Map != map)
        {
            return false;
        }
        if (ct.Faction == null || t.Faction == null)
        {
            return true;
        }
        return t.Faction.HostileTo(ct.Faction);
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref antiStealth, "antiStealth", LookMode.Reference, LookMode.Value, ref antiStealthThings, ref antiStealthRadiusSquared);
        Scribe_Collections.Look(ref ratkinHarmTurret, "ratkinHarmTurret", LookMode.Reference, LookMode.Value, ref ratkinHarmTurretThings, ref ratkinHarmTurretRadiusSquared);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            antiStealth.RemoveAll((KeyValuePair<Thing, float> t) => t.Key == null || t.Key.Destroyed);
            ratkinHarmTurret.RemoveAll((KeyValuePair<Thing, float> t) => t.Key == null || t.Key.Destroyed);
        }
    }
}

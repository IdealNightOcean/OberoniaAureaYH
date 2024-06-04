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

    public void RegisterAntiStealth(Thing thing, float radiusSquared)
    {
        if (thing == null)
        {
            return;
        }
        if (antiStealth.ContainsKey(thing))
        {
            antiStealth[thing] = radiusSquared;
        }
        else
        {
            antiStealth.Add(thing, radiusSquared);
        }
    }
    public void RegisterRatkinHarmTurret(Thing thing, float radiusSquared)
    {
        if (thing == null)
        {
            return;
        }
        if (ratkinHarmTurret.ContainsKey(thing))
        {
            ratkinHarmTurret[thing] = radiusSquared;
        }
        else
        {
            ratkinHarmTurret.Add(thing, radiusSquared);
        }
    }

    public void UnregisterAntiStealth(Thing thing)
    {
        if (thing != null)
        {
            antiStealth.Remove(thing);
        }
    }
    public void UnregisterRatkinHarmTurret(Thing thing)
    {
        if (thing != null)
        {
            ratkinHarmTurret.Remove(thing);
        }
    }

    public bool AntiStealthCheck(Map map, Thing thing)
    {
        IntVec3 thingPos = thing.Position;
        foreach (KeyValuePair<Thing, float> antiStealthThing in antiStealth)
        {
            if (ValidThing(antiStealthThing.Key, map) && thingPos.DistanceToSquared(antiStealthThing.Key.Position) <= antiStealthThing.Value)
            {
                return false;
            }
        }
        return true;
    }
    public bool RatkinHarmTurretCheck(Map map, Thing thing)
    {
        IntVec3 thingPos = thing.Position;
        foreach (KeyValuePair<Thing, float> ratkinHarmTurretThing in ratkinHarmTurret)
        {
            if (ValidThing(ratkinHarmTurretThing.Key, map) && thing.HostileTo(ratkinHarmTurretThing.Key) && thingPos.DistanceToSquared(ratkinHarmTurretThing.Key.Position) <= ratkinHarmTurretThing.Value)
            {
                return true;
            }
        }
        return false;
    }

    protected static bool ValidThing(Thing t, Map map)
    {
        if (t == null || map == null || !t.Spawned || t.Map != map)
        {
            return false;
        }
        return true;
    }
    public override void ExposeData()
    {
        Scribe_Collections.Look(ref antiStealth, "antiStealth", LookMode.Reference, LookMode.Value, ref antiStealthThings, ref antiStealthRadiusSquared);
        Scribe_Collections.Look(ref ratkinHarmTurret, "ratkinHarmTurret", LookMode.Reference, LookMode.Value, ref ratkinHarmTurretThings, ref ratkinHarmTurretRadiusSquared);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            antiStealth.RemoveAll((KeyValuePair<Thing, float> t) => t.Key == null);
            ratkinHarmTurret.RemoveAll((KeyValuePair<Thing, float> t) => t.Key == null);
        }
    }
}

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public abstract class FixedCaravan : WorldObject, IRenameable, IThingHolder
{
    private Material cachedMat;
    public override Material Material
    {
        get
        {
            if (cachedMat == null)
            {
                cachedMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
            }
            return cachedMat;
        }
    }
    public override Color ExpandingIconColor => base.Faction.Color;

    public string curName;
    public string RenamableLabel
    {
        get
        {
            return curName ?? BaseLabel;
        }
        set
        {
            curName = value;
        }
    }
    public string BaseLabel => def.label;
    public string InspectLabel => RenamableLabel;

    public ThingOwner GetDirectlyHeldThings()
    {
        return null;
    }
    public virtual void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    protected List<Pawn> occupants = [];
    public List<Pawn> AllPawns => occupants;
    public int PawnsCount => occupants.Count;
    protected List<Thing> containedItems = [];


    protected bool skillsDirty = true;
    protected readonly Dictionary<SkillDef, int> totalSkills = [];

    protected int ticksRemaining;

    public void AddItem(Thing thing)
    {
        containedItems.Add(thing);
    }
    public void RemoveItem(Thing thing)
    {
        containedItems.Remove(thing);
    }
    public bool AddPawn(Pawn pawn)
    {
        Caravan caravan = pawn.GetCaravan();
        if (caravan != null)
        {
            CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading, [pawn]);  
            if (!caravan.PawnsListForReading.Except(pawn).Any((Pawn p) => p.RaceProps.Humanlike))
            {
                foreach (Thing item2 in CaravanInventoryUtility.AllInventoryItems(caravan).ToList())
                {
                    Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, item2);
                    containedItems.Add(item2);
                    ownerOf.inventory.innerContainer.Remove(item2);
                }
            }
            pawn.ownership.UnclaimAll();
            caravan.RemovePawn(pawn);
            if (!caravan.PawnsListForReading.Any((Pawn p) => p.RaceProps.Humanlike))
            {
                containedItems.AddRange(caravan.AllThings);
                caravan.Destroy();
            }
        }
        pawn.holdingOwner?.Remove(pawn);
        if (Find.WorldPawns.Contains(pawn))
        {
            Find.WorldPawns.RemovePawn(pawn);
        }
        if (!occupants.Contains(pawn))
        {
            occupants.Add(pawn);
        }
        RecachePawn();
        return true;
    }
    public void RemovePawn(Pawn pawn)
    {
        pawn.GetCaravan()?.RemovePawn(pawn);
        pawn.holdingOwner?.Remove(pawn);
        occupants.Remove(pawn);
        RecachePawn();
    }
    public virtual void RecachePawn()
    {
        skillsDirty = true;
        foreach (Pawn pawn in containedItems.OfType<Pawn>().ToList())
        {
            containedItems.Remove(pawn);
            pawn.GetCaravan()?.RemovePawn(pawn);
            AddPawn(pawn);
        }
    }
    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObjectDef def, int initTicks = 0)
    {
        FixedCaravan fixedCaravan = (FixedCaravan)WorldObjectMaker.MakeWorldObject(def);
        fixedCaravan.curName = caravan.Name;
        fixedCaravan.Tile = caravan.Tile;
        fixedCaravan.ticksRemaining = initTicks;
        fixedCaravan.SetFaction(caravan.Faction);
        fixedCaravan.ConvertToFixCaravan(caravan);
        /*
        List<Pawn> caravanPawns = caravan.PawnsListForReading.ListFullCopy();
        foreach (Pawn pawn in caravanPawns)
        {
            fixedCaravan.AddPawn(pawn);
        }
        */
        return fixedCaravan;
    }
    public void ConvertToFixCaravan(Caravan caravan)
    {
        List<Thing> allInventoryItems = CaravanInventoryUtility.AllInventoryItems(caravan);
        foreach (Thing item in allInventoryItems)
        {
            Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, item);
            containedItems.Add(item);
            ownerOf.inventory.innerContainer.Remove(item);
        }
        List<Pawn> caravanPawns = caravan.PawnsListForReading.ListFullCopy();
        foreach(Pawn pawn in caravanPawns)
        {
            pawn.ownership.UnclaimAll();
            caravan.RemovePawn(pawn);
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            if (!occupants.Contains(pawn))
            {
                occupants.Add(pawn);
            }
        }
        containedItems.AddRange(caravan.AllThings);
        caravan.Destroy();
    }

    public override void SpawnSetup()
    {
        base.SpawnSetup();
        RecachePawn();

    }
    protected virtual void PreConvertToCaravanByPlayer()
    { }
    protected abstract void Notify_ConvertToCaravan();
    public void ConvertToCaravan()
    {
        Caravan caravan = CaravanMaker.MakeCaravan(occupants, base.Faction, base.Tile, addToWorldPawnsIfNotAlready: true);
        if (containedItems != null)
        {
            foreach (Thing item in containedItems.Except(caravan.AllThings))
            {
                caravan.AddPawnOrItem(item, addCarriedPawnToWorldPawnsIfAny: true);
            }
        }
        if (Find.WorldSelector.IsSelected(this))
        {
            Find.WorldSelector.Select(caravan, playSound: false);
        }
        Notify_ConvertToCaravan();
        Destroy();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        Command_Action command_Convert = new()
        {
            defaultLabel = "CommandReformCaravan".Translate(),
            defaultDesc = "CommandReformCaravanDesc".Translate(),
            icon = FormCaravanComp.FormCaravanCommand,
            Disabled = (occupants.Count == 0),
            action = delegate
            {
                PreConvertToCaravanByPlayer();
                ConvertToCaravan();
            }
        };
        yield return command_Convert;

    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref containedItems, "containedItems", LookMode.Deep);
        Scribe_Collections.Look(ref occupants, "occupants", LookMode.Deep);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref curName, "curName");
        RecachePawn();
    }
}
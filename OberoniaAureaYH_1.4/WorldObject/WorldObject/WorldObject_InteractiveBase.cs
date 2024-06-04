using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public abstract class WorldObject_InteractiveBase : WorldObject
{
    protected WorldObject associateWorldObject;
    private Material cachedMat;
    public override Material Material
    {
        get
        {
            if (cachedMat == null)
            {
                cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : base.Faction.Color, texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
            }
            return cachedMat;
        }
    }

    public void SetAssociateWorldObject(WorldObject worldObject)
    {
        if (worldObject != null)
        {
            associateWorldObject = worldObject;
        }
    }
    public abstract void Notify_CaravanArrived(Caravan caravan);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
    }
}

public abstract class WorldObject_MutiInteractiveBase : WorldObject_InteractiveBase
{
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Notify_CaravanArrived(caravan, 0);
    }
    public abstract void Notify_CaravanArrived(Caravan caravan, int visitType);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
    }
}
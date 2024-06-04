using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;
public class QuestNode_OAGenerateWorldObject : QuestNode_GenerateWorldObject
{
    public SlateRef<bool> worldObjectCanExist;
    public SlateRef<WorldObject> associateWorldObject;

    protected override bool TestRunInt(Slate slate)
    {
        if (worldObjectCanExist.GetValue(slate))
        {
            return true;
        }
        else
        {
            return !Find.WorldObjects.AllWorldObjects.Where(o => o.def == def.GetValue(slate)).Any();
        }
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!TestRunInt(slate))
        {
            return;
        }
        WorldObject_InteractiveBase worldObject = InitInteractiveBase(slate);
        if (storeAs.GetValue(slate) != null)
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), worldObject);
        }
    }
    protected WorldObject_InteractiveBase InitInteractiveBase(Slate slate)
    {
        WorldObject_InteractiveBase worldObject = (WorldObject_InteractiveBase)WorldObjectMaker.MakeWorldObject(def.GetValue(slate));
        worldObject.Tile = tile.GetValue(slate);
        if (faction.GetValue(slate) != null)
        {
            worldObject.SetFaction(faction.GetValue(slate));
        }
        if (associateWorldObject.GetValue(slate) != null)
        {
            worldObject.AssociateWorldObject = associateWorldObject.GetValue(slate);
        }
        return worldObject;
    }

}

//QuestNode：生成关联多派系的事件点
public class QuestNode_GenerateWorldObjectWithMutiFactions : QuestNode_OAGenerateWorldObject
{
    public SlateRef<List<Faction>> participantFactions;
    public SlateRef<Faction> associateFaction;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!TestRunInt(slate))
        {
            return;
        }
        WorldObject_WithMutiFactions worldObject = InitWorldObject(slate);
        if (storeAs.GetValue(slate) != null)
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), worldObject);
        }
    }

    protected WorldObject_WithMutiFactions InitWorldObject(Slate slate)
    {
        WorldObject_WithMutiFactions worldObject = (WorldObject_WithMutiFactions)InitInteractiveBase(slate);
        if (participantFactions.GetValue(slate) != null)
        {
            worldObject.SetParticipantFactions(participantFactions.GetValue(slate));
        }
        if (associateFaction.GetValue(slate) != null)
        {
            worldObject.AddParticipantFaction(associateFaction.GetValue(slate));
        }
        return worldObject;
    }

}

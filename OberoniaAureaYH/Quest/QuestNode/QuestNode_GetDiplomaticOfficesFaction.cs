using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;

//QuestNode：获取被斡旋派系
public class QuestNode_GetDiplomaticOfficesFaction : QuestNode_OAGetFaction
{
    public SlateRef<bool> diplomaticOfficesCantExist;

    protected override bool TryFindFaction(out Faction faction, Slate slate)
    {
        return (from x in Find.FactionManager.GetFactions(allowHidden: false)
                where IsGoodFaction(x, slate)
                select x).TryRandomElement(out faction);
    }

    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (faction == OberoniaAureaYHUtility.OAFaction)
        {
            return false;
        }
        if (faction.RelationKindWith(OberoniaAureaYHUtility.OAFaction) == FactionRelationKind.Hostile)
        {
            return false;
        }
        if (!base.IsGoodFaction(faction, slate))
        {
            return false;
        }
        if (WorldObjectContainFaction(faction, slate))
        {
            return false;
        }
        return true;
    }

}


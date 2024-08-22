﻿using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea;

//QuestNode：获取被斡旋派系
public class QuestNode_GetDiplomaticOfficesFaction : OberoniaAurea_Frame.QuestNode_GetFaction
{
    protected override bool TestRunInt(Slate slate)
    {
        if (OberoniaAureaYHUtility.OAFaction == null)
        {
            return false;
        }
        return base.TestRunInt(slate);
    }
    protected override void RunInt()
    {
        if (OberoniaAureaYHUtility.OAFaction == null)
        {
            return;
        }
        base.RunInt();
    }

    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (faction.RelationKindWith(OberoniaAureaYHUtility.OAFaction) == FactionRelationKind.Hostile)
        {
            return false;
        }
        return base.IsGoodFaction(faction, slate);
    }

}


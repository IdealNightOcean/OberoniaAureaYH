using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea;

//QuestNode：获取被斡旋派系
public class QuestNode_GetDiplomaticOfficesFaction : OberoniaAurea_Frame.QuestNode_GetFaction
{
    protected override bool TestRunInt(Slate slate)
    {
        if (OARatkin_MiscUtility.OAFaction == null)
        {
            return false;
        }
        return base.TestRunInt(slate);
    }
    protected override void RunInt()
    {
        if (OARatkin_MiscUtility.OAFaction == null)
        {
            return;
        }
        base.RunInt();
    }

    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (!base.IsGoodFaction(faction, slate))
        {
            return false;
        }
        if (faction.HostileTo(OARatkin_MiscUtility.OAFaction))
        {
            return false;
        }
        return true;
    }

}


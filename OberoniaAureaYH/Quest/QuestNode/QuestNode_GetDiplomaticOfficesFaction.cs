using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea;

//QuestNode：获取被斡旋派系
public class QuestNode_GetDiplomaticOfficesFaction : OberoniaAurea_Frame.QuestNode_GetFaction
{
    protected override bool TestRunInt(Slate slate)
    {
        if (ModUtility.OAFaction is null)
        {
            return false;
        }
        return base.TestRunInt(slate);
    }
    protected override void RunInt()
    {
        if (ModUtility.OAFaction is null)
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
        if (faction.HostileTo(ModUtility.OAFaction))
        {
            return false;
        }
        return true;
    }

}


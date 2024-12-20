using OberoniaAurea_Frame;
using RimWorld;
using System.Linq;
using Verse;


namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class OA_MiscUtility
{
    public static GameComponent_OberoniaAurea OAGameComp => Current.Game.GetComponent<GameComponent_OberoniaAurea>();
    public static MapComponent_OberoniaAurea OAMapComp(this Map map)
    {
        return map?.GetComponent<MapComponent_OberoniaAurea>();
    }

    public static Faction OAFaction => Find.FactionManager.FirstFactionOfDef(OA_MiscDefOf.OA_RK_Faction);
    public static bool IsOAFaction(this Faction faction, bool allowTemp = false)
    {
        if (faction == null)
        {
            return false;
        }
        if (faction.def == OA_MiscDefOf.OA_RK_Faction)
        {
            return allowTemp || !faction.temporary;
        }
        return false;
    }

    public static int AmountSendable(Map map, ThingDef def) //获取信标附近def物品数
    {
        return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                where t.def == def
                select t).Sum((Thing t) => t.stackCount);
    }

    public static void CallForAidFixedPoints(Map map, Faction faction, float points, PawnsArrivalModeDef raidArrivalMode = null) //呼叫固定点数支援，固定方式的工作支援
    {
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = faction,
            raidArrivalModeForQuickMilitaryAid = true,
            points = points,
            raidArrivalMode = raidArrivalMode ?? PawnsArrivalModeDefOf.EdgeDrop
        };
        OAFrame_MiscUtility.TryFireIncidentNow(IncidentDefOf.RaidFriendly, incidentParms);
    }
}



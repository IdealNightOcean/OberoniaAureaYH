using OberoniaAurea_Frame;
using RimWorld;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class ModUtility
{
    private static Faction oaFactionCache;
    public static Faction OAFaction => oaFactionCache ??= Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.OA_RK_Faction);

    public static CooldownRecordManager CooldownManager => GameComponent_OberoniaAurea.Instance.CooldownManager;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapComponent_OberoniaAurea GetOAMapComp(this Map map)
    {
        return map?.GetComponent<MapComponent_OberoniaAurea>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHashIntervalTick(int tickHashOffset, int interval)
    {
        return (Find.TickManager.TicksGame + tickHashOffset) % interval == 0;
    }

    public static bool IsOAFaction(this Faction faction, bool allowTemp = false)
    {
        if (faction is null)
        {
            return false;
        }
        if (faction.def == OARK_ModDefOf.OA_RK_Faction)
        {
            return allowTemp || !faction.temporary;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOAFactionAlly()
    {
        return OAFaction?.PlayerRelationKind == FactionRelationKind.Ally;
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
        OAFrame_MiscUtility.TryFireIncidentNow(IncidentDefOf.RaidFriendly, incidentParms, force: true);
    }



    internal static void Notify_GameStart()
    {
        oaFactionCache = Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.OA_RK_Faction);
    }

    internal static void ClearStaticCache()
    {
        oaFactionCache = null;
    }
}



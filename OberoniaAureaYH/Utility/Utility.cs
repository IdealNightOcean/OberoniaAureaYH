using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class ModUtility
{
    private static Faction OAFactionCache;
    public static Faction OAFaction => OAFactionCache ??= Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.OA_RK_Faction);

    public static MapComponent_OberoniaAurea GetOAMapComp(this Map map)
    {
        return map?.GetComponent<MapComponent_OberoniaAurea>();
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

    public static IEnumerable<Pawn> GetLendColonistsFromQuest(Quest quest)
    {
        return quest?.PartsListForReading?.OfType<QuestPart_LendColonistsToFaction>()
                                          .FirstOrFallback(null)
                                         ?.LentColonistsListForReading
                                         ?.OfType<Pawn>()
                                         ?? [];
    }

    public static List<ResearchProjectDef> GetResearchableProjects(int targetCount, int maxPotentialCount)
    {
        return DefDatabase<ResearchProjectDef>.AllDefs.Where(p => !p.IsFinished && !p.IsHidden)?.Take(maxPotentialCount)?.TakeRandomDistinct(targetCount);
    }

    public static ResearchProjectDef GetSignalResearchableProject(int maxPotentialCount)
    {
        return DefDatabase<ResearchProjectDef>.AllDefs.Where(p => !p.IsFinished && !p.IsHidden)?.Take(maxPotentialCount)?.RandomElementWithFallback(null);
    }

    public static void Notify_GameStart()
    {
        OAFactionCache = Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.OA_RK_Faction);
    }

    public static void ClearStaticCache()
    {
        OAFactionCache = null;
    }
}



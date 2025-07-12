using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

internal class IncidentWorker_NewYearEvent : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (OARatkin_MiscUtility.OAGameComp.newYearEventTriggeredOnce)
        {
            return false;
        }
        DateTime date = DateTime.Now;
        if (date.Month != 1 || date.Day > 3)
        {
            return false;
        }
        if (!parms.faction.IsOAFaction())
        {
            return false;
        }
        if (parms.faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }
        return base.CanFireNowSub(parms);
    }
    protected bool ResolveParams(IncidentParms parms)
    {
        if (OARatkin_MiscUtility.OAGameComp.newYearEventTriggeredOnce)
        {
            return false;
        }
        if (!parms.faction.IsOAFaction())
        {
            parms.faction = OARatkin_MiscUtility.OAFaction;
            if (parms.faction is null)
            {
                return false;
            }
        }
        if (parms.faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }
        Map map = (Map)parms.target;
        map ??= Find.AnyPlayerHomeMap;
        if (map is null)
        {
            return false;
        }
        IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
        if (!intVec.IsValid)
        {
            return false;
        }
        parms.spawnCenter = intVec;
        return map is not null;
    }
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!ResolveParams(parms))
        {
            return false;
        }
        Map map = (Map)parms.target;
        DateTime date = DateTime.Now;
        if (date.Month != 1 || date.Day > 3)
        {
            return false;
        }
        Pawn learder = parms.faction.leader;
        TaggedString letterLabel = "OARatkin_LetterLabel_NewYearEvent".Translate();
        TaggedString letterText;
        if (date.Day == 1)
        {
            letterText = "OARatkin_Letter_NewYearEvent".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle, learder.Named("LEADER"), map.Parent.Named("MAP"));
        }
        else
        {
            letterText = "OARatkin_Letter_NewYearEventLate".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle, learder.Named("LEADER"), map.Parent.Named("MAP"));
        }

        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(OARatkin_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 20);
        things.Add(ThingMaker.MakeThing(OARatkin_ThingDefOf.OARatkin_ResearchAnalyzer));
        ThingDef garlandDef = DefDatabase<ThingDef>.GetNamed("OA_RK_New_Hat_A");
        Thing garland = ThingMaker.MakeThing(garlandDef);
        if (garland is not null)
        {
            garland.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
            things.Add(garland);
        }

        DropPodUtility.DropThingGroupsNear(parms.spawnCenter, map, [things], 110, leaveSlag: true, forbid: false, allowFogged: false, faction: parms.faction);
        LookTargets lookTargets = new(parms.spawnCenter, map);

        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, lookTargets, relatedFaction: parms.faction);

        OARatkin_MiscUtility.OAGameComp.newYearEventTriggeredOnce = true;
        return true;

    }
}

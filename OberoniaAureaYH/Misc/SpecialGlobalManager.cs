using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class SpecialGlobalEventManager : IExposable
{
    public static SpecialGlobalEventManager Instance { get; private set; }

    private int cachedYear = 2026;
    private HashSet<string> triggeredSpecialEvents = [];

    private readonly int birthdayTickHash;

    public SpecialGlobalEventManager()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;

        birthdayTickHash = Rand.Range(0, 30000).HashOffset();
    }

    public static void ClearStaticCache() => Instance = null;

    public bool HasTriggeredSpecialEvents(string tag) => triggeredSpecialEvents.Contains(tag);
    public void MarkSpecialEventTriggered(string tag) => triggeredSpecialEvents.Add(tag);

    internal void Tick()
    {
        if (ModUtility.IsHashIntervalTick(birthdayTickHash, 60000))
        {
            CheckBirthdayWishes();
        }
    }

    internal void Notify_GameStart()
    {
        DateTime curDate = DateTime.Now;

        Map map = Find.AnyPlayerHomeMap;
        if (map is null)
            return;

        if (!ModUtility.IsOAFactionAlly())
            return;

        CheckNewYearEvent(curDate, map);
        CheckAprilFoolsEvent(curDate, map);
    }

    private void CheckNewYearEvent(DateTime curDate, Map map)
    {
        if (triggeredSpecialEvents.Contains("NewYear"))
            return;

        if (curDate.Month != 1 || curDate.Day > 3)
            return;

        List<Thing> gifts = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 20);
        gifts.Add(ThingMaker.MakeThing(OARK_ThingDefOf.OARatkin_ResearchAnalyzer));
        ThingDef garlandDef = DefDatabase<ThingDef>.GetNamed("OA_RK_New_Hat_A");
        Thing garland = ThingMaker.MakeThing(garlandDef);
        if (garland is not null)
        {
            garland.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
            gifts.Add(garland);
        }

        IncidentParms parms = new()
        {
            target = map,
            faction = ModUtility.OAFaction,
            questTag = "NewYear",

            customLetterDef = LetterDefOf.PositiveEvent,
            customLetterLabel = "OARatkin_LetterLabel_NewYearEvent".Translate(),
            gifts = gifts
        };

        Pawn learder = parms.faction.leader;
        if (curDate.Day == 1)
        {
            parms.customLetterText = "OARatkin_Letter_NewYearEvent".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle, learder.Named("LEADER"), map.Parent.Named("MAP"));
        }
        else
        {
            parms.customLetterText = "OARatkin_Letter_NewYearEventLate".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle, learder.Named("LEADER"), map.Parent.Named("MAP"));
        }

        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_SpecialGlobalEvent_GiftLetter, 2500, parms);
    }

    private void CheckAprilFoolsEvent(DateTime curDate, Map map)
    {
        if (triggeredSpecialEvents.Contains("AprilFools"))
            return;

        if (curDate.Month != 4 || curDate.Day > 3)
            return;

        List<Thing> gifts = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 10);
        gifts.Add(ThingMaker.MakeThing(OARK_ThingDefOf.OA_RK_New_Hat_A));
        gifts.Add(ThingMaker.MakeThing(OARK_ThingDefOf.OA_RK_New_Windbreaker_AA));

        IncidentParms parms = new()
        {
            target = map,
            faction = ModUtility.OAFaction,
            questTag = "AprilFools",

            customLetterDef = LetterDefOf.PositiveEvent,
            customLetterText = "OARK_LetterText_AprilFoolsEvent".Translate(),
            customLetterLabel = "OARK_LetterLabel_AprilFoolsEvent".Translate(),
            gifts = gifts,
        };

        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_SpecialGlobalEvent_GiftLetter, 2500, parms);
    }

    private void CheckBirthdayWishes()
    {
        OARK_IncidentDefOf.OARK_BirthdayWishes.Worker.TryExecute(new IncidentParms()
        {
            target = Find.AnyPlayerHomeMap,
            faction = ModUtility.OAFaction
        });
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref triggeredSpecialEvents, nameof(triggeredSpecialEvents), LookMode.Value);
        Scribe_Values.Look(ref cachedYear, nameof(cachedYear), 2026);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            DateTime curDate = DateTime.Now;
            if (curDate.Year > cachedYear)
            {
                triggeredSpecialEvents.Clear();
                cachedYear = curDate.Year;
            }
        }
    }
}

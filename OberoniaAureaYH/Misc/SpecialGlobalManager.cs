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

    private Dictionary<Pawn, int> birthdayPawnsOncePerYear = [];
    private int cachedGameYear = -1;


    private List<Pawn> birthdayPawnsOncePerYearKeys;
    private List<int> birthdayPawnsOncePerYearValues;

    public SpecialGlobalEventManager()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;

        birthdayTickHash = Rand.Range(0, 30000).HashOffset();
    }

    public static void ClearStaticCache() => Instance = null;

    public bool HasTriggeredSpecialEvents(string tag) => triggeredSpecialEvents.Contains(tag);
    public void MarkSpecialEventTriggered(string tag) => triggeredSpecialEvents.Add(tag);

    public bool IsPawnOnFirstBirthdayPerYear(Pawn pawn, bool addIfMiss)
    {
        if (!pawn.IsOnBirthday())
        {
            return false;
        }

        int ticksGame = Find.TickManager.TicksGame;
        if (birthdayPawnsOncePerYear.TryGetValue(pawn, out int firstBirthdayTick))
        {
            return ticksGame <= firstBirthdayTick + 60000;
        }
        else
        {
            firstBirthdayTick = ticksGame - ticksGame % 60000;
            birthdayPawnsOncePerYear[pawn] = firstBirthdayTick;
            return true;
        }
    }

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

        Pawn leader = parms.faction.leader;
        if (curDate.Day == 1)
        {
            parms.customLetterText = "OARatkin_Letter_NewYearEvent".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle.Named("TITLE"), leader.Named("LEADER"), map.Parent.Named("MAP"));
        }
        else
        {
            parms.customLetterText = "OARatkin_Letter_NewYearEventLate".Translate(parms.faction.Named("FACTION"), parms.faction.LeaderTitle.Named("TITLE"), leader.Named("LEADER"), map.Parent.Named("MAP"));
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
        ThingDef antigrainWarhead = DefDatabase<ThingDef>.GetNamed("Shell_AntigrainWarhead", errorOnFail: false);
        if (antigrainWarhead is not null)
        {
            gifts.Add(ThingMaker.MakeThing(antigrainWarhead));
        }

        Faction oaFaction = ModUtility.OAFaction;
        IncidentParms parms = new()
        {
            target = map,
            faction = oaFaction,
            questTag = "AprilFools",

            customLetterDef = LetterDefOf.PositiveEvent,
            customLetterLabel = "OARK_LetterLabel_AprilFoolsEvent".Translate(),
            customLetterText = "OARK_LetterText_AprilFoolsEvent".Translate(oaFaction.Named("FACTION"), oaFaction.LeaderTitle.Named("TITLE"), oaFaction.leader.Named("LEADER"), map.Parent.Named("MAP")),
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

        Scribe_Collections.Look(
            dict: ref birthdayPawnsOncePerYear,
            label: nameof(birthdayPawnsOncePerYear),
            keyLookMode: LookMode.Reference,
            valueLookMode: LookMode.Value,
            keysWorkingList: ref birthdayPawnsOncePerYearKeys,
            valuesWorkingList: ref birthdayPawnsOncePerYearValues);
        Scribe_Values.Look(ref cachedGameYear, nameof(cachedGameYear), -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            EnsureComponentsInit();
            DateTime curDate = DateTime.Now;
            if (curDate.Year > cachedYear)
            {
                triggeredSpecialEvents.Clear();
                cachedYear = curDate.Year;
            }
            if (GenDate.YearsPassed > cachedGameYear)
            {
                cachedGameYear = GenDate.YearsPassed;
                birthdayPawnsOncePerYear.Clear();
            }
        }
    }

    private void EnsureComponentsInit()
    {
        triggeredSpecialEvents ??= [];
        birthdayPawnsOncePerYear ??= [];
    }
}

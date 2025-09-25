using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

// 懒散 ----> 糟糕  ----> IF地狱
internal class ChoiceLetter_ScienceDepartmentFriendly : ChoiceLetter
{
    private List<bool> optionValider =
    [
        true,
        true,
        true,
        true,
        true,
    ];

    public void InitLetter()
    {
        bool hasValidProject = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => p.techprintCount > 0 && !p.TechprintRequirementMet).Any();
        int selCount = Rand.RangeInclusive(2, 3);
        if (selCount == 2 && hasValidProject)
        {
            optionValider[Rand.RangeInclusive(0, 1)] = false;
            optionValider[Rand.RangeInclusive(2, 3)] = false;
        }
        else
        {
            optionValider[4] = false;
            optionValider[Rand.RangeInclusive(0, 3)] = false;
        }
    }

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly)
            {
                yield return Option_Close;
                yield break;
            }

            if (optionValider[0])
            {
                yield return Option_I;
            }
            if (optionValider[1])
            {
                yield return Option_II;
            }
            if (optionValider[2])
            {
                yield return Option_III;
            }
            if (optionValider[3])
            {
                yield return Option_IV;
            }
            if (optionValider[4])
            {
                yield return Option_V;
            }
            yield return Option_Default;
            yield return Option_Postpone;
        }
    }


    private DiaOption Option_I => new("OARK_ScienceDepartmentFriendly_I".Translate())
    {
        action = Result_I,
        resolveTree = true
    };
    private DiaOption Option_II => new("OARK_ScienceDepartmentFriendly_II".Translate())
    {
        action = Result_II,
        resolveTree = true
    };
    private DiaOption Option_III => new("OARK_ScienceDepartmentFriendly_III".Translate())
    {
        action = Result_III,
        resolveTree = true
    };
    private DiaOption Option_IV => new("OARK_ScienceDepartmentFriendly_IV".Translate())
    {
        action = Result_IV,
        resolveTree = true
    };
    private DiaOption Option_V => new("OARK_ScienceDepartmentFriendly_V".Translate())
    {
        action = Result_V,
        resolveTree = true
    };
    private DiaOption Option_Default => new("OARK_ScienceDepartmentFriendly_Default".Translate())
    {
        action = Result_Default,
        resolveTree = true
    };

    private void Result_I()
    {
        OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_GravResearchExchange, 1000f, forced: true);

        Find.LetterStack.RemoveLetter(this);
        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_GravResearchExchange".Translate(),
                                       text: "OARK_Letter_GravResearchExchange".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: LookTargets.Invalid,
                                       relatedFaction: ModUtility.OAFaction);
    }

    private void Result_II()
    {
        Map map = Find.AnyPlayerHomeMap;
        if (map is not null)
        {
            OAFrame_DropPodUtility.DefaultDropThingOfDef(OARK_ThingDefOf.OARK_ComponentBox, 2, map, ModUtility.OAFaction);
        }
        Find.LetterStack.RemoveLetter(this);
    }

    private void Result_III()
    {
        Map map = Find.AnyPlayerHomeMap;
        if (map is not null)
        {
            List<Thing> rewardsI = OAFrame_MiscUtility.TryGenerateThing(OARK_RimWorldDefOf.Synthread, 100);
            List<Thing> rewardsII = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 40);
            List<Thing> rewardsIII = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_G, 5);

            IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingGroupsNear(dropCell, map, [rewardsI, rewardsII, rewardsIII], forbid: false, allowFogged: false, faction: ModUtility.OAFaction);
        }
        Find.LetterStack.RemoveLetter(this);
    }

    private void Result_IV()
    {
        Map map = Find.AnyPlayerHomeMap;
        if (map is not null)
        {
            Thing thing = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_AirportClearanceBeacon);
            if (thing.def.CanHaveFaction)
            {
                thing.SetFactionDirect(Faction.OfPlayer);
            }
            Thing miniedThing = MinifyUtility.TryMakeMinified(thing);
            IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(dropCell, map, [miniedThing], forbid: false, allowFogged: false, faction: ModUtility.OAFaction);
        }
        Find.LetterStack.RemoveLetter(this);
    }
    private void Result_V()
    {
        Map map = Find.AnyPlayerHomeMap;
        if (map is not null)
        {
            ThingDef techPrintDef = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => p.techprintCount > 0 && !p.TechprintRequirementMet).Take(5)?.RandomElementWithFallback(null)?.Techprint;
            if (techPrintDef is not null)
            {
                OAFrame_DropPodUtility.DefaultDropSingleThingOfDef(techPrintDef, map, ModUtility.OAFaction);
            }
        }
        Find.LetterStack.RemoveLetter(this);
    }

    private void Result_Default()
    {
        ModUtility.OAFaction?.TryAffectGoodwillWith(Faction.OfPlayer, 20);
        OAInteractHandler.Instance.AdjustAssistPoints(5);
        ScienceDepartmentInteractHandler.Instance?.AdjustGravTechAssistPoint(50);
        Find.LetterStack.RemoveLetter(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref optionValider, "optionValider", LookMode.Value);
    }
}

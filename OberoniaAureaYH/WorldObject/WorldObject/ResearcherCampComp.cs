using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class WorldObjectCompProperties_ResearcherCamp : WorldObjectCompProperties
{
    public WorldObjectCompProperties_ResearcherCamp()
    {
        compClass = typeof(ResearcherCampComp);
    }
}

public class ResearcherCampComp : WorldObjectComp
{
    private bool active = false;

    private const int ReduceGoodwill = -55;
    public override void PostMapGenerate()
    {
        if (!active)
        {
            return;
        }
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        if (oaFaction != null)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, ReduceGoodwill, canSendMessage: false, canSendHostilityLetter: false, OARatkin_HistoryEventDefOf.OA_AttackResearcherCamp);
            Find.LetterStack.ReceiveLetter("OA_LetterLabelAttackResearcherCamp".Translate(), "OA_LetterAttackResearcherCamp".Translate(oaFaction.NameColored, oaFaction.leader, ReduceGoodwill), LetterDefOf.NegativeEvent, parent, oaFaction);
        }
    }
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        if (!active)
        {
            yield break;
        }
        foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_RobResearcherCamp.GetFloatMenuOptions(caravan, parent))
        {
            yield return floatMenuOption;
        }
    }

    public void SetActivate(bool active)
    {
        this.active = active;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref active, "active_ResearcherCamp", defaultValue: false);
    }

    public static Faction GenerateTempCampFaction()
    {
        List<FactionRelation> list = [];
        foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
        {
            if (!item2.def.permanentEnemy)
            {
                if (item2 == Faction.OfPlayer)
                {
                    list.Add(new FactionRelation
                    {
                        other = item2,
                        kind = FactionRelationKind.Hostile
                    });
                }
                else
                {
                    list.Add(new FactionRelation
                    {
                        other = item2,
                        kind = FactionRelationKind.Neutral
                    });
                }
            }
        }
        FactionGeneratorParms parms = new(DefDatabase<FactionDef>.AllDefsListForReading.Where(FactionDefUseable).RandomElement(), default, true);
        parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false);
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
        faction.temporary = true;
        Find.FactionManager.Add(faction);
        return faction;
    }

    private static bool FactionDefUseable(FactionDef def)
    {
        if (def.humanlikeFaction && !def.pawnGroupMakers.NullOrEmpty() && def.pawnGroupMakers.Any((PawnGroupMaker gm) => gm.kindDef == PawnGroupKindDefOf.Settlement))
        {
            return true;
        }
        return false;
    }
}


public class CaravanArrivalAction_RobResearcherCamp : CaravanArrivalAction
{
    private const int SilverNum = 300;

    private WorldObject site;
    public override string Label => "OA_RobResearcherCamp".Translate(site.Label);
    public override string ReportString => "CaravanVisiting".Translate(site.Label);
    public CaravanArrivalAction_RobResearcherCamp()
    {
    }

    public CaravanArrivalAction_RobResearcherCamp(WorldObject site)
    {
        this.site = site;
    }
    public override void Arrived(Caravan caravan)
    {
        Robbery(caravan, site);
    }
    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (site != null && site.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(site);
    }
    private static void Robbery(Caravan caravan, WorldObject worldObject)
    {
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Silver, SilverNum);
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelRobResearcherCamp".Translate(), "OA_LetterRobResearcherCamp".Translate(SilverNum), LetterDefOf.PositiveEvent, caravan);
        worldObject.Destroy();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
    }

    public static FloatMenuAcceptanceReport CanVisit(WorldObject site)
    {
        if (site == null || !site.Spawned)
        {
            return false;
        }
        EnterCooldownComp enterCooldown = site.GetComponent<EnterCooldownComp>();
        if (enterCooldown != null && enterCooldown.BlocksEntering)
        {
            return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(enterCooldown.TicksLeft.ToStringTicksToPeriod()));
        }
        return true;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject site)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(site), () => new CaravanArrivalAction_RobResearcherCamp(site), "OA_RobResearcherCamp".Translate(site.Label), caravan, site.Tile, site);
    }
}
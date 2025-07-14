using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;
public class RoyalTitlePermitWorker_OACallAid : RoyalTitlePermitWorker_Targeted
{
    protected Faction calledFaction;

    protected float biocodeChance;

    [Unsaved]
    protected OARatkinGroupExtension modEx_RG;
    public OARatkinGroupExtension ModEx_RG => modEx_RG ??= def.GetModExtension<OARatkinGroupExtension>();

    public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
    {
        if (AidDisabled(map, pawn, faction, out string reason))
        {
            yield return new FloatMenuOption(def.LabelCap + ": " + reason, null);
            yield break;
        }
        if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(pawn.MapHeld, faction))
        {
            yield return new FloatMenuOption(def.LabelCap + ": " + "HostileVisitorsPresent".Translate(), null);
            yield break;
        }
        Action action = null;
        string description = def.LabelCap + ": ";
        if (FillAidOption(pawn, faction, ref description, out bool free))
        {
            action = delegate
            {
                BeginCallAid(pawn, map, faction, free);
            };
        }
        yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
    }

    protected void BeginCallAid(Pawn caller, Map map, Faction faction, bool free, float biocodeChance = 1f)
    {
        IEnumerable<Faction> source = from f in (from p in map.mapPawns.AllPawnsSpawned
                                                 where p.Faction is not null && !p.Faction.IsPlayer && p.Faction != faction && !p.IsPrisonerOfColony
                                                 select p.Faction).Distinct()
                                      where f.HostileTo(Faction.OfPlayer) && !faction.HostileTo(f)
                                      select f;
        if (source.Any())
        {
            Find.WindowStack.Add(new Dialog_MessageBox(text: "CommandCallRoyalAidWarningNonHostileFactions".Translate(faction, source.Select(f => f.NameColored.Resolve()).ToCommaList()),
                                                       buttonAText: "Confirm".Translate(), buttonAAction: Call,
                                                       buttonBText: "GoBack".Translate()));
        }
        else
        {
            Call();
        }
        void Call()
        {
            targetingParameters = new()
            {
                canTargetLocations = true,
                canTargetSelf = false,
                canTargetPawns = false,
                canTargetFires = false,
                canTargetBuildings = false,
                canTargetItems = false,
                validator = delegate (TargetInfo target)
                {
                    if (def.royalAid.targetingRange > 0f && target.Cell.DistanceTo(caller.Position) > def.royalAid.targetingRange)
                    {
                        return false;
                    }
                    if (target.Cell.Fogged(map) || !DropCellFinder.CanPhysicallyDropInto(target.Cell, map, canRoofPunch: true))
                    {
                        return false;
                    }
                    return target.Cell.GetEdifice(map) is null && !target.Cell.Impassable(map);
                }
            };
            base.caller = caller;
            base.map = map;
            calledFaction = faction;
            base.free = free;
            this.biocodeChance = biocodeChance;
            Find.Targeter.BeginTargeting(this);
        }
    }

    public override void OrderForceTarget(LocalTargetInfo target)
    {
        CallAid(caller, map, target.Cell, calledFaction, free, biocodeChance);
    }
    protected void CallAid(Pawn caller, Map map, IntVec3 spawnPos, Faction faction, bool free, float biocodeChance = 1f)
    {
        bool callSuccess = false;
        callSuccess = CallNormalAid(map, spawnPos, faction, biocodeChance) || callSuccess;
        OARatkinGroupExtension modEx_RG = ModEx_RG;
        if (modEx_RG is not null)
        {
            foreach (OARatkinGroup ratkinGroup in modEx_RG.extraGroups)
            {
                callSuccess = CallOAAid(ratkinGroup, map, spawnPos, faction, biocodeChance) || callSuccess;
            }
        }
        if (callSuccess)
        {
            faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            if (!free)
            {
                caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
            }
            caller.royalty.GetPermit(def, faction).Notify_Used();
        }
        else
        {
            Log.Error(string.Concat("Could not send aid to map ", map, " from faction ", faction));
        }
    }
    protected bool CallNormalAid(Map map, IntVec3 spawnPos, Faction faction, float biocodeChance = 1f)
    {
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = faction,
            raidArrivalModeForQuickMilitaryAid = true,
            biocodeApparelChance = biocodeChance,
            biocodeWeaponsChance = biocodeChance,
            spawnCenter = spawnPos
        };
        if (def.royalAid.pawnKindDef is not null)
        {
            incidentParms.pawnKind = def.royalAid.pawnKindDef;
            incidentParms.pawnCount = def.royalAid.pawnCount;
        }
        else
        {
            incidentParms.points = def.royalAid.points;
        }
        return IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
    }
    protected bool CallOAAid(OARatkinGroup ratkinGroup, Map map, IntVec3 spawnPos, Faction faction, float biocodeChance = 1f)
    {
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = faction,
            raidArrivalModeForQuickMilitaryAid = true,
            sendLetter = false, //不提示信封
            biocodeApparelChance = biocodeChance,
            biocodeWeaponsChance = biocodeChance,
            spawnCenter = spawnPos
        };
        if (ratkinGroup.pawnKindDef is not null)
        {
            incidentParms.pawnKind = ratkinGroup.pawnKindDef;
            incidentParms.pawnCount = ratkinGroup.pawnCount;
        }
        else
        {
            incidentParms.points = def.royalAid.points;
        }
        return IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
    }
}

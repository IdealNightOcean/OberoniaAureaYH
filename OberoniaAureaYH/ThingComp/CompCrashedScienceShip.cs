using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class CompProperties_CrashedScienceShip : CompProperties_Hackable
{
    public HediffDef weakGravityHediff;
    public HediffDef strongGravityHediff;
    public HediffDef disorderedGravityHediff;
    public HediffDef disorderedMagnetismHediff;

    public JobDef mechanicalJob;
    public JobDef gravitationalJob;
    public JobDef hyperthermiaJob;
    public JobDef gravityAdjustmentJob;

    public GameConditionDef shipGameCondition;

    public CompProperties_CrashedScienceShip()
    {
        compClass = typeof(CompCrashedScienceShip);
    }
}

public class CompCrashedScienceShip : CompHackable
{
    private static readonly int[] raidInterval = [50000, 40000, 30000, 22500, 15000];

    private static readonly Texture2D increaseGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_IncreaseGrav");
    private static readonly Texture2D decreaseGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_Decrease");
    private static readonly Texture2D reassignmentGravIcon = ContentFinder<Texture2D>.Get("UI/OARK_ReassignmentGrav");

    private new CompProperties_CrashedScienceShip Props => (CompProperties_CrashedScienceShip)props;

    private ScienceShipRecord shipRecord;
    public ScienceShipRecord ShipRecord => shipRecord;

    private int signalStrength = 1;
    private float raidMulti = 2f;
    private int ticksToNextRaid = 30000;

    private int extaRaidRemaining;
    private int ticksToNextExtaRaid = 32500;

    private int mainGravIndex;
    private int subGravIndex;

    private int gravityAdjuestType;

    private int gravitationalCount;
    private int mechanicalCount;

    private int hyperthermiaLeft = -1;
    private float hyperthermiaReduceRate = 1f;

    private bool quizResearcher;

    private bool allTroubleResolved;
    private int ticksToUpload = 10000;
    private bool dataUploaded;

    private GameCondition_CrashedScienceShip shipGameCondition;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref shipRecord, "shipRecord");

        Scribe_Values.Look(ref signalStrength, "signalStrength", 1);
        Scribe_Values.Look(ref raidMulti, "raidMulti", 2f);
        Scribe_Values.Look(ref ticksToNextRaid, "ticksToNextRaid", 0);

        Scribe_Values.Look(ref extaRaidRemaining, "extaRaidRemaining", 0);
        Scribe_Values.Look(ref ticksToNextExtaRaid, "ticksToNextExtaRaid", 0);

        Scribe_Values.Look(ref mainGravIndex, "mainGravIndex", 1);
        Scribe_Values.Look(ref subGravIndex, "subGravIndex", 1);

        Scribe_Values.Look(ref gravityAdjuestType, "gravityAdjuestType", 0);

        Scribe_Values.Look(ref hyperthermiaLeft, "hyperthermiaLeft", -1);
        Scribe_Values.Look(ref hyperthermiaReduceRate, "hyperthermiaReduceRate", 1f);

        Scribe_Values.Look(ref quizResearcher, "quizResearcher", defaultValue: false);

        Scribe_Values.Look(ref allTroubleResolved, "allTroubleResolved", defaultValue: false);
        Scribe_Values.Look(ref ticksToUpload, "ticksToUpload", 10000);
        Scribe_Values.Look(ref dataUploaded, "dataUploaded", defaultValue: false);

        Scribe_References.Look(ref shipGameCondition, "shipGameCondition");
    }

    public void InitCrashedScienceShip()
    {
        if (ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.HasValue)
        {
            shipRecord = ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.Value;
        }
        else
        {
            shipRecord = ScienceShipRecord.GenerateShip();
        }

        isHackable = shipRecord.HasTrouble(ScienceShipRecord.TroubleType.InformationBase);

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Mechanical))
        {
            mechanicalCount = Rand.RangeInclusive(4, 6);
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Gravitational))
        {
            gravitationalCount = Rand.RangeInclusive(2, 3);
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Hyperthermia))
        {
            isHackable = false;
            hyperthermiaLeft = 60000;
        }

        if (shipRecord.TypeOfShip == ScienceShipRecord.ShipType.TurbulentRegion)
        {
            extaRaidRemaining = 5;
            ticksToNextExtaRaid = 32500;
            raidMulti = 2.5f;
        }
        if (shipRecord.TypeOfShip == ScienceShipRecord.ShipType.Fragile)
        {
            parent.HitPoints = 2500;
        }

        mainGravIndex = subGravIndex = 3;
        switch (shipRecord.EnvironmentAffect)
        {
            case ScienceShipRecord.EnvirAffectType.WeakGravity:
                mainGravIndex = subGravIndex = Rand.RangeInclusive(1, 2);
                break;
            case ScienceShipRecord.EnvirAffectType.StrongWeakGravity:
                mainGravIndex = subGravIndex = Rand.RangeInclusive(4, 5);
                break;
            case ScienceShipRecord.EnvirAffectType.DisorderedGravity:
                mainGravIndex = Rand.RangeInclusive(1, 4);
                do
                {
                    subGravIndex = Rand.RangeInclusive(1, 4);
                }
                while (subGravIndex == mainGravIndex);
                break;
            case ScienceShipRecord.EnvirAffectType.DisorderedMagnetism:
                mainGravIndex = Rand.RangeInclusive(1, 5);
                if (mainGravIndex == 5)
                {
                    subGravIndex = Rand.RangeInclusive(1, 4);
                }
                else
                {
                    subGravIndex = 5;
                }
                break;
            default:
                mainGravIndex = subGravIndex = 3;
                break;
        }

        RecheckSignalStrength();
    }

    public override void CompTickInterval(int delta)
    {
        if (allTroubleResolved)
        {
            if (!dataUploaded && (ticksToUpload -= delta) <= 0)
            {
                FinishDataUpload();
            }
        }
        else
        {
            RaidTickInterval(delta);
            if (hyperthermiaLeft > 0)
            {
                HyperthermiaTickInterval(delta);
            }
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        QuestUtility.SendQuestTargetSignals(MapParent.questTags, "CrashShipSpawned", this.Named("SUBJECT"));
        if (!respawningAfterLoad)
        {
            shipGameCondition = (GameCondition_CrashedScienceShip)GameConditionMaker.MakeCondition(Props.shipGameCondition, duration: 600000);
            shipGameCondition.conditionCauser = parent;
            parent.MapHeld.gameConditionManager.RegisterCondition(shipGameCondition);
            RecheckGameCondition();

            if (shipRecord.TypeOfShip == ScienceShipRecord.ShipType.TurbulentRegion)
            {
                IncidentParms parms = new()
                {
                    target = parent.MapHeld,
                    faction = ModUtility.OAFaction
                };
                OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_ScienceShip_OASupport, 1200, parms);
            }
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        if (!dataUploaded)
        {
            QuestUtility.SendQuestTargetSignals(map.Parent.questTags, "DeSpawnedBeforeDataUpload", this.Named("SUBJECT"));
        }
        shipGameCondition?.End();
        shipGameCondition = null;
        base.PostDeSpawn(map, mode);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        if (!dataUploaded)
        {
            QuestUtility.SendQuestTargetSignals(previousMap.Parent.questTags, "DestoryedBeforeDataUpload", this.Named("SUBJECT"));
        }
        shipGameCondition?.End();
        shipGameCondition = null;
        base.PostDestroy(mode, previousMap);
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Hyperthermia))
        {
            yield return new FloatMenuOption("OARK_ScienceShip_HyperthermiaJob".Translate(), action: delegate { MakeJob(selPawn, Props.hyperthermiaJob); });
            yield break;
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.InformationBase))
        {
            if (isHackable)
            {
                yield return new FloatMenuOption("Hack".Translate(parent.Label), delegate { MakeJob(selPawn, OARK_ModDefOf.OARK_Job_Hack); });
            }
            else
            {
                yield return new FloatMenuOption("CannotHack".Translate(parent.Label), null);
            }
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Mechanical))
        {
            yield return new FloatMenuOption("OARK_ScienceShip_MechanicalJob".Translate(), action: delegate { MakeJob(selPawn, Props.mechanicalJob); });
        }
        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Gravitational))
        {
            yield return new FloatMenuOption("OARK_ScienceShip_GravitationalJob".Translate(), action: delegate { MakeJob(selPawn, Props.gravitationalJob); });
        }
        else
        {
            yield return new FloatMenuOption("OARK_ScienceShip_IncreaseGravityJob".Translate(), action: delegate { MakeGravityAdjustmentJob(selPawn, 1); });
            yield return new FloatMenuOption("OARK_ScienceShip_DecreaseGravityJob".Translate(), action: delegate { MakeGravityAdjustmentJob(selPawn, 2); });
            yield return new FloatMenuOption("OARK_ScienceShip_ReassignmentGravityJob".Translate(), action: delegate { MakeGravityAdjustmentJob(selPawn, 3); });
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        if (allTroubleResolved)
        {
            yield break;
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Hyperthermia))
        {
            Command_Action Command_Gravitational = new()
            {
                defaultLabel = "OARK_ScienceShip_Hyperthermia".Translate(),
                defaultDesc = "OARK_ScienceShip_HyperthermiaDesc".Translate(),
                icon = null,
                action = delegate { JobFloatMenu(Props.hyperthermiaJob); }
            };

            yield return Command_Gravitational;
            yield break;
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Mechanical))
        {
            Command_Action Command_Gravitational = new()
            {
                defaultLabel = "OARK_ScienceShip_MechanicalJob".Translate(),
                defaultDesc = "OARK_ScienceShip_MechanicalJobDesc".Translate(),
                icon = null,
                action = delegate { JobFloatMenu(Props.mechanicalJob); }
            };

            yield return Command_Gravitational;
        }

        if (!quizResearcher && shipRecord.HasTrouble(ScienceShipRecord.TroubleType.InformationBase))
        {
            Command_Action Command_Gravitational = new()
            {
                defaultLabel = "OARK_ScienceShip_QuizResearcherJob".Translate(),
                defaultDesc = "OARK_ScienceShip_QuizResearcherJobDesc".Translate(),
                icon = ModUtility.OADipIcon,
                action = QuizResearcher
            };

            yield return Command_Gravitational;
        }

        Command_Action Command_IncreaseGravity = new()
        {
            defaultLabel = "OARK_ScienceShip_IncreaseGravityJob".Translate(),
            defaultDesc = "OARK_ScienceShip_IncreaseGravityJobDesc".Translate(),
            icon = increaseGravIcon,
            action = delegate
            {
                GravityAdjustmentFloatMenu(1);
            }
        };
        Command_Action Command_DecreaseGravity = new()
        {
            defaultLabel = "OARK_ScienceShip_DecreaseGravityJob".Translate(),
            defaultDesc = "OARK_ScienceShip_DecreaseGravityJobDesc".Translate(),
            icon = decreaseGravIcon,
            action = delegate
            {
                GravityAdjustmentFloatMenu(2);
            }
        };
        Command_Action Command_ReassignmentGravity = new()
        {
            defaultLabel = "OARK_ScienceShip_ReassignmentGravityJob".Translate(),
            defaultDesc = "OARK_ScienceShip_ReassignmentGravityJobDesc".Translate(),
            icon = reassignmentGravIcon,
            action = delegate
            {
                GravityAdjustmentFloatMenu(3);
            }
        };

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Gravitational))
        {
            Command_Action Command_Gravitational = new()
            {
                defaultLabel = "OARK_ScienceShip_GravitationalJob".Translate(),
                defaultDesc = "OARK_ScienceShip_GravitationalJobDesc".Translate(),
                icon = null,
                action = delegate { JobFloatMenu(Props.gravitationalJob); }
            };

            yield return Command_Gravitational;

            Command_IncreaseGravity.Disable("OARK_ScienceShip_HasGravitational".Translate());
            Command_DecreaseGravity.Disable("OARK_ScienceShip_HasGravitational".Translate());
            Command_ReassignmentGravity.Disable("OARK_ScienceShip_HasGravitational".Translate());
        }

        yield return Command_IncreaseGravity;
        yield return Command_DecreaseGravity;
        yield return Command_ReassignmentGravity;
    }

    protected override StringBuilder CompInspectStringExtraBuilder()
    {
        StringBuilder sb = new();

        if (allTroubleResolved)
        {
            sb.AppendInNewLine("OARK_ScienceShip_AllTroubleResolved".Translate().Colorize(Color.green));
            if (!dataUploaded)
            {
                sb.AppendInNewLine("OARK_ScienceShip_DataUploading".Translate(ticksToUpload.ToStringTicksToPeriod()));
            }
            return sb;
        }

        sb.AppendInNewLine("OARK_ScienceShip_SignalStrength".Translate(signalStrength));
        sb.AppendInNewLine("--------");
        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Hyperthermia))
        {
            sb.AppendInNewLine("OARK_ScienceShip_HyperthermiaInfo".Translate(hyperthermiaLeft, hyperthermiaReduceRate));
            sb.AppendInNewLine("OARK_ScienceShip_HyperthermiaBlock".Translate().Colorize(new Color(1f, 0.5f, 0f)));
            sb.AppendInNewLine("--------");
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.InformationBase))
        {
            sb.AppendInNewLine(base.CompInspectStringExtraBuilder().ToString());
        }

        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Mechanical))
        {
            sb.AppendInNewLine("OARK_ScienceShip_MechanicalInfo".Translate(6 - mechanicalCount, 6));
        }
        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.Gravitational))
        {
            sb.AppendInNewLine("OARK_ScienceShip_GravitationalInfo".Translate(3 - gravitationalCount, 3));
        }

        if (shipRecord.TypeOfShip == ScienceShipRecord.ShipType.Disaster)
        {
            sb.AppendInNewLine("--------");
            float workIncrease = shipRecord.TroubleCount * 0.25f;
            sb.AppendInNewLine("OARK_ScienceShip_AllKeyTrouble".Translate(workIncrease.ToStringPercent()));
        }
        else if (shipRecord.MainTrouble != ScienceShipRecord.TroubleType.None)
        {
            sb.AppendInNewLine("--------");
            sb.AppendInNewLine("OARK_ScienceShip_KeyTrouble".Translate());
            sb.Append(": ");
            sb.Append(ScienceShipRecord.GetShipTroubleLabel(shipRecord.MainTrouble).Translate().Colorize(Color.red));
            float workIncrease = (shipRecord.TroubleCount - 1) * 0.25f;
            sb.AppendInNewLine("OARK_ScienceShip_TroubleWorkIncrease".Translate(workIncrease.ToStringPercent()));
        }

        if (Prefs.DevMode)
        {
            sb.AppendInNewLine("--------");
            sb.AppendInNewLine($"mainGravIndex:{mainGravIndex}, subGravIndex:{subGravIndex}");
            sb.AppendInNewLine($"ticksToNextRaid:{ticksToNextRaid}");
            sb.AppendInNewLine($"extaRaidRemaining:{extaRaidRemaining}, ticksToNextExtaRaid:{ticksToNextExtaRaid}");
        }

        return sb;
    }

    public void ActivateCoolingDevice()
    {
        hyperthermiaReduceRate = 6f;
        Messages.Message("OARK_ScienceShip_CoolingDeviceActivated".Translate(), MessageTypeDefOf.PositiveEvent, historical: true);
    }

    public void MechanicalRepaired(Pawn pawn)
    {
        mechanicalCount--;
        pawn.skills?.Learn(SkillDefOf.Construction, 2000f);
        Messages.Message("OARK_ScienceShip_MechanicalRepaired".Translate(), MessageTypeDefOf.PositiveEvent);
        Messages.Message("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Construction.LabelCap, 2000), MessageTypeDefOf.PositiveEvent);
        if (mechanicalCount <= 0)
        {
            RemoveTrouble(ScienceShipRecord.TroubleType.Mechanical);
        }
    }

    public void GravitationalRepaired(Pawn pawn)
    {
        float successChance = pawn.GetStatValue(StatDefOf.ResearchSpeed) * 0.4f + 0.1f;
        if (Rand.Chance(successChance))
        {
            gravitationalCount--;
            pawn.skills?.Learn(SkillDefOf.Intellectual, 1500f);
            pawn.skills?.Learn(SkillDefOf.Crafting, 1500f);
            Messages.Message("OARK_ScienceShip_GravitationalRepaireSuccess".Translate(), MessageTypeDefOf.PositiveEvent);
            Messages.Message("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Intellectual.LabelCap, 1500), MessageTypeDefOf.PositiveEvent);
            Messages.Message("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Crafting.LabelCap, 1500), MessageTypeDefOf.PositiveEvent);
            if (gravitationalCount <= 0)
            {
                RemoveTrouble(ScienceShipRecord.TroubleType.Gravitational);
            }
        }
        else
        {
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 4f));
            pawn.stances?.stunner.StunFor(600, pawn);
            Messages.Message("OARK_ScienceShip_GravitationalRepaireFail".Translate(), MessageTypeDefOf.NegativeEvent);
        }
    }

    private void HyperthermiaSubsided()
    {
        RemoveTrouble(ScienceShipRecord.TroubleType.Hyperthermia);
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_HyperthermiaSubsided".Translate()));
        if (shipRecord.HasTrouble(ScienceShipRecord.TroubleType.InformationBase))
        {
            isHackable = true;
        }
    }

    public override void FinishHack()
    {
        base.FinishHack();
        RemoveTrouble(ScienceShipRecord.TroubleType.InformationBase);
    }

    public void DoGravityAdjuestment()
    {
        if (gravityAdjuestType == 1)
        {
            IncreaseGravity();
        }
        else if (gravityAdjuestType == 2)
        {
            DecreaseGravity();
        }
        else if (gravityAdjuestType == 3)
        {
            ReassignmentGravity();
        }
        gravityAdjuestType = 0;
    }

    private void HyperthermiaTickInterval(int delta)
    {
        if ((hyperthermiaLeft -= Mathf.RoundToInt(delta * hyperthermiaReduceRate)) <= 0)
        {
            hyperthermiaLeft = -1;
            HyperthermiaSubsided();
        }
    }

    private void RaidTickInterval(int delta)
    {
        if ((ticksToNextRaid -= delta) <= 0)
        {
            ticksToNextRaid = raidInterval[signalStrength - 1];
            IncidentParms parms = new()
            {
                target = parent.MapHeld,
                points = StorytellerUtility.DefaultThreatPointsNow(parent.MapHeld) * raidMulti,
                faction = Find.FactionManager.RandomEnemyFaction(),
                raidArrivalMode = Rand.Bool ? PawnsArrivalModeDefOf.EdgeWalkIn : PawnsArrivalModeDefOf.EdgeDrop,
                forced = true
            };
            OAFrame_MiscUtility.TryFireIncidentNow(IncidentDefOf.RaidEnemy, parms, force: true);
        }

        if (extaRaidRemaining > 0 && (ticksToNextExtaRaid -= delta) <= 0)
        {
            extaRaidRemaining--;
            ticksToNextExtaRaid = 32500;
            IncidentParms parms = new()
            {
                target = parent.MapHeld,
                points = StorytellerUtility.DefaultThreatPointsNow(parent.MapHeld) * raidMulti,
                faction = Find.FactionManager.RandomEnemyFaction(),
                raidArrivalMode = Rand.Bool ? PawnsArrivalModeDefOf.EdgeWalkIn : PawnsArrivalModeDefOf.EdgeDrop,
                forced = true
            };
            OAFrame_MiscUtility.TryFireIncidentNow(IncidentDefOf.RaidEnemy, parms, force: true);

            if (extaRaidRemaining == 0)
            {
                Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_FinalExtaRaid".Translate()));
            }
        }
    }

    private void GravityAdjustmentFloatMenu(int adjuestType)
    {
        List<FloatMenuOption> list = [];
        List<Pawn> allPawnsSpawned = parent.Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < allPawnsSpawned.Count; i++)
        {
            Pawn pawn = allPawnsSpawned[i];
            list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
            {
                MakeGravityAdjustmentJob(pawn, adjuestType);
            }, pawn, Color.white));
        }

        if (!list.Any())
        {
            list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
        }
        Find.WindowStack.Add(new FloatMenu(list));
    }

    private void JobFloatMenu(JobDef jobDef)
    {
        List<FloatMenuOption> list = [];
        List<Pawn> allPawnsSpawned = parent.Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < allPawnsSpawned.Count; i++)
        {
            Pawn pawn = allPawnsSpawned[i];
            list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
            {
                MakeJob(pawn, jobDef);

            }, pawn, Color.white));
        }

        if (!list.Any())
        {
            list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
        }
        Find.WindowStack.Add(new FloatMenu(list));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MakeGravityAdjustmentJob(Pawn pawn, int adjuestType)
    {
        gravityAdjuestType = adjuestType;
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Props.gravityAdjustmentJob, parent), JobTag.Misc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MakeJob(Pawn pawn, JobDef jobDef)
    {
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), JobTag.Misc);
    }

    private void IncreaseGravity()
    {
        if (mainGravIndex == 5)
        {
            Messages.Message("OARK_ScienceShip_GravityMaxLevel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        if (mainGravIndex == subGravIndex)
        {
            mainGravIndex = Math.Min(mainGravIndex + 1, 5);
            subGravIndex = mainGravIndex;
        }
        else
        {
            mainGravIndex = Math.Min(mainGravIndex + 1, 5);
        }
        Messages.Message("OARK_ScienceShip_GravityLevelIncreased".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
        RecheckEnvironmentAffect();
    }

    private void DecreaseGravity()
    {
        if (mainGravIndex == 1)
        {
            Messages.Message("OARK_ScienceShip_GravityMinLevel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        if (mainGravIndex == subGravIndex)
        {
            mainGravIndex = Math.Max(mainGravIndex - 1, 1);
            subGravIndex = mainGravIndex;
        }
        else
        {
            mainGravIndex = Math.Max(mainGravIndex - 1, 1);
        }
        Messages.Message("OARK_ScienceShip_GravityLevelDecreased".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
        RecheckEnvironmentAffect();
    }

    private void ReassignmentGravity()
    {
        if (mainGravIndex == subGravIndex)
        {
            if (mainGravIndex == 5)
            {
                subGravIndex--;
            }
            else if (mainGravIndex == 1)
            {
                subGravIndex++;
            }
            else
            {
                subGravIndex += Rand.Bool ? 1 : -1;
            }
            Messages.Message("OARK_ScienceShip_GravityLevelDisordered".Translate(), MessageTypeDefOf.NegativeEvent, historical: false);
        }
        else
        {
            subGravIndex += mainGravIndex > subGravIndex ? 1 : -1;
            Messages.Message("OARK_ScienceShip_GravityLevelStabilized".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
        }

        RecheckEnvironmentAffect();
    }

    private void QuizResearcher()
    {
        quizResearcher = true;
        float chance = Rand.Value;
        if (chance < 0.4f)
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_QuizSuccess".Translate()));
            RemoveTrouble(ScienceShipRecord.TroubleType.InformationBase);
        }
        else if (chance < 0.9f)
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_QuizFail".Translate()));
        }
        else
        {
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_QuizEndlessWaiting".Translate()));
        }
    }

    private void RemoveTrouble(ScienceShipRecord.TroubleType trouble)
    {
        shipRecord.RemoveTrouble(trouble);
        RecheckSignalStrength();
        if (shipRecord.TroubleCount == 0)
        {
            mainGravIndex = subGravIndex = 3;
            allTroubleResolved = true;
            shipGameCondition?.End();
            shipGameCondition = null;
        }
    }

    private void FinishDataUpload()
    {
        dataUploaded = true;
        QuestUtility.SendQuestTargetSignals(MapParent.questTags, "DataUploaded", this.Named("SUBJECT"));
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShip_UploadedDestory".Translate()));
        if (shipRecord.TypeOfShip == ScienceShipRecord.ShipType.TravelRK)
        {
            IncidentParms parms = new()
            {
                target = Find.World,
                points = 5000f
            };
            OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_ScienceShip_TravelRKReward, Rand.RangeInclusive(2, 3) * 60000, parms);
        }
    }

    private void RecheckSignalStrength()
    {
        int preSignalStrength = signalStrength;
        signalStrength = 5;

        if (shipRecord.TypeOfShip != ScienceShipRecord.ShipType.HighSignal)
        {
            signalStrength -= shipRecord.TroubleCount;
            if (shipRecord.EnvironmentAffect == ScienceShipRecord.EnvirAffectType.DisorderedMagnetism)
            {
                signalStrength -= 2;
            }
            signalStrength = Mathf.Clamp(signalStrength, 1, 5);
        }

        if (signalStrength != preSignalStrength && ticksToNextRaid > 0)
        {
            if (ticksToNextRaid > raidInterval[signalStrength - 1])
            {
                ticksToNextExtaRaid = raidInterval[signalStrength - 1];
            }
            else
            {
                ticksToNextRaid += Mathf.Max(raidInterval[signalStrength - 1] - raidInterval[preSignalStrength - 1], 0);
            }
        }
    }

    private void RecheckEnvironmentAffect()
    {
        if (mainGravIndex == subGravIndex)
        {
            if (mainGravIndex == 3)
            {
                shipRecord.SetEnvironmentAffect(ScienceShipRecord.EnvirAffectType.None);
            }
            else
            {
                shipRecord.SetEnvironmentAffect(mainGravIndex > 3 ? ScienceShipRecord.EnvirAffectType.StrongWeakGravity : ScienceShipRecord.EnvirAffectType.WeakGravity);
            }
        }
        else
        {
            if (mainGravIndex == 5 || subGravIndex == 5)
            {
                shipRecord.SetEnvironmentAffect(ScienceShipRecord.EnvirAffectType.DisorderedMagnetism);
            }
            else
            {
                shipRecord.SetEnvironmentAffect(ScienceShipRecord.EnvirAffectType.DisorderedGravity);
            }
        }

        RecheckSignalStrength();
        RecheckGameCondition();
    }

    private void RecheckGameCondition()
    {
        if (shipGameCondition is null)
        {
            return;
        }

        HediffDef curAffectHediff = shipRecord.EnvironmentAffect switch
        {
            ScienceShipRecord.EnvirAffectType.WeakGravity => Props.weakGravityHediff,
            ScienceShipRecord.EnvirAffectType.StrongWeakGravity => Props.strongGravityHediff,
            ScienceShipRecord.EnvirAffectType.DisorderedGravity => Props.disorderedGravityHediff,
            ScienceShipRecord.EnvirAffectType.DisorderedMagnetism => Props.disorderedMagnetismHediff,
            _ => null,
        };

        shipGameCondition.SetCondition(shipRecord.EnvironmentAffect, curAffectHediff);
    }
}

using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompBrokenPilotConsole : CompHackable
{
    private static readonly int[] hackPointBoundary = [0, 60, 300, 540, 600];
    private static readonly int maxStageIndex = hackPointBoundary.Length - 1;

    protected override bool SendMapParentQuestTagSignal => true;
    private CrashedGravship CrashedGravhip => MapParent as CrashedGravship;
    private Quest Quest => CrashedGravhip?.AssociatedQuest;

    private bool isNormalPilot;
    private bool acceptRequest;
    private int curStage;

    public override void DoHack(float points, Pawn hackPawn)
    {
        if (!isHackable)
        {
            return;
        }

        hackPoint += points;
        if (curStage < maxStageIndex && hackPoint >= hackPointBoundary[curStage + 1])
        {
            StageChange(++curStage);
        }

        hackPoint = hackPoint > MaxHackPoint ? MaxHackPoint : hackPoint;
    }

    private void StageChange(int newStage)
    {
        switch (newStage)
        {
            case 1:
                isNormalPilot = Rand.Chance(0.25f);
                if (isNormalPilot)
                {
                    Find.LetterStack.ReceiveLetter("OARK_LetterLabel_NormalPilot".Translate(), "OARK_Letter_NormalPilot".Translate(), LetterDefOf.PositiveEvent, parent, ModUtility.OAFaction);
                }
                else
                {
                    Find.LetterStack.ReceiveLetter("OARK_LetterLabel_NotNormalPilot".Translate(), "OARK_Letter_NotNormalPilot".Translate(), LetterDefOf.ThreatSmall, parent, ModUtility.OAFaction);
                }
                return;
            case 2:
                if (!isNormalPilot)
                {
                    IncidentParms parms = new()
                    {
                        target = parent.MapHeld,
                        faction = Find.FactionManager.RandomEnemyFaction(),
                        raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                        raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
                        points = Mathf.Max(300f, StorytellerUtility.DefaultThreatPointsNow(parent.MapHeld) * 2.5f),
                    };

                    OAFrame_MiscUtility.AddNewQueuedIncident(IncidentDefOf.RaidEnemy, 2500, parms);

                    Find.LetterStack.ReceiveLetter("OARK_LetterLabel_BrokenPilotAttack".Translate(), "OARK_Letter_BrokenPilotAttack".Translate(), LetterDefOf.ThreatSmall, parent, ModUtility.OAFaction);
                }
                return;
            case 3:
                if (!isNormalPilot)
                {
                    ChoiceLetter_BrokenPilotConsole letter = (ChoiceLetter_BrokenPilotConsole)LetterMaker.MakeLetter(label: "OARK_LetterLabel_BrokenPilotRequest".Translate(),
                                                                                                                     text: "OARK_Letter_BrokenPilotRequest".Translate(),
                                                                                                                     def: OARK_LetterDefOf.OARK_BrokenPilotConsoleLetter,
                                                                                                                     lookTargets: parent,
                                                                                                                     relatedFaction: ModUtility.OAFaction,
                                                                                                                     quest: (MapParent as IQuestAssociate)?.AssociatedQuest);
                    letter.brokenPilot = parent;
                    Find.LetterStack.ReceiveLetter(letter);
                }
                return;
            case 4:
                FinishHack(null);
                return;
            default:
                if (!isHackForceEnded)
                {
                    ForceEndHack();
                }
                return;
        }
    }

    public void AcceptOberoniaAureaRequest()
    {
        QuestUtility.SendQuestTargetSignals(MapParentQuestTag, "AcceptOberoniaAureaRequest", parent.Named("SUBJECT"));
        acceptRequest = true;
        if (Rand.Chance(0.4f))
        {
            int delayTicks = (int)(Rand.Range(8f, 12f) * 60000);
            IncidentParms parms = new()
            {
                target = Find.World
            };
            OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_CrashedGravship_FollowUp, delayTicks, parms);
        }

        if (!isHackForceEnded)
        {
            ForceEndHack();
        }
    }

    public override void FinishHack(Pawn hackPawn)
    {
        base.FinishHack(hackPawn);

        ResearchManager researchManager = Find.ResearchManager;
        List<ResearchProjectDef> targetProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => !p.IsFinished && !p.IsHidden).InRandomOrder().Take(2).ToList();
        ResearchProjectDef curProject = researchManager.GetProject();
        if (curProject is not null)
        {
            if (targetProjects.Count == 2)
            {
                targetProjects[1] = curProject;
            }
            else
            {
                targetProjects.Add(curProject);
            }
        }

        StringBuilder sb;
        if (targetProjects.Count == 0)
        {
            sb = new("OARK_Letter_BrokenPilotHacked_NoProjectProgress".Translate());
        }
        else
        {
            sb = new("OARK_Letter_BrokenPilotHacked".Translate());
            sb.AppendLine();
            sb.AppendInNewLine("OAFrame_ResearchProgressAdded".Translate(2000));
            foreach (ResearchProjectDef project in targetProjects)
            {
                researchManager.AddProgress(project, 2000f);
                sb.AppendInNewLine(project.LabelCap);
            }
        }

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_BrokenPilotHacked".Translate(),
                                       text: sb.ToString(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: parent,
                                       relatedFaction: ModUtility.OAFaction,
                                       quest: Quest);

    }

    protected override StringBuilder CompInspectStringExtraBuilder()
    {
        StringBuilder sb = base.CompInspectStringExtraBuilder();
        sb.AppendInNewLine($"OARK_PilotConsole_CurHackStage".Translate(curStage, maxStageIndex));
        if (acceptRequest)
        {
            sb.AppendInNewLine($"OARK_PilotConsole_AcceptRequest".Translate());
        }
        return sb;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref isNormalPilot, "isNormalPilot", defaultValue: false);
        Scribe_Values.Look(ref acceptRequest, "acceptRequest", defaultValue: false);
        Scribe_Values.Look(ref curStage, "curStage", defaultValue: 0);
    }

}

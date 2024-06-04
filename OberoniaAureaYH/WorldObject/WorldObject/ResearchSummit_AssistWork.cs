﻿using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;
public class ResearchSummit_AssistWork : WorldObject_InteractiveBase
{
    public FixedCaravan_ResearchSummitAssistWork assistWorkCaravan;
    public int lastWorkTick = -1;
    public bool workStart;

    protected int RemainingCoolingTick
    {
        get
        {
            if (lastWorkTick < 0)
            {
                return -1;
            }
            return lastWorkTick + 60000 - Find.TickManager.TicksGame;
        }
    }
    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        foreach (Gizmo gizmo in base.GetCaravanGizmos(caravan))
        {
            yield return gizmo;
        }
        Command_Action command_startWork = new()
        {
            defaultLabel = "OA_StartRSAssistWork".Translate(),
            defaultDesc = "OA_StartRSAssistWorkDesc".Translate(),

            action = delegate
            {
                StartWork(caravan);
            }
        };
        int remainingCoolingTick = RemainingCoolingTick;
        if (remainingCoolingTick > 0)
        {
            command_startWork.Disable("WaitTime".Translate(remainingCoolingTick.ToStringTicksToPeriod()));
        }
        yield return command_startWork;
    }
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        StartWork(caravan);
    }
    public void StartWork(Caravan caravan)
    {
        FixedCaravan_ResearchSummitAssistWork fixedCaravan = (FixedCaravan_ResearchSummitAssistWork)FixedCaravan.CreateFixedCaravan(caravan, OA_WorldObjectDefOf.OA_FixedCaravan_ResearchSummitAssistWork, FixedCaravan_ResearchSummitAssistWork.CheckInterval);
        fixedCaravan.assistWork = this;
        Find.WorldObjects.Add(fixedCaravan);
        Find.WorldSelector.Select(fixedCaravan);
        assistWorkCaravan = fixedCaravan;
        workStart = true;
    }
    public void EndWork()
    {
        assistWorkCaravan = null;
        lastWorkTick = Find.TickManager.TicksGame;
        workStart = false;
    }

    public override void PostRemove()
    {
        assistWorkCaravan?.FinishedWork();
        base.PostRemove();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref assistWorkCaravan, "assistWorkCaravan");
        Scribe_Values.Look(ref lastWorkTick, "lastWorkTick", -1);
        Scribe_Values.Look(ref workStart, "workStart", defaultValue: false);
    }
}

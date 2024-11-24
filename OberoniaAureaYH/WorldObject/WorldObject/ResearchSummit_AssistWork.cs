using OberoniaAurea_Frame;
using RimWorld;
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
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        StartWork(caravan);
    }
    private FloatMenuAcceptanceReport CanVisit()
    {
        if (!Spawned)
        {
            return false;
        }
        int remainingCoolingTick = RemainingCoolingTick;
        if (remainingCoolingTick > 0)
        {
            return FloatMenuAcceptanceReport.WithFailReason("WaitTime".Translate(remainingCoolingTick.ToStringTicksToPeriod()));
        }
        return true;
    }

    public override IEnumerable<FloatMenuOption> GetSpecificFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this, CanVisit()))
        {
            yield return floatMenuOption;
        }
    }
    public void StartWork(Caravan caravan)
    {
        if (!OAFrame_CaravanUtility.IsExactTypeCaravan(caravan))
        {
            return;
        }
        FixedCaravan_ResearchSummitAssistWork fixedCaravan = (FixedCaravan_ResearchSummitAssistWork)OAFrame_FixedCaravanUtility.CreateFixedCaravan(caravan, OA_WorldObjectDefOf.OA_FixedCaravan_ResearchSummitAssistWork, FixedCaravan_ResearchSummitAssistWork.CheckInterval);
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

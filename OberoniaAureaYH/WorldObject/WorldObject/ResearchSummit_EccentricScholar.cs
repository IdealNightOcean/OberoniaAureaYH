﻿using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

public class ResearchSummit_EccentricScholar : WorldObject_InteractiveBase
{
    protected bool workStart;
    protected int OrganizeTicks = 15000;
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        if (!workStart)
        {
            DiaNode startNode = OAFrame_DiaUtility.ConfirmDiaNode
            (
                "OA_ResearchSummit_EccentricScholarText".Translate(),
                "OA_ResearchSummit_EccentricScholarConfirm".Translate(OrganizeTicks.ToStringTicksToPeriod(shortForm: true)),
                delegate { StartOrganize(caravan); },
                "OA_ResearchSummit_EccentricScholarIngore".Translate(),
                Destroy
            );
            Dialog_NodeTree nodeTree = new(startNode);
            Find.WindowStack.Add(nodeTree);
        }
        else
        {
            Messages.Message("OA_ResearchSummit_EccentricScholarWorking".Translate(), MessageTypeDefOf.RejectInput, historical: false);
        }
    }

    protected void StartOrganize(Caravan caravan)
    {
        if (!FixedCaravanUtility.IsExactTypeCaravan(caravan))
        {
            return;
        }
        FixedCaravan_ResearchSummitEccentricScholar fixedCaravan = (FixedCaravan_ResearchSummitEccentricScholar)FixedCaravanUtility.CreateFixedCaravan(caravan, OA_WorldObjectDefOf.OA_FixedCaravan_ResearchSummitEccentricScholar, OrganizeTicks);
        fixedCaravan.associateScholar = this;
        Find.WorldObjects.Add(fixedCaravan);
        Find.WorldSelector.Select(fixedCaravan);
        workStart = true;
    }

    public void EndOrganize()
    {
        Destroy();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref workStart, "workStart", defaultValue: false);
    }
}

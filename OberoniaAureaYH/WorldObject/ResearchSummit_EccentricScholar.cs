using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ResearchSummit_EccentricScholar : WorldObject_InteractWithFixedCaravanBase
{
    private static readonly IntRange ComponentCountRange = new(4, 10);
    private static readonly IntRange SteelCountRange = new(20, 50);

    public override int TicksNeeded => 15000;

    public override bool StartWork(Caravan caravan)
    {
        Dialog_NodeTreeWithFactionInfo nodeTree = OAFrame_DiaUtility.ConfirmDiaNodeTreeWithFactionInfo(
            text: "OA_ResearchSummit_EccentricScholarText".Translate(),
            faction: Faction,
            acceptText: "OA_ResearchSummit_EccentricScholarConfirm".Translate(TicksNeeded.ToStringTicksToPeriod(shortForm: true)),
            acceptAction: delegate { base.StartWork(caravan); },
            rejectText: "OA_ResearchSummit_EccentricScholarIngore".Translate(),
            rejectAction: Destroy);

        Find.WindowStack.Add(nodeTree);
        return true;
    }

    protected override void FinishWork()
    {
        int componentCount = ComponentCountRange.RandomInRange;
        int steelCount = SteelCountRange.RandomInRange;
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.ComponentIndustrial, componentCount);
        things.AddRange(OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Steel, steelCount));
        OAFrame_FixedCaravanUtility.GiveThings(associatedFixedCaravan, things);

        foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, 1000f, direct: true);
        }

        string ideoPawnsPlural = Faction.OfPlayer.ideos?.PrimaryIdeo?.MemberNamePlural;
        string pawnsPlural = ideoPawnsPlural.NullOrEmpty() ? Faction.OfPlayer.def.pawnsPlural : ideoPawnsPlural;
        Dialog_NodeTreeWithFactionInfo nodeTree = OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo(
            text: "OA_ResearchSummit_EccentricScholarFinish".Translate(pawnsPlural, componentCount, steelCount, 1000),
            faction: Faction);
        Find.WindowStack.Add(nodeTree);
        Destroy();
    }

    protected override void InterruptWork()
    {
        string ideoPawnsPlural = Faction.OfPlayer.ideos?.PrimaryIdeo?.MemberNamePlural;
        string pawnsPlural = ideoPawnsPlural.NullOrEmpty() ? Faction.OfPlayer.def.pawnsPlural : ideoPawnsPlural;
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OA_ResearchSummit_EccentricScholarLeaveHalfway".Translate(pawnsPlural), delegate { }));
        Destroy();
    }

    public override string FixedCaravanWorkDesc() => "OA_FixedCaravanRSEccentricScholar_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod());

}

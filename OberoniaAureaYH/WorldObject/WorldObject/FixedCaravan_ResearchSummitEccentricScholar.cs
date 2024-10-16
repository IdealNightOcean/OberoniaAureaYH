using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;
namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class FixedCaravan_ResearchSummitEccentricScholar : FixedCaravan
{
    private static readonly IntRange ComponentCountRange = new(4, 10);
    private static readonly IntRange SteelCountRange = new(20, 50);

    public ResearchSummit_EccentricScholar associateScholar;
    public override void Tick()
    {
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            FinishedWork();
        }
    }
    private void FinishedWork()
    {
        int componentCount = ComponentCountRange.RandomInRange;
        int steelCount = SteelCountRange.RandomInRange;
        List<Thing> things = OberoniaAureaFrameUtility.TryGenerateThing(ThingDefOf.ComponentIndustrial, componentCount);
        things.AddRange(OberoniaAureaFrameUtility.TryGenerateThing(ThingDefOf.Steel, steelCount));
        foreach (Thing t in things)
        {
            FixedCaravanUtility.GiveThing(this, t);
        }
        foreach (Pawn pawn in PawnsListForReading)
        {
            pawn.skills?.Learn(SkillDefOf.Intellectual, 1000f, direct: true);
        }
        FixedCaravanUtility.ConvertToCaravan(this);

        string ideoPawnsPlural = Faction.OfPlayer.ideos?.PrimaryIdeo?.MemberNamePlural;
        string pawnsPlural = ideoPawnsPlural.NullOrEmpty() ? Faction.OfPlayer.def.pawnsPlural : ideoPawnsPlural;
        DiaNode startNode = OAFrame_DiaUtility.DefaultConfirmDiaNode("OA_ResearchSummit_EccentricScholarFinish".Translate(pawnsPlural, componentCount, steelCount, 1000));
        Dialog_NodeTree nodeTree = new(startNode);
        Find.WindowStack.Add(nodeTree);
    }
    protected override void PreConvertToCaravanByPlayer()
    {
        string ideoPawnsPlural = Faction.OfPlayer.ideos?.PrimaryIdeo?.MemberNamePlural;
        string pawnsPlural = ideoPawnsPlural.NullOrEmpty() ? Faction.OfPlayer.def.pawnsPlural : ideoPawnsPlural;
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OA_ResearchSummit_EccentricScholarLeaveHalfway".Translate(pawnsPlural), delegate { }));
    }
    public override void Notify_ConvertToCaravan()
    {
        associateScholar?.EndOrganize();
    }
    public override string GetInspectString()
    {
        StringBuilder stringBuilder = new(base.GetInspectString());
        stringBuilder.AppendInNewLine("OA_FixedCaravanRSEccentricScholar_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod()));
        return stringBuilder.ToString();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateScholar, "associateScholar");
    }
}
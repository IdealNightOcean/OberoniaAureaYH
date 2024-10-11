using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//大地图事件点：学术约架
public class ResearchSummit_AcademicDispute : WorldObject_InteractiveBase
{
    private static readonly IntRange SilverCounter = new(100, 800);

    public override void Notify_CaravanArrived(Caravan caravan)
    {
        foreach (Pawn pawn in caravan.PawnsListForReading)
        {
            pawn.skills?.Learn(SkillDefOf.Melee, 1000f, direct: true);
        }
        int silverNum = SilverCounter.RandomInRange;
        List<Thing> things = OberoniaAureaFrameUtility.TryGenerateThing(RimWorld.ThingDefOf.Silver, silverNum);
        foreach (Thing thing in things)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelVisit_RSAcademicDispute".Translate(), "OA_LetterVisit_RSAcademicDispute".Translate(1000f, silverNum), LetterDefOf.PositiveEvent, caravan);
        Destroy();
    }

    public override IEnumerable<FloatMenuOption> GetSpecificFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this, "OA_Visit_RSAcademicDispute".Translate(this.Label)))
        {
            yield return floatMenuOption;
        }
    }
}
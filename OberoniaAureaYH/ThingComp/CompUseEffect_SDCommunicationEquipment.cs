using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompUseEffect_SDCommunicationEquipment : CompUseEffect_ResearchAnalyzer
{
    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        usedBy.skills?.Learn(SkillDefOf.Intellectual, 6000f, direct: true);
        Messages.Message("OARK_UsedByIntellectualXPGain".Translate(usedBy.LabelShort, 6000f.ToString("F0")), usedBy, MessageTypeDefOf.PositiveEvent);
    }
}
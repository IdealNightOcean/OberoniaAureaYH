using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_NegotiatorSkill : MultiPartyTalksTopicEventPart
{
    public SkillDef skill;
    public float xpGain;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (skill is null || eventData.Negotiator is null)
            return;

        eventData.Negotiator.skills?.Learn(skill, xpGain);
        resultBuilder.AppendLine("PeaceTalksSocialXPGain".Translate(
            eventData.Negotiator.LabelShort,
            xpGain.ToString("0.##"),
            eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
    }
}
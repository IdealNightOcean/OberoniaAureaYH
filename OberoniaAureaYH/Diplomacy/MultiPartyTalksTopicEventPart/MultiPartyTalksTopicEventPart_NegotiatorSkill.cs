using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

using static MultiPartyTalksTopicEventWorker;

public class MultiPartyTalksTopicEventPart_NegotiatorSkill : MultiPartyTalksTopicEventPart
{
    public SkillDef skill;
    public float xpGain;

    public override void ApplyEventOutcome(EventData eventData, StringBuilder resultBuilder)
    {
        if (skill is null || eventData.Negotiator is null)
            return;

        eventData.Negotiator.skills?.Learn(skill, xpGain);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_NegotiatorSkill".Translate(
            skill.Named(KeyLibrary_NamedArgument.DEF),
            xpGain.Named(KeyLibrary_NamedArgument.Value),
            eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
    }
}
using OberoniaAurea_Frame.Utility;

using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_NobleAndAttendant : MultiPartyTalksTopicEventWorker_MultiOptions
{
    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return new("OARK_TopicEvent_NobleAndAttendant_Assist".Translate())
        {
            linkLateBind = () => AssistNode(eventData),
        };
        yield return OAFrame_DiaUtility.DefaultIngnoreOption;
    }

    private static DiaNode AssistNode(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder nodeTextBuilder = new(64);
        Faction oaFaction = ModUtility.OAFaction;
        int intellectualLevel = eventData.Negotiator?.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
        if (intellectualLevel >= 10)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_NobleAndAttendant_Succeed".Translate(
                oaFaction.Named(KeyLibrary_NamedArgument.FACTION),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));

            OAInteractHandler.Instance.AdjustAssistPoints(5);
            nodeTextBuilder.AppendLine("OARK_AssistPointChange".Translate(
               oaFaction.Named(KeyLibrary_NamedArgument.FACTION),
               OAFrame_TextUtility.ColoredIntNamedArgument(5, KeyLibrary_NamedArgument.Change, includeSign: true)));

            eventData.Negotiator.skills?.Learn(SkillDefOf.Intellectual, 1000f);
            nodeTextBuilder.AppendLine("OAFrame_PawnGetSkillXP".Translate(
                SkillDefOf.Intellectual.Named(KeyLibrary_NamedArgument.SKILL),
                1000.Named(KeyLibrary_NamedArgument.Value),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }
        else
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_NobleAndAttendant_Failed".Translate(
                oaFaction.Named(KeyLibrary_NamedArgument.FACTION),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));

            eventData.Negotiator.skills?.Learn(SkillDefOf.Intellectual, -500f);
            nodeTextBuilder.AppendLine("OAFrame_PawnGetSkillXP".Translate(
                SkillDefOf.Intellectual.Named(KeyLibrary_NamedArgument.SKILL),
                (-500).Named(KeyLibrary_NamedArgument.Value),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }
}
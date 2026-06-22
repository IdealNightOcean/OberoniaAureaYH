using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_FirstAid : MultiPartyTalksTopicEventWorker_MultiOptions
{
    private const int SucceedGoodwill = 20;
    private const int FailedGoodwill = -10;

    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return new("OARK_TopicEvent_FirstAid_TryAid".Translate())
        {
            linkLateBind = () => TryAidNode(eventData),
        };
        yield return new("OARK_TopicEvent_FirstAid_TryIngnore".Translate())
        {
            resolveTree = true
        };
    }


    protected DiaNode TryAidNode(MultiPartyTalksTopicEventData eventData)
    {
        (Pawn maxPawn, int maxSkill) = OAFrame_PawnUtility.GetMaxSkillLevelPawn(MultiPartyTalksUtility.GetCaravanPawns(eventData), SkillDefOf.Medicine); ;
        StringBuilder nodeTextBuilder = new(64);
        if (maxSkill >= 12)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_FirstAid_AidSucceed".Translate(
                maxPawn.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));
            nodeTextBuilder.AppendLine();

            eventData.TalksSite.AdjustBaselineNegotiationAbility(-0.05f);
            nodeTextBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_TalksBaseline".Translate(OAFrame_TextUtility.ColoredPercentNamedArgument(-0.05f, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true)));

            Faction.OfPlayer.TryAffectGoodwillWith(other: eventData.TalksSite.Faction,
                                       goodwillChange: SucceedGoodwill,
                                       reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
            nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
                eventData.TalksSite.Faction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(SucceedGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));

            if (eventData.PrimaryFaction != eventData.TalksSite.Faction)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(other: eventData.PrimaryFaction,
                           goodwillChange: SucceedGoodwill,
                           reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
                nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(SucceedGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));
            }

        }
        else
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_FirstAid_AidFailed".Translate(
                maxPawn.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));
            nodeTextBuilder.AppendLine();

            Faction.OfPlayer.TryAffectGoodwillWith(other: eventData.PrimaryFaction,
                                                   goodwillChange: FailedGoodwill,
                                                   reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
            nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(FailedGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));

        }

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }
}

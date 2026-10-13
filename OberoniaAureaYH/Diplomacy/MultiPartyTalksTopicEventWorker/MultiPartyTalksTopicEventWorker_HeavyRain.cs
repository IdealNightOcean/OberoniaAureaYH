using OberoniaAurea_Frame;

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_HeavyRain : MultiPartyTalksTopicEventWorker_MultiOptions
{
    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        List<Faction> factions = eventData.TalksSite.ParticipantFactions.TakeRandomElements(3).ToList();
        if (factions.NullOrEmpty())
        {
            factions.AddRange(eventData.TopicFactions);
        }

        foreach (Faction f in factions)
        {
            yield return new(f.Name)
            {
                linkLateBind = () => FactionLateBind(f, eventData.Negotiator)
            };
        }
        yield return new("OARK_TopicEvent_HeavyRain_Self".Translate())
        {
            linkLateBind = () => SelfLateBind(eventData)
        };
    }

    private DiaNode FactionLateBind(Faction faction, Pawn negotiator)
    {
        string nodeText;
        if (faction == ModUtility.OAFaction)
        {
            nodeText = "OARK_TopicEvent_HeavyRain_OA";
        }
        else
        {
            nodeText = faction.PlayerRelationKind switch
            {
                FactionRelationKind.Ally => "OARK_TopicEvent_HeavyRain_Ally",
                FactionRelationKind.Hostile => "OARK_TopicEvent_HeavyRain_Hostile",
                _ => "OARK_TopicEvent_HeavyRain_Neutral"
            };
        }

        StringBuilder nodeTextBuilder = new(64);
        nodeTextBuilder.AppendLine(nodeText.Translate(
            faction.Named(KeyLibrary_NamedArgument.FACTION),
            negotiator.Named(KeyLibrary_NamedArgument.PAWN)));

        Faction.OfPlayer.TryAffectGoodwillWith(other: faction,
                                               goodwillChange: 10,
                                               reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);

        nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
            faction.Named(KeyLibrary_NamedArgument.FACTION),
            OAFrame_TextUtility.ColoredIntNamedArgument(10, KeyLibrary_NamedArgument.Change, includeSign: true)));

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }

    private DiaNode SelfLateBind(MultiPartyTalksTopicEventData eventData)
    {
        IEnumerable<Pawn> pawns = MultiPartyTalksUtility.GetCaravanPawns(eventData);
        List<Pawn> patients = [];
        foreach (Pawn p in pawns)
        {
            float immunityGainSpeed = p.GetStatValue(StatDefOf.ImmunityGainSpeed);
            if (immunityGainSpeed < 1.5f)
            {
                p.AdjustOrAddHediff(HediffDefOf.FleshmassLung);
            }
        }

        TaggedString nodeText;
        if (patients.Count > 0)
        {
            nodeText = "OARK_TopicEvent_HeavyRain_SelfWithPatients".Translate(GenLabel.ThingsLabel(patients).Named("PawnsLabel"));
        }
        else
        {
            nodeText = "OARK_TopicEvent_HeavyRain_SelfWithoutPatient".Translate();
        }

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeText);
    }
}

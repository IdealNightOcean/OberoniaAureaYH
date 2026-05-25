using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventData
{
    public WorldObject_MultiPartyTalks TalksSite { get; private set; }
    public List<Faction> TopicFactions { get; private set; }
    public Faction PrimaryFaction { get; private set; }
    public string TopicFactionsName { get; }
    public Pawn Negotiator { get; private set; }

    public MultiPartyTalksTopicEventData(WorldObject_MultiPartyTalks talksSite,
                                         IEnumerable<Faction> topicFactions,
                                         Pawn negotiator)
    {
        TalksSite = talksSite ?? throw new System.ArgumentNullException(nameof(talksSite));
        TopicFactions = [.. topicFactions];
        if (TopicFactions.Count == 0) throw new System.ArgumentException($"{nameof(topicFactions)} is null or empty.");

        Negotiator = negotiator;
        PrimaryFaction = TopicFactions[0];
        TopicFactionsName = string.Join(", ", topicFactions.Select(f => f.NameColored));
    }

    public string Formatted(string oriString)
    {
        return oriString.Formatted(TalksSite.Faction.Named("ORGANIZERFACTION"),
                                   Negotiator.Named("NEGOTIATOR"),
                                   PrimaryFaction.Named("PRIMARYFACTION"),
                                   TopicFactionsName.Named(KeyLibrary_NamedArgument.TopicFactionsName));
    }
}

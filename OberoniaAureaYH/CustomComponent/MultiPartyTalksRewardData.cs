using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksRewardData
{
    public int AssistPoints { get; set; }
    public int RoyalFavor { get; set; }
    public int OrganizerFactionGoodwill { get; set; }
    public int FactionGoodwill { get; set; }
    public LetterDef LetterDef { get; set; }
    public HistoryEventDef HistoryEvent { get; set; }
    public List<Thing> Gifts { get; } = [];
}

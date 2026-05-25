using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class DiplomacyRewardData
{
    public int AssistPoints { get; set; }
    public int RoyalFavor { get; set; }
    public int OrganizerFactionGoodwill { get; set; }
    public int OtherFactionGoodwill { get; set; }
    public LetterDef LetterDef { get; set; }
    public HistoryEventDef HistoryEvent { get; set; }
    public int NegotiatorXpGain { get; set; }
    public List<Thing> Gifts { get; } = [];

    public DiplomacyRewardData()
    {
        HistoryEvent = OARK_HistoryEventDefOf.OARK_DiplomacyInteraction;
        LetterDef = LetterDefOf.NeutralEvent;
    }
}

using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class QuestNode_MultiPartyTalksDescMaker : QuestNode
{
    public SlateRef<IEnumerable<Faction>> participantFactions;
    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        HashSet<Faction> factions = participantFactions.GetValue(slate)?.ToHashSet();
        if (factions.NullOrEmpty())
            return;

        factions.Add(ModUtility.OAFaction);

        slate.Set("totalTopics", factions.Count - 1);
        MultiPartyTalksUtility.CalculateBaselineNegotiationAbility(factions, resultOnly: false, out string explanation);
        slate.Set("baselineExplanation", explanation);
    }
}
using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_FieldTripForage : MultiPartyTalksTopicEventPart
{
    public int foodCountPerPawn = 100;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (eventData.TalksSite is null)
            return;

        int totalCount = foodCountPerPawn * eventData.TalksSite.AssociatedFixedCaravan.PawnsCount;

        BiomeDef biome = Find.WorldGrid[eventData.TalksSite.Tile].PrimaryBiome;
        ThingDef foragedThingDef = biome?.foragedFood ?? OARK_RimWorldDefOf.RawBerries;

        List<Thing> foragedThings = OAFrame_ThingUtility.GenerateThingListSplitByStack(foragedThingDef, totalCount);

        MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, foragedThings);
        resultBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(foragedThings).Named(KeyLibrary_NamedArgument.ThingsLabel)));
    }
}
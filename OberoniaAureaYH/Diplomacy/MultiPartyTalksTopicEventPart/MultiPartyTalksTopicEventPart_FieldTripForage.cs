using RimWorld;
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

        Thing foragedThing = ThingMaker.MakeThing(foragedThingDef);
        foragedThing.stackCount = totalCount;

        MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, [foragedThing]);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_FieldTripForage".Translate(
            foragedThingDef.Named(KeyLibrary_NamedArgument.DEF),
            totalCount.Named(KeyLibrary_NamedArgument.Value)));
    }
}
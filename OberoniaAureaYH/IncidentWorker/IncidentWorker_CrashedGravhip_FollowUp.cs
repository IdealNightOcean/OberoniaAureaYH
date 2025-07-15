using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

internal class IncidentWorker_CrashedGravship_FollowUp : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return Find.AnyPlayerHomeMap is not null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = Find.AnyPlayerHomeMap;
        if (map is null)
        {
            return false;
        }

        if (CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, ThingDefOf.ShipChunk.terrainAffordanceNeeded, out IntVec3 pos, minDistToEdge: 10, nearLoc: map.Center, nearLocMaxDist: 999999))
        {
            SpawnShipChunks(pos, map, Rand.RangeInclusive(8, 12));

            Faction faction = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);
            if (faction is not null)
            {
                int pawnCount = Rand.RangeInclusive(2, 4);

                Skyfaller crashShip = SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShuttleCrashing);
                for (int i = 0; i < pawnCount; i++)
                {
                    PawnGenerationRequest generationRequest = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(PawnKindDefOf.Pirate, faction);
                    Pawn pawn = PawnGenerator.GeneratePawn(generationRequest);
                    HealthUtility.DamageUntilDowned(pawn);
                    crashShip.innerContainer.TryAddOrTransfer(pawn);
                }
                CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, ThingDefOf.ShipChunk.terrainAffordanceNeeded, out IntVec3 pawnPos, minDistToEdge: 10, nearLoc: pos, nearLocMaxDist: 8);
                GenSpawn.Spawn(crashShip, pawnPos, map);
            }
        }

        OARK_DropPodUtility.DefaultDropThingOfDef(ThingDefOf.Silver, 2000, map);

        Find.FactionManager.OfTradersGuild?.TryAffectGoodwillWith(Faction.OfPlayer, 20, reason: OARK_HistoryEventDefOf.OARK_CrashedGravshipHelp);
        ModUtility.OAFaction?.TryAffectGoodwillWith(Faction.OfPlayer, 10, reason: OARK_HistoryEventDefOf.OARK_CrashedGravshipHelp);


        foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
        {
            pawn.needs.mood?.thoughts.memories.TryGainMemory(OARK_ThoughtDefOf.OARK_Thought_PirateShipExploded);
        }

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_CrashedGravshipFollowUp".Translate(),
                                       text: "OARK_Letter_CrashedGravshipFollowUp".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: null,
                                       relatedFaction: ModUtility.OAFaction);

        return true;
    }

    private static void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
    {
        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, firstChunkPos, map);
        for (int i = 0; i < count - 1; i++)
        {
            if (CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, ThingDefOf.ShipChunk.terrainAffordanceNeeded, out IntVec3 chunkPos, minDistToEdge: 10, nearLoc: firstChunkPos, nearLocMaxDist: 8))
            {
                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, chunkPos, map);
            }
        }
    }
}

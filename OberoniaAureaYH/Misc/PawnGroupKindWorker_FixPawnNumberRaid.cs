using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class PawnGroupKindWorker_FixPawnNumberRaid : PawnGroupKindWorker_Normal
{
    protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
    {
        if (!CanGenerateFrom(parms, groupMaker))
        {
            if (errorOnZeroResults)
            {
                Log.Error(string.Concat("Cannot generate pawns for ", parms.faction, " with ", parms.points, ". Defaulting to a single random cheap group."));
            }
            return;
        }
        bool allowFood = parms.raidStrategy is null || parms.raidStrategy.pawnsCanBringFood || (parms.faction is not null && !parms.faction.HostileTo(Faction.OfPlayer));
        Predicate<Pawn> validatorPostGear = ((parms.raidStrategy is not null) ? ((Pawn p) => parms.raidStrategy.Worker.CanUsePawn(parms.points, p, outPawns)) : null);
        bool flag = false;

        foreach (PawnGenOption pgo in groupMaker.options)
        {
            PawnGenerationRequest request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(pgo.kind, parms.faction);
            request.Tile = parms.tile;
            request.FixedIdeo = parms.ideo;
            request.MustBeCapableOfViolence = true;
            request.Inhabitant = parms.inhabitants;

            if (parms.raidAgeRestriction is not null && parms.raidAgeRestriction.Worker.ShouldApplyToKind(pgo.kind))
            {
                request.BiologicalAgeRange = parms.raidAgeRestriction.ageRange;
                request.AllowedDevelopmentalStages = parms.raidAgeRestriction.developmentStage;
            }
            if (pgo.kind.pawnGroupDevelopmentStage.HasValue)
            {
                request.AllowedDevelopmentalStages = pgo.kind.pawnGroupDevelopmentStage.Value;
            }
            if (!Find.Storyteller.difficulty.ChildRaidersAllowed && parms.faction is not null && parms.faction.HostileTo(Faction.OfPlayer))
            {
                request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
            }
            int num = (int)pgo.selectionWeight;
            for (int i = 0; i < num; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (parms.forceOneDowned && !flag)
                {
                    pawn.health.forceDowned = true;
                    if (pawn.guest is not null)
                    {
                        pawn.guest.Recruitable = true;
                    }
                    pawn.mindState.canFleeIndividual = false;
                    flag = true;
                }
                outPawns.Add(pawn);
            }
        }
    }

}

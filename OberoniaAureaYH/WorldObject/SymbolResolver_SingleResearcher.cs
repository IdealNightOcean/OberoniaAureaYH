﻿using OberoniaAurea_Frame.Utility;
using RimWorld;
using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;
public class SymbolResolver_SingleResearcher : SymbolResolver
{
    private static readonly IntRange IntellectualSkill = new(8, 18);
    public override bool CanResolve(ResolveParams rp)
    {
        if (!base.CanResolve(rp))
        {
            return false;
        }
        if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
        {
            return true;
        }
        if (!TryFindSpawnCell(rp, out var _))
        {
            return false;
        }
        return true;
    }

    public override void Resolve(ResolveParams rp)
    {
        if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
        {
            return;
        }
        Map map = BaseGen.globalSettings.map;
        if (!TryFindSpawnCell(rp, out var cell))
        {
            if (rp.singlePawnToSpawn != null)
            {
                Find.WorldPawns.PassToWorld(rp.singlePawnToSpawn);
            }
            return;
        }
        Pawn pawn;
        if (rp.singlePawnToSpawn == null)
        {
            PawnGenerationRequest request;
            if (rp.singlePawnGenerationRequest.HasValue)
            {
                request = rp.singlePawnGenerationRequest.Value;
            }
            else
            {
                PawnKindDef pawnKindDef = rp.singlePawnKindDef ?? DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef x) => x.defaultFactionType == null || !x.defaultFactionType.isPlayer).RandomElement();
                Faction result = rp.faction;
                if (result == null && pawnKindDef.RaceProps.Humanlike)
                {
                    if (pawnKindDef.defaultFactionType != null)
                    {
                        result = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
                        if (result == null)
                        {
                            return;
                        }
                    }
                    else if (!Find.FactionManager.AllFactions.Where((Faction x) => !x.IsPlayer).TryRandomElement(out result))
                    {
                        return;
                    }
                }
                request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(pawnKindDef, result);
                request.Tile = map.Tile;
            }
            pawn = PawnGenerator.GeneratePawn(request);
            rp.postThingGenerate?.Invoke(pawn);
        }
        else
        {
            pawn = rp.singlePawnToSpawn;
        }
        if (!pawn.Dead && rp.disableSinglePawn.HasValue && rp.disableSinglePawn.Value)
        {
            pawn.mindState.Active = false;
        }
        AdjustPawnSkill(pawn);
        GenSpawn.Spawn(pawn, cell, map);
        rp.singlePawnLord?.AddPawn(pawn);
        rp.postThingSpawn?.Invoke(pawn);
    }

    private static void AdjustPawnSkill(Pawn pawn)
    {
        SkillRecord intellectual = pawn.skills?.GetSkill(SkillDefOf.Intellectual);
        if (intellectual != null && intellectual.Level < 8)
        {
            intellectual.Level = IntellectualSkill.RandomInRange;
        }
    }

    public static bool TryFindSpawnCell(ResolveParams rp, out IntVec3 cell)
    {
        Map map = BaseGen.globalSettings.map;
        return CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
    }
}

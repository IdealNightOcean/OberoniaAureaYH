﻿using RimWorld;
using RimWorld.BaseGen;
using System;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

public class SymbolResolver_ResearcherCamp : SymbolResolver_WorkSite
{

    public static readonly FloatRange DefaultPawnsPoints = new(300f, 400f);
    public static readonly IntRange DefaultBedCount = new(3, 5);

    public override void Resolve(ResolveParams rp)
    {
        rp.bedCount = DefaultBedCount.RandomInRange;

        Map map = BaseGen.globalSettings.map;
        Faction faction = ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : ResearcherCampComp.GenerateTempCampFaction());
        Lord singlePawnLord = (rp.settlementLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell, rp.attackWhenPlayerBecameEnemy ?? false), map));
        TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
        ResolveParams resolveParams1 = rp;
        PawnGenerationRequest pawnRequest = new()
        {
            Faction = faction,
            KindDef = RimWorld.PawnKindDefOf.Villager,
            Context = PawnGenerationContext.NonPlayer,
            Tile = map.Tile,
            AllowDead = false,
            AllowDowned = false,
        };
        resolveParams1.rect = rp.rect;
        resolveParams1.faction = faction;
        resolveParams1.singlePawnGenerationRequest = pawnRequest;
        resolveParams1.singlePawnSpawnCellExtraPredicate = rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));

        ResolveParams resolveParams2 = new()
        {
            rect = rp.rect,
            singleThingDef = RimWorld.ThingDefOf.Bedroll
        };
        for (int i = 0; i < rp.bedCount; i++)
        {
            BaseGen.symbolStack.Push("oa_SingleResearcher", resolveParams1);
            BaseGen.symbolStack.Push("thing", resolveParams2);
        }
    }
}

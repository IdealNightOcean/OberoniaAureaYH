using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//重型商队成员生成
public class PawnGroupKindWorker_OALargeScaleTrader : PawnGroupKindWorker_Trader
{
    protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
    {
        if (!CanGenerateFrom(parms, groupMaker))
        {
            if (errorOnZeroResults)
            {
                Log.Error(string.Concat("Cannot generate trader caravan for ", parms.faction, "."));
            }
            return;
        }
        if (!parms.faction.def.caravanTraderKinds.Any())
        {
            Log.Error(string.Concat("Cannot generate trader caravan for ", parms.faction, " because it has no trader kinds."));
            return;
        }
        PawnGenOption pawnGenOption = groupMaker.traders.FirstOrDefault(p => !p.kind.trader);
        if (pawnGenOption is not null)
        {
            Log.Error(string.Concat("Cannot generate arriving trader caravan for ", parms.faction, " because there is a pawn kind (") + pawnGenOption.kind.LabelCap + ") who is not a trader but is in a traders list.");
            return;
        }
        PawnGenOption pawnGenOption2 = groupMaker.carriers.FirstOrDefault(p => !p.kind.RaceProps.packAnimal);
        if (pawnGenOption2 is not null)
        {
            Log.Error(string.Concat("Cannot generate arriving trader caravan for ", parms.faction, " because there is a pawn kind (") + pawnGenOption2.kind.LabelCap + ") who is not a carrier but is in a carriers list.");
            return;
        }
        if (parms.seed.HasValue)
        {
            Log.Warning("Deterministic seed not implemented for this pawn group kind worker. The result will be random anyway.");
        }
        TraderKindDef traderKindDef = parms.traderKind ?? parms.faction.def.caravanTraderKinds.RandomElementByWeight(traderDef => traderDef.CalculatedCommonality);
        Pawn pawn = GenerateTrader(parms, groupMaker, traderKindDef);
        outPawns.Add(pawn);
        ThingSetMakerParams parms2 = default;
        parms2.traderDef = traderKindDef;
        parms2.tile = parms.tile;
        parms2.makingFaction = parms.faction;
        List<Thing> wares = ThingSetMakerDefOf.TraderStock.root.Generate(parms2).InRandomOrder().ToList();
        foreach (Pawn slavesAndAnimalsFromWare in GetSlavesAndAnimalsFromWares(parms, pawn, wares))
        {
            outPawns.Add(slavesAndAnimalsFromWare);
        }
        GenerateCarriers(parms, groupMaker, pawn, wares, outPawns);
        GenerateGuards(parms, groupMaker, outPawns);
    }

    protected void GenerateGuards(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns)
    {
        if (!groupMaker.guards.Any())
        {
            return;
        }
        foreach (PawnGenOption pgo in groupMaker.guards)
        {
            int num = (int)pgo.selectionWeight;
            for (int i = 0; i < num; i++)
            {
                PawnGenerationRequest request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(pgo.kind, parms.faction);
                request.Tile = parms.tile;
                request.FixedIdeo = parms.ideo;
                request.Inhabitant = parms.inhabitants;
                Pawn item = PawnGenerator.GeneratePawn(request);
                outPawns.Add(item);
            }
        }
    }

    //下面三个不重要，是因为泰南用了private才复制过来的
    protected Pawn GenerateTrader(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
    {
        PawnKindDef traderPawnKind = groupMaker.traders.RandomElementByWeight(p => p.selectionWeight).kind;
        PawnGenerationRequest request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(traderPawnKind, parms.faction);
        request.Tile = parms.tile;
        request.FixedIdeo = parms.ideo;
        request.Inhabitant = parms.inhabitants;
        Pawn pawn = PawnGenerator.GeneratePawn(request);
        pawn.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
        pawn.trader.traderKind = traderKind;
        parms.points -= pawn.kindDef.combatPower;
        return pawn;
    }
    protected void GenerateCarriers(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, List<Pawn> outPawns)
    {
        List<Thing> list = wares.Where(t => t is not Pawn).ToList();
        int i = 0;
        int num = Mathf.CeilToInt(list.Count / 8f);
        PawnKindDef kind = groupMaker.carriers.Where(p => parms.tile == -1 || Find.WorldGrid[parms.tile].biome.IsPackAnimalAllowed(p.kind.race)).RandomElementByWeight(p => p.selectionWeight).kind;
        List<Pawn> list2 = [];
        for (int j = 0; j < num; j++)
        {
            PawnGenerationRequest request = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(kind, parms.faction);
            request.Tile = parms.tile;
            request.FixedIdeo = parms.ideo;
            request.Inhabitant = parms.inhabitants;
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (i < list.Count)
            {
                pawn.inventory.innerContainer.TryAdd(list[i]);
                i++;
            }
            list2.Add(pawn);
            outPawns.Add(pawn);
        }
        for (; i < list.Count; i++)
        {
            list2.RandomElement().inventory.innerContainer.TryAdd(list[i]);
        }
    }
    protected IEnumerable<Pawn> GetSlavesAndAnimalsFromWares(PawnGroupMakerParms parms, Pawn trader, List<Thing> wares)
    {
        for (int i = 0; i < wares.Count; i++)
        {
            if (wares[i] is Pawn pawn)
            {
                if (pawn.Faction != parms.faction)
                {
                    pawn.SetFaction(parms.faction);
                }
                yield return pawn;
            }
        }
    }
}
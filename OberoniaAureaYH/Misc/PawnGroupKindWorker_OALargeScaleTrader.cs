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
                Log.Error($"[OARK] 无法为派系 {parms.faction} 生成贸易商队。");
            }
            return;
        }
        if (!parms.faction.def.caravanTraderKinds.Any())
        {
            Log.Error($"[OARK] 无法为派系 {parms.faction} 生成贸易商队，因为该派系没有交易类型。");
            return;
        }
        PawnGenOption pawnGenOption = groupMaker.traders.FirstOrDefault(p => !p.kind.trader);
        if (pawnGenOption is not null)
        {
            Log.Error($"[OARK] 无法为派系 {parms.faction} 生成到达的贸易商队，因为存在角色类型 ({pawnGenOption.kind.LabelCap}) 不是商人但在商人列表中。");
            return;
        }
        PawnGenOption pawnGenOption2 = groupMaker.carriers.FirstOrDefault(p => !p.kind.RaceProps.packAnimal);
        if (pawnGenOption2 is not null)
        {
            Log.Error($"[OARK] 无法为派系 {parms.faction} 生成到达的贸易商队，因为存在角色类型 ({pawnGenOption2.kind.LabelCap}) 不是驮兽但在驮兽列表中。");
            return;
        }
        if (parms.seed.HasValue)
        {
            Log.Warning("[OARK] 此角色组类型尚未实现确定性种子。结果将是随机的。");
        }
        TraderKindDef traderKindDef = (parms.traderKind ?? parms.faction.def.caravanTraderKinds.RandomElementByWeight(traderDef => traderDef.CalculatedCommonality));
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
        PawnKindDef kind = groupMaker.carriers.Where(p => !parms.tile.Valid || Find.WorldGrid[parms.tile].PrimaryBiome.IsPackAnimalAllowed(p.kind.race)).RandomElementByWeight(p => p.selectionWeight).kind;
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
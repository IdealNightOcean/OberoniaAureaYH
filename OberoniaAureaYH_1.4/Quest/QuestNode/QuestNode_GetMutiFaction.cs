using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

//QuestNode：获取指定数量多个派系
public class QuestNode_OAGetMutiFaction : QuestNode_OAGetFaction
{
    public SlateRef<IntRange> factionCount;
    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet<List<Faction>>(storeAs.GetValue(slate), out var var) && IsGoodFactions(var, slate))
        {
            return true;
        }
        if (TryFindFactions(out var, factionCount.GetValue(slate), slate))
        {
            slate.Set(storeAs.GetValue(slate), var);
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if ((!QuestGen.slate.TryGet<List<Faction>>(storeAs.GetValue(slate), out var var) || !IsGoodFactions(var, QuestGen.slate)) && TryFindFactions(out var, factionCount.GetValue(slate), QuestGen.slate))
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), var);
            foreach (Faction f in var)
            {
                if (!f.Hidden)
                {
                    QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                    questPart_InvolvedFactions.factions.Add(f);
                    QuestGen.quest.AddPart(questPart_InvolvedFactions);
                }
            }
        }
    }

    private bool TryFindFactions(out List<Faction> factions, IntRange factionCount, Slate slate)
    {
        System.Random rnd = new();
        var potentialFactions = Find.FactionManager.GetFactions(allowHidden: true).Where(f => IsGoodFaction(f, slate)).ToHashSet();
        int fNum = Mathf.Min(factionCount.RandomInRange, potentialFactions.Count);
        factions = potentialFactions.Take(fNum).ToList();
        return factionCount.min <= potentialFactions.Count;
    }

    protected bool IsGoodFactions(List<Faction> factions, Slate slate)
    {
        if (!factions.Any())
        {
            return false;
        }
        foreach (Faction f in factions)
        {
            if (!IsGoodFaction(f, slate))
            {
                return false;
            }
        }
        return true;
    }

    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (faction == OberoniaAureaYHUtility.OAFaction)
        {
            return false;
        }
        if (!base.IsGoodFaction(faction, slate))
        {
            return false;
        }
        if (WorldObjectContainFaction(faction, slate))
        {
            return false;
        }
        return true;
    }

}

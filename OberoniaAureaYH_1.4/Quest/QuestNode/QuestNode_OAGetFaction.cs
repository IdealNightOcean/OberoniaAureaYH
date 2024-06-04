using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;

//QuestNode：获取派系基础（泰南你TM搞那么多Private干啥）
public abstract class QuestNode_OAGetFaction : QuestNode_GetFaction
{
    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) && IsGoodFaction(var, slate))
        {
            return true;
        }
        if (TryFindFaction(out var, slate))
        {
            slate.Set(storeAs.GetValue(slate), var);
            return true;
        }
        return false;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if ((!QuestGen.slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) || !IsGoodFaction(var, QuestGen.slate)) && TryFindFaction(out var, QuestGen.slate))
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), var);
            if (!var.Hidden)
            {
                QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
                questPart_InvolvedFactions.factions.Add(var);
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    protected virtual bool TryFindFaction(out Faction faction, Slate slate)
    {
        return (from x in Find.FactionManager.GetFactions(allowHidden: true)
                where IsGoodFaction(x, slate)
                select x).TryRandomElement(out faction);
    }
    protected virtual bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (faction.Hidden && (allowedHiddenFactions.GetValue(slate) == null || !allowedHiddenFactions.GetValue(slate).Contains(faction)))
        {
            return false;
        }
        if (ofPawn.GetValue(slate) != null && faction != ofPawn.GetValue(slate).Faction)
        {
            return false;
        }
        if (exclude.GetValue(slate) != null && exclude.GetValue(slate).Contains(faction))
        {
            return false;
        }
        if (mustBePermanentEnemy.GetValue(slate) && !faction.def.permanentEnemy)
        {
            return false;
        }
        if (!allowEnemy.GetValue(slate) && faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        if (!allowNeutral.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Neutral)
        {
            return false;
        }
        if (!allowAlly.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Ally)
        {
            return false;
        }
        bool? value = allowPermanentEnemy.GetValue(slate);
        if (value.HasValue && !value.GetValueOrDefault() && faction.def.permanentEnemy)
        {
            return false;
        }
        if (playerCantBeAttackingCurrently.GetValue(slate) && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
        {
            return false;
        }
        if (mustHaveGoodwillRewardsEnabled.GetValue(slate) && !faction.allowGoodwillRewards)
        {
            return false;
        }
        if (leaderMustBeSafe.GetValue(slate) && (faction.leader == null || faction.leader.Spawned || faction.leader.IsPrisoner))
        {
            return false;
        }
        Thing value2 = mustBeHostileToFactionOf.GetValue(slate);
        if (value2 != null && value2.Faction != null && (value2.Faction == faction || !faction.HostileTo(value2.Faction)))
        {
            return false;
        }
        return true;
    }

    protected virtual bool WorldObjectContainFaction(Faction faction, Slate slate)
    {
        return false;
    }
}



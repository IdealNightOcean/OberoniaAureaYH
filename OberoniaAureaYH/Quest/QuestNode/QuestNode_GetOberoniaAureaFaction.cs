using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//QuestNode：获取金鼠鼠派系
public class QuestNode_GetOberoniaAureaFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> mustAlly;
    public SlateRef<bool> mustNotAlly;
    public SlateRef<bool> mustHostile;
    public SlateRef<bool> mustNotHostile = true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (OAFactionValid(slate))
        {
            slate.Set(storeAs.GetValue(slate), OberoniaAureaYHUtility.OAFaction);
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        return OAFactionValid(slate);
    }
    private bool OAFactionValid(Slate slate)
    {
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null)
        {
            return false;
        }
        FactionRelationKind playerRelationKind = OAFaction.PlayerRelationKind;
        switch (playerRelationKind)
        {
            case FactionRelationKind.Ally:
                {
                    if (mustNotAlly.GetValue(slate) || mustHostile.GetValue(slate))
                    {
                        return false;
                    }
                    return true;
                }
            case FactionRelationKind.Neutral:
                {
                    if (mustAlly.GetValue(slate) || mustHostile.GetValue(slate))
                    {
                        return false;
                    }
                    return true;
                }
            case FactionRelationKind.Hostile:
                {
                    if (mustAlly.GetValue(slate) || mustNotHostile.GetValue(slate))
                    {
                        return false;
                    }
                    return true;
                }
            default: return false;
        }
    }
}

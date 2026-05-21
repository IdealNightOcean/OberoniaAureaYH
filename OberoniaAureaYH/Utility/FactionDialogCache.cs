using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class FactionDialogCache
{
    public Pawn Negotiator { get; }
    public bool NegotiatorSocialEnabled { get; }
    public Faction Faction { get; }
    public Map Map { get; }

    public FactionRelationKind PlayerRelationKind => Faction.PlayerRelationKind;
    public float TotalAllianceDuration { get; } = -1f;

    private int? amountSendableSilver;
    public int AmountSendableSilver
    {
        get
        {
            if (!amountSendableSilver.HasValue)
            {
                amountSendableSilver = OAFrame_MapUtility.AmountSendableSilver(this.Map);
            }
            return amountSendableSilver.Value;
        }
    }

    public FactionDialogCache(Pawn negotiator, Faction faction)
    {
        Negotiator = negotiator;
        Faction = faction;
        Map = negotiator.MapHeld;

        NegotiatorSocialEnabled = !negotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
        TotalAllianceDuration = OAInteractHandler.Instance.AllianceDuration();
    }

    private void MarkAmountSendableSilverDirty()
    {
        amountSendableSilver = int.MinValue;
    }
}
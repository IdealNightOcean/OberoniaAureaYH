using OberoniaAurea_Frame;
using RimWorld.Planet;
using System.Text;
using Verse;

namespace OberoniaAurea;


[StaticConstructorOnStartup]
public class FixedCaravan_DiplomaticSummit : FixedCaravan
{
    public Settlement associateSettlement;
    [Unsaved] private SettlementDipComp associateSettlementComp;
    [Unsaved] private DiplomaticSummitHandler associateSummitHandler;

    public void SetSettlement(Settlement settlement)
    {
        associateSettlement = settlement;
        associateSettlementComp = settlement.GetComponent<SettlementDipComp>();
        associateSummitHandler = associateSettlementComp?.DiplomaticSummitHandler;
    }

    protected override void PreConvertToCaravanByPlayer()
    {
        associateSummitHandler?.Notify_FixedCaravanLeaveByPlayer(this);
        base.PreConvertToCaravanByPlayer();
    }

    public override string GetInspectString()
    {
        if (associateSummitHandler is null)
        {
            return base.GetInspectString();
        }
        else
        {
            StringBuilder stringBuilder = new(base.GetInspectString());
            stringBuilder.AppendInNewLine(associateSummitHandler.FixedCaravanWorkDesc());
            return stringBuilder.ToString();
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateSettlement, "associateSettlement");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            associateSettlementComp = associateSettlement.GetComponent<SettlementDipComp>();
            associateSummitHandler = associateSettlementComp?.DiplomaticSummitHandler;
        }
    }
}


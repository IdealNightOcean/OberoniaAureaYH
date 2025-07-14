using OberoniaAurea_Frame;

namespace OberoniaAurea;

public class FixedCaravan_DiplomaticSummit : FixedCaravan
{
    protected override void TrySetAssociatedInterface()
    {
        associatedInterface = associatedWorldObject.GetComponent<SettlementDipComp>()?.DiplomaticSummitHandler;
    }
}
using Verse;

namespace OberoniaAurea;

public abstract class JobDriver_CrashedScienceShip : JobDriver_InteractWithThing
{
    protected abstract ScienceShipRecord.TroubleType TroubleType { get; }
    protected override float GetTotalWorkAmount(float baseWorkAmount)
    {
        CompCrashedScienceShip shipComp = Target.TryGetComp<CompCrashedScienceShip>();
        if (shipComp is null)
        {
            return baseWorkAmount;
        }
        else
        {
            if (shipComp.ShipRecord.IsKeyTrouble(TroubleType))
            {
                return baseWorkAmount * (1f + (shipComp.ShipRecord.TroubleCount - 1) * 0.25f);
            }
            else
            {
                return baseWorkAmount;
            }
        }
    }
}

public class JobDriver_ScienceShip_GravityAdjuestment : JobDriver_CrashedScienceShip
{
    protected override ScienceShipRecord.TroubleType TroubleType => ScienceShipRecord.TroubleType.Gravitational;
    protected override void JobFinishResult(Pawn pawn)
    {
        Target.TryGetComp<CompCrashedScienceShip>()?.DoGravityAdjuestment();
    }
}

public class JobDriver_ScienceShip_GravitationalRepaire : JobDriver_CrashedScienceShip
{
    protected override ScienceShipRecord.TroubleType TroubleType => ScienceShipRecord.TroubleType.Gravitational;
    protected override void JobFinishResult(Pawn pawn)
    {
        Target.TryGetComp<CompCrashedScienceShip>()?.GravitationalRepaired(pawn);
    }
}

public class JobDriver_ScienceShip_MechanicalRepaire : JobDriver_CrashedScienceShip
{
    protected override ScienceShipRecord.TroubleType TroubleType => ScienceShipRecord.TroubleType.Mechanical;
    protected override void JobFinishResult(Pawn pawn)
    {
        Target.TryGetComp<CompCrashedScienceShip>()?.MechanicalRepaired(pawn);
    }
}

public class JobDriver_ScienceShip_ActivateCoolingDevice : JobDriver_CrashedScienceShip
{
    protected override ScienceShipRecord.TroubleType TroubleType => ScienceShipRecord.TroubleType.Hyperthermia;
    protected override void JobFinishResult(Pawn pawn)
    {
        Target.TryGetComp<CompCrashedScienceShip>()?.ActivateCoolingDevice();
    }
}

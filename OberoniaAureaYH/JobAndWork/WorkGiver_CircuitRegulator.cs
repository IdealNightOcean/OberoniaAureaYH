using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;
public class WorkGiver_CircuitRegulator : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(OARK_ThingDefOf.OA_RK_CircuitRegulator);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }
    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        CompCircuitRegulator compCircuitRegulator = t.TryGetComp<CompCircuitRegulator>();
        if (compCircuitRegulator is null)
        {
            return false;
        }
        if (!compCircuitRegulator.repairmentEnabled)
        {
            return false;
        }
        if (t.IsForbidden(pawn))
        {
            return false;
        }
        if (!pawn.CanReserve(t, 1, -1, null, forced))
        {
            return false;
        }
        return compCircuitRegulator.NeedRepairment;
    }
    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(OARK_ModDefOf.OA_RK_RepairCircuitRegulator, t);
    }
}


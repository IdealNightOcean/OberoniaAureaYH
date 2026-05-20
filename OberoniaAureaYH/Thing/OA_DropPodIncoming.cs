
using RimWorld;
using Verse;

namespace OberoniaAurea;

//金鸢尾兰运输舱
public class OA_DropPodIncoming : DropPodIncoming
{
    protected override void Impact()
    {
        GenExplosion.DoExplosion(
            center: Position,
            map: Map,
            radius: def.skyfaller.explosionRadius,
            damType: DamageDefOf.Smoke,
            instigator: null,
            postExplosionGasType: GasType.BlindSmoke);
        base.Impact();
    }
}
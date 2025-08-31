
using RimWorld;
using Verse;

namespace OberoniaAurea;

//金鸢尾兰运输舱
public class OA_DropPodIncoming : DropPodIncoming
{
    protected override void Impact()
    {
        GenExplosion.DoExplosion(Position, Map, def.skyfaller.explosionRadius, DamageDefOf.Smoke, null, postExplosionGasType: GasType.BlindSmoke);
        base.Impact();
    }
}
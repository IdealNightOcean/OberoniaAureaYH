using Verse;

namespace OberoniaAurea;

//位移爆炸
public class DisplacementExplosionExtension : DefModExtension
{
    public bool isRepel;

    public float displacementRadius;
    public float defaultDisplacement = 1f;

    public bool affectedByDistance;
    public SimpleCurve displacementCurveFromDistance;

    public bool affectedByMass;
    public SimpleCurve displacementCurveFromMass;

    public bool doExplosion;
    public bool onlyTargetHostile;
    public bool canApplyOnSelf = true;
}
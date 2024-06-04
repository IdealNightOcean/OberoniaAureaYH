using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompProperties_BarrageEnhancedFirepower : CompProperties
{
    public int maxLevel;
    public float incrementPreLevel;
    public int maxInterval = 600;
    public CompProperties_BarrageEnhancedFirepower()
    {
        compClass = typeof(CompBarrageEnhancedFirepower);
    }

}

public class CompBarrageEnhancedFirepower : ThingComp
{
    public CompProperties_BarrageEnhancedFirepower Props => (CompProperties_BarrageEnhancedFirepower)props;
    private Verb_BarrageEnhancedShoot AttackVerbInt;
    private Verb_BarrageEnhancedShoot AttackVerb => AttackVerbInt ??= (Verb_BarrageEnhancedShoot)parent.TryGetComp<CompEquippable>().PrimaryVerb;
    protected int CurLevel => Mathf.Min(AttackVerb.CurLevel, Props.maxLevel);
    public float DamageMultiplier => CurLevel * Props.incrementPreLevel;

    public int GetCurLevel()
    {
        if (Find.TickManager.TicksGame - AttackVerb.LastShotTick > Props.maxInterval)
        {
            AttackVerb.ResetBarrageLevel();
            return 0;
        }
        else
        {
            return Mathf.Min(AttackVerb.CurLevel, Props.maxLevel);
        }
    }
}

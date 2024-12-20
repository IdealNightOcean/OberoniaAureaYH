using Verse;

namespace OberoniaAurea;

public class Verb_BarrageEnhancedShoot : Verb_Shoot
{
    private int curLevel;
    public int CurLevel => curLevel;
    private CompBarrageEnhancedFirepower CompBEFInt;
    private CompBarrageEnhancedFirepower CompBEF => CompBEFInt ??= EquipmentSource.TryGetComp<CompBarrageEnhancedFirepower>();
    private int MaxInterval => CompBEF.Props.maxInterval;
    public override void WarmupComplete()
    {
        AdjustBarrageLevel();
        base.WarmupComplete();
    }

    private void AdjustBarrageLevel()
    {
        if (Find.TickManager.TicksGame - lastShotTick > MaxInterval)
        {
            curLevel = 0;
        }
        else
        {
            curLevel++;
        }
    }
    public void ResetBarrageLevel()
    {
        curLevel = 0;
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curLevel, "curLevel", 0);
    }

}

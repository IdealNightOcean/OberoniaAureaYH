using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//位移爆炸
public class CompProperties_DisplacementExplosive : CompProperties_Explosive
{
    public CompProperties_DisplacementExplosive()
    {
        compClass = typeof(CompDisplacementExplosive);
    }
}

public class CompDisplacementExplosive : CompExplosive
{
    private bool hasDoDisplacement;
    private DisplacementExplosionExtension ModEx_DE => parent.def.GetModExtension<DisplacementExplosionExtension>();
    private readonly List<Pawn> targetPawns = [];

    public override void CompTick()
    {
        base.CompTick();
        if (wickStarted && !hasDoDisplacement)
        {
            hasDoDisplacement = true;
            Map map = parent.MapHeld;
            IntVec3 ctrPosition = parent.PositionHeld;
            DisplacementExplosionExtension modEx_DE = ModEx_DE;
            GravityWeaponUtility.GetPawnsInRadius(ctrPosition, map, parent, modEx_DE.displacementRadius, targetPawns, modEx_DE.onlyTargetHostile);
            GravityWeaponUtility.TryDisplacement(ctrPosition, map, modEx_DE, targetPawns, PostPawnDisplaced);

            PostDisplaced(modEx_DE, map, ctrPosition);

            if (!modEx_DE.doExplosion && !parent.Destroyed)
            {
                destroyedThroughDetonation = true;
                parent.Kill();
                EndWickSustainer();
                wickStarted = false;
            }
        }
    }

    protected virtual void PostDisplaced(DisplacementExplosionExtension modEx_DE, Map map, IntVec3 ctrPos)
    {
        if (modEx_DE.effecter is null) { return; }
        Effecter effecter = modEx_DE.effecter.Spawn();
        effecter.scale = modEx_DE.displacementRadius * 0.2f;
        effecter.Trigger(new TargetInfo(ctrPos, map), new TargetInfo(ctrPos, map));
        effecter.Cleanup();
    }

    protected virtual void PostPawnDisplaced(Pawn pawn)
    {
        pawn.stances.stunner.StunFor(120, parent);
    }

    private void EndWickSustainer()
    {
        if (wickSoundSustainer is not null)
        {
            wickSoundSustainer.End();
            wickSoundSustainer = null;
        }
    }
}
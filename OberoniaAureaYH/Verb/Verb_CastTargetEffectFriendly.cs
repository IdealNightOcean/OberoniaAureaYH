using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class Verb_CastTargetEffectFriendly : Verb_CastTargetEffect
{
    public override void OnGUI(LocalTargetInfo target)
    {
        if (CanHitTarget(target) && IsValidlPawn(target))
        {
            if (verbProps.targetParams.CanTarget(target.ToTargetInfo(caster.Map)))
            {
                base.OnGUI(target);
            }
            else
            {
                GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
                if (!verbProps.invalidTargetPawn.NullOrEmpty())
                {
                    Widgets.MouseAttachedLabel(verbProps.invalidTargetPawn, 0f, -20f);
                }
            }
        }
        else
        {
            GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!IsValidlPawn(target))
        {
            return false;
        }
        return base.ValidateTarget(target, showMessages);
    }

    private static bool IsValidlPawn(LocalTargetInfo target)
    {
        Pawn pawn = target.Pawn;
        if (pawn is null)
        {
            return false;
        }
        if (pawn.Downed)
        {
            return true;
        }
        if (pawn.Faction.IsPlayerSafe() || pawn.IsPrisonerOfColony)
        {
            return true;
        }
        if (!Faction.OfPlayer.HostileTo(pawn.Faction))
        {
            return true;
        }
        return false;
    }
}

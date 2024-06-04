using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class Verb_CastTargetEffectFriendly : Verb_CastTargetEffect
{
	public override void OnGUI(LocalTargetInfo target)
	{
		if (CanHitTarget(target) && verbProps.targetParams.CanTarget(target.ToTargetInfo(caster.Map)))
		{
			if (target.Pawn != null && IsFriendly(target.Pawn))
			{
				base.OnGUI(target);
				return;
			}
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			if (!string.IsNullOrEmpty(verbProps.invalidTargetPawn))
			{
				Widgets.MouseAttachedLabel(verbProps.invalidTargetPawn, 0f, -20f);
			}
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		Pawn pawn = target.Pawn;
		return pawn != null && IsFriendly(pawn) && base.ValidateTarget(target, showMessages);
	}

	private static bool IsFriendly(Pawn pawn)
	{
		return pawn.Downed || pawn.IsColonist || pawn.IsQuestLodger() || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony || pawn.Faction == Faction.OfPlayer;
	}
}

using RimWorld;
using Verse;

namespace OberoniaAurea;

public class Verb_SummonEMP : Verb_Static
{
	protected override bool CastShot()
	{
		SummonEMPExtension modExtension = base.EquipmentSource.def.GetModExtension<SummonEMPExtension>();
		if (modExtension != null)
		{
			Thing thing = ThingMaker.MakeThing(modExtension.def);
			DropPodUtility.DropThingsNear(currentTarget.Cell, caster.Map, new Thing[1] { thing }, 60, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: true, allowFogged: true, caster.Faction);
			return true;
		}
		return false;
	}
}

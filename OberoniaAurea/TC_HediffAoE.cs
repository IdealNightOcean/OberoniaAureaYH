using RimWorld;
using Verse;

namespace OberoniaAurea;

public class TC_HediffAoE : ThingComp
{
	public TCP_HediffAoE Props => (TCP_HediffAoE)props;

	private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

	private bool IsPawnAffected(Pawn target)
	{
		return (PowerTrader == null || PowerTrader.PowerOn) && !target.Dead && target.health != null && target.Position.DistanceTo(parent.Position) <= Props.range && (!Props.onlyTargetMechs || target.RaceProps.IsMechanoid) && (!Props.onlyTargetOwnSide || target.IsColonist || target.IsQuestLodger() || target.IsPrisonerOfColony || target.IsSlaveOfColony);
	}

	public override void CompTick()
	{
		if (!parent.IsHashIntervalTick(Props.checkInterval))
		{
			return;
		}
		CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
		if (compPowerTrader != null && !compPowerTrader.PowerOn)
		{
			return;
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (!IsPawnAffected(item))
			{
				continue;
			}
			Hediff hediff = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
			if (hediff == null)
			{
				hediff = ((!Props.InBrain) ? item.health.AddHediff(Props.hediff) : item.health.AddHediff(Props.hediff, item.health.hediffSet.GetBrain()));
				hediff.Severity = 1f;
				HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
				if (hediffComp_Link != null)
				{
					hediffComp_Link.drawConnection = false;
					hediffComp_Link.other = parent;
				}
			}
			HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears == null)
			{
				Log.Error("TCP_HediffAoE has a hediff in props which does not have a HediffComp_Disappears");
			}
			else
			{
				hediffComp_Disappears.ticksToDisappear = Props.checkInterval + 1;
			}
		}
	}

	public override void PostDraw()
	{
		if (!Find.Selector.SelectedObjectsListForReading.Contains(parent))
		{
			return;
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (IsPawnAffected(item))
			{
				GenDraw.DrawLineBetween(item.DrawPos, parent.DrawPos);
			}
		}
	}
}

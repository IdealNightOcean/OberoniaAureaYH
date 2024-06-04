using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class TC_BatteryOA : CompPower
{
	private float storedEnergy;

	private const float SelfDischargingWatts = 5f;

	public new TCP_BatteryOA Props => (TCP_BatteryOA)props;

	public float AmountCanAccept
	{
		get
		{
			if (parent.IsBrokenDown())
			{
				return 0f;
			}
			return (Props.storedEnergyMax - storedEnergy) / Props.efficiency;
		}
	}

	public float StoredEnergy => storedEnergy;

	public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
		if (storedEnergy > Props.storedEnergyMax)
		{
			storedEnergy = Props.storedEnergyMax;
		}
	}

	public void AddEnergy(float amount)
	{
		if (amount < 0f)
		{
			Log.Error("Cannot add negative energy " + amount);
			return;
		}
		if (amount > AmountCanAccept)
		{
			amount = AmountCanAccept;
		}
		amount *= Props.efficiency;
		storedEnergy += amount;
	}

	public void SetStoredEnergyPct(float pct)
	{
		pct = Mathf.Clamp01(pct);
		storedEnergy = Props.storedEnergyMax * pct;
	}

	public override string CompInspectStringExtra()
	{
		string text = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + Props.storedEnergyMax.ToString("F0") + " Wd";
		text += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (Props.efficiency * 100f).ToString("F0") + "%";
		if (storedEnergy > 0f)
		{
			text += "\n" + "SelfDischarging".Translate() + ": " + 5f.ToString("F0") + " W";
		}
		return text + "\n" + base.CompInspectStringExtra();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fill",
				action = delegate
				{
					SetStoredEnergyPct(1f);
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Empty",
				action = delegate
				{
					SetStoredEnergyPct(0f);
				}
			};
		}
	}
}

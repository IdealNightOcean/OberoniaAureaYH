using RimWorld;

namespace OberoniaAurea;

public class TCP_BatteryOA : CompProperties_Power
{
	public float storedEnergyMax = 1000f;

	public float efficiency = 0.5f;

	public TCP_BatteryOA()
	{
		compClass = typeof(TC_BatteryOA);
	}
}

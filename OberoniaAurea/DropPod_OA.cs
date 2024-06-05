using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class DropPod_OA : Skyfaller, IActiveDropPod, IThingHolder
{
	public ActiveDropPodInfo Contents
	{
		get
		{
			return ((ActiveDropPod)innerContainer[0]).Contents;
		}
		set
		{
			((ActiveDropPod)innerContainer[0]).Contents = value;
		}
	}

	protected override void SpawnThings()
	{
		if (!Contents.spawnWipeMode.HasValue)
		{
			base.SpawnThings();
			return;
		}
		for (int num = innerContainer.Count - 1; num >= 0; num--)
		{
			GenSpawn.Spawn(innerContainer[num], base.Position, base.Map, Contents.spawnWipeMode.Value);
		}
	}

	protected override void Impact()
	{
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map, 1.2f);
		}
		FleckMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
		GenClamor.DoClamor(this, 15f, ClamorDefOf.Impact);
		GenExplosion.DoExplosion(base.Position, base.Map, 2.9f, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
		base.Impact();
	}
}

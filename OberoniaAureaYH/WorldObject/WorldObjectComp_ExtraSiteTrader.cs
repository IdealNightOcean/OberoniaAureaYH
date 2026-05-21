using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class WorldObjectCompProperties_ExtraSiteTrader : WorldObjectCompProperties
{
    public WorldObjectCompProperties_ExtraSiteTrader() => compClass = typeof(WorldObjectComp_ExtraSiteTrader);
}

public class WorldObjectComp_ExtraSiteTrader : WorldObjectComp
{
    private static readonly CachedTexture TradeCommandTex = new("UI/Commands/Trade");

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        foreach (Gizmo gizmo in base.GetCaravanGizmos(caravan))
            yield return gizmo;

        if (!ExtraSiteTraderManager.Instance.TryGetExtraSiteTrader(parent, out ExtraSiteTrader extraSiteTrader) || !extraSiteTrader.IsActive)
            yield break;

        Command_Action command_ExtraSiteTrader = new()
        {
            defaultLabel = "OARK_CommandExtraSiteTrade".Translate(extraSiteTrader.TraderFaction.Named(KeyLibrary_NamedArgument.FACTION)),
            defaultDesc = "CommandTradeDesc".Translate(),
            icon = TradeCommandTex.Texture,
            action = delegate
            {
                if (extraSiteTrader.SiteTrader is not null)
                {
                    Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, extraSiteTrader.TraderFaction, extraSiteTrader.SiteTrader.TraderKind);
                    Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, extraSiteTrader.SiteTrader));
                }
                else
                {
                    Log.Error($"[OARK] {parent}的{nameof(ExtraSiteTrader)} 不可用。");
                }
            }
        };
        yield return command_ExtraSiteTrader;
    }

    public override void PostDestroy()
    {
        base.PostDestroy();
        ExtraSiteTraderManager.Instance.DeregisterExtraSiteTrader(parent);
    }
}
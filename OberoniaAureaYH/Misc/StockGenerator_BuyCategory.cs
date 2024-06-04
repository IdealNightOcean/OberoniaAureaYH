using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;
public class StockGenerator_BuyCategory : StockGenerator
{
    public ThingCategoryDef categoryDef;
    public List<ThingDef> excludedThingDefs;
    public List<ThingCategoryDef> excludedCategories;

    public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
    {
        return Enumerable.Empty<Thing>();
    }

    public override bool HandlesThingDef(ThingDef t)
    {
        if (categoryDef.DescendantThingDefs.Contains(t) && (excludedThingDefs == null || !excludedThingDefs.Contains(t)))
        {
            if (excludedCategories != null)
            {
                return !excludedCategories.Any((ThingCategoryDef c) => c.DescendantThingDefs.Contains(t));
            }
            return true;
        }
        return false;
    }
    public override Tradeability TradeabilityFor(ThingDef thingDef)
    {
        if (thingDef.tradeability == Tradeability.None || !HandlesThingDef(thingDef))
        {
            return Tradeability.None;
        }
        return Tradeability.Sellable;
    }
}

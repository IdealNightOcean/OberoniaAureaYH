using RimWorld;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

//授勋官不携带启灵神经会报错，因此仪式开始时当场生成一个（一开始不携带，否则玩家会坑害授勋官）
public class QuestPart_OABestowingCeremony : QuestPart_BestowingCeremony
{
    protected override Lord MakeLord()
    {
        Lord lord = base.MakeLord();
        if (lord is not null && bestower is not null)
        {
            bestower.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier), 1);
        }
        return lord;
    }
}
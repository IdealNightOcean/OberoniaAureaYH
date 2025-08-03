using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class QuestNode_ScienceShip_SetQuestData : QuestNode
{
    protected override bool TestRunInt(Slate slate)
    {
        return ScienceDepartmentInteractHandler.IsScienceDepartmentInteractAvailable();
    }

    protected override void RunInt()
    {
        float rewardValue = 3000f + (WealthUtility.PlayerWealth / 5000f) * 100f;
        rewardValue = Mathf.Min(rewardValue, 24000f);

        int challengeRating = 1;
        float rewardMulti = 1f;
        bool hasShipTypeText = false;
        bool turbulentRegion = false;
        if (ScienceDepartmentInteractHandler.Instance?.ScienceShipRecord.HasValue ?? false)
        {
            ScienceShipRecord shipRecord = ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.Value;
            QuestGen.slate.Set("shipName", shipRecord.ShipName);
            string shipTypeText = ScienceShipRecord.GetShipTypeText(shipRecord.TypeOfShip);
            if (!shipTypeText.NullOrEmpty())
            {
                hasShipTypeText = true;
                QuestGen.slate.Set("shipTypeText", shipTypeText.Translate());
            }

            switch (shipRecord.TypeOfShip)
            {
                case ScienceShipRecord.ShipType.TurbulentRegion:
                    challengeRating = 3;
                    rewardMulti *= 2.5f;
                    turbulentRegion = true;
                    break;
                case ScienceShipRecord.ShipType.HighSignal:
                    challengeRating = 3;
                    rewardMulti *= 2f;
                    break;
                default: break;
            }
        }
        QuestGen.slate.Set("hasShipTypeText", hasShipTypeText);
        QuestGen.slate.Set("rewardValue", rewardValue * rewardMulti);
        QuestGen.slate.Set("turbulentRegion", turbulentRegion);
        QuestGen.quest.challengeRating = challengeRating;
    }
}

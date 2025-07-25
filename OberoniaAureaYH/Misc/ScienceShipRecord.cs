using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public struct ScienceShipRecord : IExposable
{
    public enum ShipType : byte
    {
        Normal,
        Settlement,
        TurbulentRegion,
        TravelRK,
        Fragile,
        HighSignal,
        Disaster
    }

    [Flags]
    public enum TroubleType : byte
    {
        None = 0,
        Mechanical = 1,
        InformationBase = 2,
        Gravitational = 4,
        Hyperthermia = 8
    }
    public static readonly IReadOnlyList<TroubleType> ScienceShipTroubleTypeList = Enum.GetValues(typeof(TroubleType)).Cast<TroubleType>().ToList();

    public enum EnvirAffectType : byte
    {
        None,
        WeakGravity,
        StrongWeakGravity,
        DisorderedGravity,
        DisorderedMagnetism,
    }

    private ShipType shipType;
    private string shipName;
    private TroubleType troubles;
    private TroubleType mainTrouble;
    private int troubleCount;
    private EnvirAffectType environmentAffect;

    public readonly ShipType TypeOfShip => shipType;
    public readonly EnvirAffectType EnvironmentAffect => environmentAffect;
    public readonly string ShipName => shipName;
    public readonly int TroubleCount => troubleCount;
    public readonly TroubleType MainTrouble => mainTrouble;

    public ScienceShipRecord()
    {
        troubles = TroubleType.None;
        mainTrouble = TroubleType.None;
    }

    public ScienceShipRecord(string shipName, ShipType shipType, EnvirAffectType environmentAffect)
    {
        this.shipType = shipType;
        this.shipName = shipName;
        this.environmentAffect = environmentAffect;
        troubles = TroubleType.None;
        mainTrouble = TroubleType.None;
    }

    public void SetEnvironmentAffect(EnvirAffectType environmentAffect)
    {
        this.environmentAffect = environmentAffect;
    }

    public void AddTrouble(TroubleType trouble)
    {
        if (trouble == TroubleType.None || HasTrouble(trouble))
        {
            return;
        }

        troubles |= trouble;
        troubleCount++;
    }

    public void SetMainTrouble(TroubleType trouble)
    {
        RemoveTrouble(mainTrouble);
        AddTrouble(trouble);
        mainTrouble = trouble;
    }

    public void RemoveTrouble(TroubleType trouble)
    {
        if (trouble == TroubleType.None || !HasTrouble(trouble))
        {
            return;
        }

        troubles &= ~trouble;
        troubleCount--;

        if (mainTrouble == trouble)
        {
            mainTrouble = TroubleType.None;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasTrouble(TroubleType trouble)
    {
        return (troubles & trouble) == trouble;
    }

    public readonly bool IsKeyTrouble(TroubleType trouble)
    {
        if (trouble == TroubleType.None || !HasTrouble(trouble))
        {
            return false;
        }
        if (shipType == ShipType.Disaster)
        {
            return true;
        }
        return trouble == mainTrouble;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref shipType, "shipType", ShipType.Normal);
        Scribe_Values.Look(ref shipName, "shipName");
        Scribe_Values.Look(ref troubles, "troubles");
        Scribe_Values.Look(ref mainTrouble, "mainTrouble", TroubleType.None);
        Scribe_Values.Look(ref troubleCount, "troubleCount", 0);
        Scribe_Values.Look(ref environmentAffect, "environmentAffect");
    }

    public static string GenerateShipName()
    {
        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_ModDefOf.OARK_PackScienceShipName);
        string shipName = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("shipName_root", grammarRequest));
        shipName += Rand.RangeInclusive(1, 99);

        return shipName;
    }

    public static ScienceShipRecord GenerateShip(string shipName = null)
    {
        float playerWealth = WealthUtility.PlayerWealth;

        List<(ShipType, float)> scienceShipChance =
            [
                (ShipType.Normal, playerWealth < 180000 ? 120 : 30),
                (ShipType.Settlement, 15),
                (ShipType.TurbulentRegion, 15),
                (ShipType.Fragile, 15),
                (ShipType.HighSignal, 15),
                (ShipType.Disaster, 10)
            ];
        if (OARK_ModDefOf.Rakinia_TravelRatkin is not null && Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.Rakinia_TravelRatkin) is not null)
        {
            scienceShipChance.Add((ShipType.TravelRK, 10));
        }

        ShipType shipType = scienceShipChance.RandomElementByWeight(tc => tc.Item2).Item1;
        EnvirAffectType environmentAffect = (EnvirAffectType)Rand.RangeInclusive(1, 4);

        ScienceShipRecord scienceShipRecord = new(shipName ?? GenerateShipName(), shipType, environmentAffect);

        int troubleCount = (playerWealth > 500000 || shipType == ShipType.Disaster) ? Rand.RangeInclusive(3, 4) : Rand.RangeInclusive(2, 3);
        List<TroubleType> troubles = ScienceShipTroubleTypeList.Where(t => t != TroubleType.None)
                                                                          .TakeRandomDistinct(troubleCount);

        foreach (TroubleType trouble in troubles)
        {
            scienceShipRecord.AddTrouble(trouble);
        }

        if (playerWealth > 800000)
        {
            scienceShipRecord.SetMainTrouble(troubles.Where(t => t != TroubleType.Hyperthermia).RandomElement());
        }
        else
        {
            scienceShipRecord.SetMainTrouble(TroubleType.None);
        }


        return scienceShipRecord;
    }

    public static string GetShipEnvirText(EnvirAffectType type)
    {
        return type switch
        {
            EnvirAffectType.WeakGravity => "OARK_ScienceShip_WeakGravityText",
            EnvirAffectType.StrongWeakGravity => "OARK_ScienceShip_StrongWeakGravityText",
            EnvirAffectType.DisorderedGravity => "OARK_ScienceShip_DisorderedGravityText",
            EnvirAffectType.DisorderedMagnetism => "OARK_ScienceShip_DisorderedMagnetismText",
            _ => "OARK_ScienceShipEnvir_NonEnvirText",
        };
    }

    public static string GetShipEnvirLabel(EnvirAffectType type)
    {
        return type switch
        {
            EnvirAffectType.WeakGravity => "OARK_ScienceShip_WeakGravity",
            EnvirAffectType.StrongWeakGravity => "OARK_ScienceShip_StrongWeakGravity",
            EnvirAffectType.DisorderedGravity => "OARK_ScienceShip_DisorderedGravity",
            EnvirAffectType.DisorderedMagnetism => "OARK_ScienceShip_DisorderedMagnetism",
            _ => "None",
        };
    }

    public static string GetShipTroubleLabel(TroubleType type)
    {
        return type switch
        {
            TroubleType.Mechanical => "OARK_ScienceShip_Mechanical",
            TroubleType.Gravitational => "OARK_ScienceShip_Gravitational",
            TroubleType.Hyperthermia => "OARK_ScienceShip_Hyperthermia",
            TroubleType.InformationBase => "OARK_ScienceShip_InformationBase",
            TroubleType.None => "None",
            _ => "Unknown",
        };
    }

    public static (bool, string) GetShipTypeText(ShipType type, bool fundDisaster)
    {
        if (fundDisaster && type == ShipType.Disaster)
        {
            return (true, "OARK_ScienceShipLaunch_DisasterText");
        }

        return type switch
        {
            ShipType.TravelRK => (true, "OARK_ScienceShipLaunch_TravelRKText"),
            ShipType.Fragile => (true, "OARK_ScienceShipLaunch_FragileText"),
            ShipType.HighSignal => (true, "OARK_ScienceShipLaunch_HighSignalText"),
            _ => (false, null),
        };
    }
}
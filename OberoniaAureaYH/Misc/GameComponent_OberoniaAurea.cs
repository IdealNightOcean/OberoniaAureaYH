using OberoniaAurea_Frame;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    private int ticksRemaining;
    public static GameComponent_OberoniaAurea Instance { get; private set; }

    private OAInteractHandler interactHandler;

    private int cachedYear = 2025;
    public bool newYearEventTriggeredOnce;

    public GameComponent_OberoniaAurea(Game game)
    {
        // OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance)); 
        if (Instance != this)
        {
            Log.Message($"[OARK] {nameof(GameComponent_OberoniaAurea)} Instance switched.".Colorize(Color.cyan));
        }
        Instance = this;
    }

    public static void ClearStaticCache()
    {
        //不能清理GameComponent，直接替换就行

        ModUtility.ClearStaticCache();
        OAInteractHandler.ClearStaticCache();
        ThoughtWorker_Precept_Snow.ClearStaticCache();
    }

    public override void StartedNewGame()
    {
        ClearStaticCache();

        GameStart();
    }

    public override void LoadedGame()
    {
        GameStart();
    }

    public override void GameComponentTick()
    {
        if (--ticksRemaining < 0)
        {
            ticksRemaining = 60000;
            interactHandler.TickDay();
        }
    }

    private void GameStart()
    {
        try
        {
            interactHandler ??= new OAInteractHandler();
        }
        catch (System.Exception ex)
        {
            Log.Error($"[OARK] An exception occurred during initializing {nameof(OAInteractHandler)} in {nameof(GameComponent_OberoniaAurea)}.{nameof(GameStart)}. \nException: \n{ex}");
            OAInteractHandler.ClearStaticCache();
            interactHandler = new OAInteractHandler();
        }
        ModUtility.Notify_GameStart();
        TryAddNewYearEnent();
    }

    private void TryAddNewYearEnent()
    {
        DateTime curDate = DateTime.Now;
        if (curDate.Year > cachedYear)
        {
            newYearEventTriggeredOnce = false;
            cachedYear = curDate.Year;
        }

        if (!newYearEventTriggeredOnce)
        {
            if (curDate.Month == 1 && curDate.Day <= 3)
            {
                Map map = Find.AnyPlayerHomeMap;
                if (map is not null && ModUtility.OAFaction is not null)
                {
                    if (ModUtility.OAFaction.PlayerRelationKind == FactionRelationKind.Ally)
                    {
                        IncidentParms parms = new()
                        {
                            target = map,
                            faction = ModUtility.OAFaction
                        };
                        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_ModDefOf.OARatkin_NewYearEvent, 2500, parms);
                    }
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), -1);
        Scribe_Deep.Look(ref interactHandler, nameof(interactHandler));

        Scribe_Values.Look(ref cachedYear, nameof(cachedYear), -1);
        Scribe_Values.Look(ref newYearEventTriggeredOnce, nameof(newYearEventTriggeredOnce), defaultValue: false);
    }
}

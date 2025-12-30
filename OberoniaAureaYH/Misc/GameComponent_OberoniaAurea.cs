using OberoniaAurea_Frame;
using RimWorld;
using System;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    private int ticksRemaining = 60000;
    public static GameComponent_OberoniaAurea Instance { get; private set; }

    private OAInteractHandler interactHandler;
    private ScienceDepartmentInteractHandler sdInteractHandler;

    private int cachedYear = 2025;
    public bool newYearEventTriggeredOnce;

    public GameComponent_OberoniaAurea(Game game) => Instance = this;

    public override void StartedNewGame() => GameStart();
    public override void LoadedGame() => GameStart();

    public override void GameComponentTick()
    {
        if (--ticksRemaining < 0)
        {
            ticksRemaining = 60000;

            interactHandler.TickDay();
            if (ModsConfig.OdysseyActive)
            {
                sdInteractHandler.TickDay();
            }
        }
    }

    private void GameStart()
    {
        interactHandler ??= new OAInteractHandler();
        if (ModsConfig.OdysseyActive)
        {
            sdInteractHandler ??= new ScienceDepartmentInteractHandler();
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
                if (map is not null && ModUtility.OAFaction is not null && ModUtility.OAFaction.PlayerRelationKind == FactionRelationKind.Ally)
                {
                    IncidentParms parms = new()
                    {
                        target = map,
                        faction = ModUtility.OAFaction,
                    };
                    OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_NewYearEvent, 2500, parms);
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), -1);
        Scribe_Deep.Look(ref interactHandler, nameof(interactHandler));
        Scribe_Deep.Look(ref sdInteractHandler, nameof(sdInteractHandler));

        Scribe_Values.Look(ref cachedYear, nameof(cachedYear), 2025);
        Scribe_Values.Look(ref newYearEventTriggeredOnce, nameof(newYearEventTriggeredOnce), defaultValue: false);
    }
}
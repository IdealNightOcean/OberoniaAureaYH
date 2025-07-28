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

    public bool newYearEventTriggeredOnce;

    public GameComponent_OberoniaAurea(Game game) => Instance = this;

    public override void StartedNewGame()
    {
        GameStart();
    }
    public override void LoadedGame()
    {
        GameStart();

        TempNullParmsTargetFix();
    }

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

    private void TempNullParmsTargetFix()
    {
        try
        {
            foreach (QueuedIncident queuedInc in Find.Storyteller.incidentQueue)
            {
                if (queuedInc.FiringIncident.parms.target is null)
                {
                    queuedInc.FiringIncident.parms.target = Find.World;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to fix the null parms.target for QueuedIncident.firingInc: " + ex);
        }
    }

    private void TryAddNewYearEnent()
    {
        if (!newYearEventTriggeredOnce)
        {
            DateTime date = DateTime.Now;
            if (date.Month == 1 && date.Day <= 3)
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
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
        Scribe_Deep.Look(ref interactHandler, "interactHandler");
        Scribe_Deep.Look(ref sdInteractHandler, "sdInteractHandler");
        Scribe_Values.Look(ref newYearEventTriggeredOnce, "newYearEventTriggeredOnce", defaultValue: false);
    }
}
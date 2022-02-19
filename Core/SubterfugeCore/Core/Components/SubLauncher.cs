using System;
using System.Linq;
using GameEventModels;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.GameEvents;

namespace SubterfugeCore.Core.Components
{
    public class SubLauncher : EntityComponent
    {

        /// <summary>
        /// The number of drillers at the outpost
        /// </summary>
        private DrillerCarrier _drillerCarrier;
        private SpecialistManager _specialistManager;
        
        public event EventHandler<OnSubLaunchEventArgs> OnSubLaunch;
        public event EventHandler<OnUndoSubLaunchEventArgs> OnUndoSubLaunch;

        public SubLauncher(Entity parent) : base(parent)
        {
            this._drillerCarrier = parent.GetComponent<DrillerCarrier>();
            this._specialistManager = parent.GetComponent<SpecialistManager>();
        }

        public Sub LaunchSub(GameState state, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();
            Entity source = state.GetEntity(launchData.SourceId);
            Entity destination = state.GetEntity(launchData.DestinationId);

            if (destination != null && _drillerCarrier.HasDrillers(launchData.DrillerCount))
            {
                _drillerCarrier.RemoveDrillers(launchData.DrillerCount);
                Sub launchedSub = new Sub(launchEvent.GetEventId(), source, destination, state.GetCurrentTick(), launchData.DrillerCount, this._drillerCarrier.GetOwner());
                this._specialistManager.transferSpecialistsById(launchedSub.GetComponent<SpecialistManager>(), launchData.SpecialistIds.ToList());
                state.AddSub(launchedSub);
                
                OnSubLaunch?.Invoke(this, new OnSubLaunchEventArgs()
                {
                    Destination = destination,
                    LaunchedSub = launchedSub,
                    LaunchEvent = launchEvent,
                    Source = source
                });
                
                return launchedSub;
            }

            return null;
        }

        public void UndoLaunch(GameState state, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();

            if (launchEvent.GetActiveSub() != null && launchEvent.GetActiveSub() is Sub)
            {
                Sub launchedSub = launchEvent.GetActiveSub();
                _drillerCarrier.AddDrillers(launchData.DrillerCount);
                launchedSub.GetComponent<SpecialistManager>().transferSpecialistsTo(_specialistManager);
                state.RemoveSub(launchEvent.GetActiveSub());
                
                OnUndoSubLaunch?.Invoke(this, new OnUndoSubLaunchEventArgs()
                {
                    Destination = state.GetEntity(launchData.DestinationId),
                    LaunchedSub = launchedSub,
                    LaunchEvent = launchEvent,
                    Source = state.GetEntity(launchData.SourceId)
                });
            }
        }

    }
}
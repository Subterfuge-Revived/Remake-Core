using System;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.GameState;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class SubLauncher : EntityComponent, ISubLaunchEventPublisher
    {

        /// <summary>
        /// The number of drillers at the outpost
        /// </summary>
        private readonly DrillerCarrier _drillerCarrier;
        private readonly SpecialistManager _specialistManager;
        
        public event EventHandler<OnSubLaunchEventArgs> OnSubLaunched;

        public SubLauncher(IEntity parent) : base(parent)
        {
            _drillerCarrier = parent.GetComponent<DrillerCarrier>();
            _specialistManager = parent.GetComponent<SpecialistManager>();
        }

        public Sub LaunchSub(
            TimeMachine timeMachine,
            LaunchEvent launchEvent
        ) {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();
            GameState.GameState state = timeMachine.GetState();
            Entity source = state.GetEntity(launchData.SourceId);
            Entity destination = state.GetEntity(launchData.DestinationId);

            if (destination != null && _drillerCarrier.HasDrillers(launchData.DrillerCount))
            {
                _drillerCarrier.RemoveDrillers(launchData.DrillerCount);
                Sub launchedSub = new Sub(launchEvent.GetEventId(), source, destination, state.GetCurrentTick(), launchData.DrillerCount, this._drillerCarrier.GetOwner(), timeMachine);
                this._specialistManager.TransferSpecialistsById(launchedSub.GetComponent<SpecialistManager>(), launchData.SpecialistIds.ToList());
                state.AddSub(launchedSub);
                launchEvent.SetLaunchedSub(launchedSub);
                
                OnSubLaunched?.Invoke(this, new OnSubLaunchEventArgs()
                {
                    Destination = destination,
                    LaunchedSub = launchedSub,
                    LaunchEvent = launchEvent,
                    Source = source,
                    Direction = TimeMachineDirection.FORWARD
                });
                
                return launchedSub;
            }

            return null;
        }

        public void UndoLaunch(IGameState state, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();

            if (launchEvent.GetActiveSub() != null)
            {
                Sub launchedSub = launchEvent.GetActiveSub();
                _drillerCarrier.AddDrillers(launchData.DrillerCount);
                launchedSub.GetComponent<SpecialistManager>().TransferSpecialistsTo(_specialistManager);
                state.RemoveSub(launchEvent.GetActiveSub());
                
                OnSubLaunched?.Invoke(this, new OnSubLaunchEventArgs()
                {
                    LaunchedSub = launchedSub,
                    LaunchEvent = launchEvent,
                    Source = Parent,
                    Direction = TimeMachineDirection.REVERSE
                });
                
            }
        }

    }
}
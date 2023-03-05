using System;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class SubLauncher : EntityComponent, ISubLaunchEventPublisher
    {

        /// <summary>
        /// The number of drillers at the outpost
        /// </summary>
        private readonly DrillerCarrier _drillerCarrier;
        private readonly SpecialistManager _specialistManager;
        
        public event EventHandler<OnSubLaunchEventArgs> OnSubLaunched;

        public bool CanTargetSubs { get; set; } = false;

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
            GameState state = timeMachine.GetState();
            Entity source = state.GetEntity(launchData.SourceId);
            Entity destination = state.GetEntity(launchData.DestinationId);

            if (destination is Sub && CanTargetSubs == false)
            {
                if (!CanTargetSubs || !launchData.SpecialistIds.Contains(SpecialistTypeId.Pirate.ToString()))
                {
                    // Error. Attempting to target a sub when you cannot target subs or have not brought the pirate with you.
                    return null;
                }
            }

            if (destination != null && _drillerCarrier.HasDrillers(launchData.DrillerCount))
            {
                _drillerCarrier.AlterDrillers(launchData.DrillerCount * -1);
                Sub launchedSub = new Sub(launchEvent.GetEventId(), source, destination, state.GetCurrentTick(), launchData.DrillerCount, this._drillerCarrier.GetOwner(), timeMachine);
                this._specialistManager.TransferSpecialistsById(launchedSub.GetComponent<SpecialistManager>(), launchData.SpecialistIds.ToList(), timeMachine);
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
                destination.GetComponent<PositionManager>().OnTargeted(launchedSub);
                
                return launchedSub;
            }

            return null;
        }

        public void UndoLaunch(TimeMachine timeMachine, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();

            Sub launchedSub = launchEvent.GetActiveSub();
            if (launchedSub != null)
            {
                _drillerCarrier.AlterDrillers(launchData.DrillerCount);
                launchedSub.GetComponent<SpecialistManager>().TransferSpecialistsTo(_specialistManager, timeMachine);
                timeMachine.GetState().RemoveSub(launchEvent.GetActiveSub());
                
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
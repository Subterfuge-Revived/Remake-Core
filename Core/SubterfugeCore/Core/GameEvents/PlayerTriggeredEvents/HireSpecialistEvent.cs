using System.Linq;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class HireSpecialistEvent : PlayerTriggeredEvent
    {
        private IEntity HiredAt;
        private Specialist HiredSpecialist;
        
        public HireSpecialistEvent(GameRoomEvent model) : base(model)
        {
        }
        
        
        public HireSpecialistEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<HireSpecialistEventData>(Model.GameEventData.SerializedEventData);
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            HireSpecialistEventData hireEvent = GetEventData();
            
            var hireLocation = timeMachine.GetState().GetEntity(hireEvent.HireLocation);
            if (hireLocation == null)
            {
                EventSuccess = false;
                return EventSuccess;
            }

            var specialist = this.IssuedBy().SpecialistPool.HireSpecialist(hireEvent.SpecialistTypeIdHired);
            hireLocation.GetComponent<SpecialistManager>()
                .HireSpecialist(specialist, timeMachine);

            HiredSpecialist = specialist;
            HiredAt = hireLocation;
            
            EventSuccess = true;
            return EventSuccess;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            if (EventSuccess)
            {
                HiredAt.GetComponent<SpecialistManager>().UndoHireSpecialist(HiredSpecialist, timeMachine);
                
                this.IssuedBy()
                    .SpecialistPool
                    .UndoSpecialistHire();
                
                return true;
            }

            return false;
        }

        public override bool WasEventSuccessful()
        {
            return EventSuccess;
        }
    }
}
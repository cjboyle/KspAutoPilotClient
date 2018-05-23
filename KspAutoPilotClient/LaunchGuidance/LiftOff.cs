using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient.LaunchGuidance
{
    public class LiftOff : RunMode
    {
        public LiftOff(Vehicle vehicle, int stage, int step) : base(vehicle, stage, step)
        {
            base.vehicle.AutoPilot.Engage();
        }

        protected override void Execute()
        {
            vehicle.Control.Throttle = 1;

            if (vehicle.VerticalSpeed > 1)
                vehicle.Control.Gear = false;

            vehicle.AutoPilot.TargetPitch = 90;
        }
    }
}

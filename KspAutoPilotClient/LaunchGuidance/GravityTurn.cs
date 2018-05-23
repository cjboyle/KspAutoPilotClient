using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient.LaunchGuidance
{
    public class GravityTurn : RunMode
    {
        private readonly float _maxQThrottle;

        public GravityTurn(Vehicle vehicle, int stage, int step, float maxQThrottle = 0.82f) : base(vehicle, stage, step)
        {
            base.vehicle.AutoPilot.Engage();
            _maxQThrottle = maxQThrottle;
        }

        protected override void Execute()
        {
            if (vehicle.Mach >= 0.75 && vehicle.Mach < 1.05)
                vehicle.Control.Throttle = _maxQThrottle;
            else
                vehicle.Control.Throttle = 1;

            vehicle.AutoPilot.TargetPitchAndHeading(Math.Max(5, (float)VelocityAnglePitch()), 90);
        }

        private double VelocityAnglePitch()
        {
            return Math.Atan(900 / vehicle.Speed).R2D();
        }
    }
}

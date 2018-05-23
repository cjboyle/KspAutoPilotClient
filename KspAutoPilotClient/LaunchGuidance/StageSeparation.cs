using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient.LaunchGuidance
{
    public class StageSeparation : RunMode
    {
        private Vehicle _stage;
        private bool _isCompleted;

        public StageSeparation(Vehicle vehicle, int stage, int step) : base(vehicle, stage, step)
        {
            _isCompleted = false;
        }

        protected override void Execute()
        {
            vehicle.Control.Throttle = 0;
            System.Threading.Thread.Sleep(1000);
            _stage = vehicle.Control.ActivateNextStage().Select(v => new Vehicle(v)).First();
            System.Threading.Thread.Sleep(2000);
            _isCompleted = true;
        }

        public Vehicle GetNextStage() => _isCompleted ? _stage : null;
    }
}

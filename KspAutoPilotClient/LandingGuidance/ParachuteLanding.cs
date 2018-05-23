using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient.LandingGuidance
{
    public class ParachuteLanding : RunMode
    {
        public ParachuteLanding(Vehicle vehicle, int stage, int step) : base(vehicle, stage, step)
        {
        }

        protected override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}

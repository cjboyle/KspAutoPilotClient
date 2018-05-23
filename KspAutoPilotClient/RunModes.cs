using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    /// <summary>
    /// Defines an action performed in the current vehicle runmode
    /// </summary>
    /// <param name="runmode">A reference to the current runmode</param>
    public delegate void RunAction();

    public class RunModes
    {
        private delegate void RunAction(ref int runmode);

        SortedDictionary<int, RunAction> RunActions;
        private readonly Vehicle _vehicle;

        public RunModes(Vehicle vehicle)
        {
            _vehicle = vehicle;
            RunActions.Add(0, null);
        }

    }
}

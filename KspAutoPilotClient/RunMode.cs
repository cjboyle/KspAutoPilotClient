using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public abstract class RunMode : IRunMode
    {
        protected readonly Vehicle vehicle;

        private static readonly Dictionary<int, int> _stageStep = new Dictionary<int, int>();

        private readonly int _stage;
        private readonly int _step;
        private bool _isComplete;

        /// <summary>
        /// The current step in this stage
        /// </summary>
        public int Current
        {
            get => _stageStep[_stage];
            protected set => _stageStep[_stage] = value;
        }

        /// <summary>
        /// Adds a run mode action for the vehicle.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="stage">The vehicle stage (for running scripts on multiple vessels</param>
        /// <param name="step">The run mode step number</param>
        public RunMode(Vehicle vehicle, int stage, int step)
        {
            this.vehicle = vehicle;
            _stage = stage;
            _step = step;
            _isComplete = false;

            if (!_stageStep.ContainsKey(_stage))
                _stageStep.Add(_stage, 0);
        }

        /// <summary>
        /// Handles user processes after the run mode has completed
        /// </summary>
        public event EventHandler OnComplete = delegate { };
        private void NotifyComplete()
        {
            OnComplete(_isComplete, EventArgs.Empty);
        }

        /// <summary>
        /// Executes the vessel action.
        /// </summary>
        /// <param name="when">The running condition</param>
        /// <param name="until">Predicate to end the current runmode</param>
        public virtual void Execute(Func<bool> until)
        {
            if (Current == _step)
            {
                if (until?.Invoke() ?? true)
                {
                    Console.WriteLine($"Run Mode {Current} Complete.");
                    Current++;
                    _isComplete = true;
                }
                else
                {
                    Execute();
                }
            }
        }

        protected abstract void Execute();
    }



    public interface IRunMode
    {
        /// <summary>
        /// Executes the vessel action.
        /// </summary>
        /// <param name="when">The running condition</param>
        /// <param name="until">Predicate to invoke the next run mode</param>
        void Execute(Func<bool> until);

        /// <summary>
        /// The current run mode number
        /// </summary>
        int Current { get; }
    }
}

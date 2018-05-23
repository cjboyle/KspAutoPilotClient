using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public class PidController
    {
        /// <summary>
        /// The proportional gain
        /// </summary>
        public double P { get; set; }

        /// <summary>
        /// The integral of gain
        /// </summary>
        public double I { get; set; }

        /// <summary>
        /// The derivative of gain
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// The control range lower bound
        /// </summary>
        public double Min { get; private set; }

        /// <summary>
        /// The control range upper bound
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        /// The time (in seconds) of the last controller update
        /// </summary>
        private double _lastUpdate = 0;

        private double _p = 0;
        private double _pLast = 0;
        private double _pTotal = 0;
        private double _i = 0;
        private double _d = 0;

        /// <summary>
        /// Creates a new PID loop controller instance
        /// </summary>
        /// <param name="p"></param>
        /// <param name="i"></param>
        /// <param name="d"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public PidController(double p, double i, double d, double min, double max)
        {
            P = p;
            I = i;
            D = d;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Get the next control value in the PID loop
        /// </summary>
        /// <param name="current">The current value to process</param>
        /// <param name="target">The target process value</param>
        /// <returns>A value within the instance bounds</returns>
        public double Seek(double current, double target)
        {
            double time = (DateTime.Now - DateTime.MinValue).TotalSeconds;
            double output = 0;

            _p = target - current;

            if (_lastUpdate > 0)
            {
                _i = _pTotal + ((time - _lastUpdate) * (_p + _pLast) / 2);
                _i = Limit(_i); // Protect the value from too much gain
                _d = (_p - _pLast) / (time - _lastUpdate);
            }

            output = (_p * P) + (_i * I) + (_d * D);

            _pLast = _p;
            _pTotal = _i;
            _lastUpdate = time;

            return Limit(output);
        }

        /// <summary>
        /// Keeps the a new value within the instance control range bounds
        /// <param name="value"></param>
        /// <returns></returns>
        private double Limit(double value) => Math.Max(Min, Math.Min(value, Max));
    }
}

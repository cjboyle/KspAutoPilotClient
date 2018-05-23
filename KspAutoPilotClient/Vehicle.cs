using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    /// <summary>
    /// Wrapper for the kRPC Vessel class
    /// </summary>
    public class Vehicle : Vessel
    {
        public Vehicle(ulong id) : base(Program.Connection, id)
        {
            // Ensures no issues with later stages
            //ActivateAllEngines();
        }

        public Vehicle(Vessel vessel) : this(vessel.id)
        {
        }

        /// <summary>
        /// Activates all engines
        /// </summary>
        public void ActivateAllEngines()
        {
            foreach (var eng in CurrentStageEngines)
            {
                eng.Active = true;
            }
        }

        private IEnumerable<Engine> CurrentStageEngines => Parts.InStage(Control.CurrentStage).Select(i => i.Engine).Where(e => e != null);

        /// <summary>
        /// Deactivates all engines
        /// </summary>
        public void DeactivateAllEngines()
        {
            foreach (var eng in CurrentStageEngines)
            {
                eng.Active = false;
            }
        }

        /// <summary>
        /// Activates all engines having the given tag name
        /// </summary>
        /// <param name="tag">The part tag to filter</param>
        /// <param name="strict">Whether other engines will be deactivated</param>
        public void ActivateEnginesByTag(string tag, bool strict = true)
        {
            if (strict)
                DeactivateAllEngines();

            foreach (var eng in CurrentStageEngines)
            {
                if (eng.Part.Tag.ToLower().Contains(tag.ToLower()))
                {
                    eng.Active = true;
                }
            }
        }

        /// <summary>
        /// Activates the number of engines, counting radially outwards from the central axis.
        /// Currently only activates engines on the Y axis.
        /// </summary>
        /// <param name="numEngines">The number of engines to activate</param>
        /// <param name="strict">Whether other engines will be deactivated</param>
        public void ActivateCenterEngines(int numEngines = 1, bool strict = true)
        {
            if (numEngines > Parts.Engines.Count())
                throw new ArgumentOutOfRangeException("numEngines");

            if (strict)
                DeactivateAllEngines();

            // var centerEngines = 
            CurrentStageEngines.First(
                e => Math.Abs(e.Part.Position(ReferenceFrame).Item1) < 0.001
                && e.Part.Position(ReferenceFrame).Item2 <= 0 // only want engines below the center of mass
                && Math.Abs(e.Part.Position(ReferenceFrame).Item3) < 0.001).Active = true;
        }

        /// <summary>
        /// Jettisons all fairing parts
        /// </summary>
        public void JettisonFairing()
        {
            foreach (var f in Parts.Fairings)
            {
                f.Jettison();
            }
        }

        /// <summary>
        /// The time it takes the active engines to ignite and power to full thrust (calculated at launch)
        /// </summary>
        public double MaxThrustIgnitionTime { get; set; }

        /// <summary>
        /// A listing of all solid rocket boosters on the vehicle
        /// </summary>
        public IEnumerable<Engine> SolidRocketBoosters => Parts.Engines.Where(e => !e.CanShutdown);

        /// <summary>
        /// A listing of all liquid fuel rockets on the vehicle
        /// </summary>
        public IEnumerable<Engine> LiquidFuelRocketEngines => Parts.Engines.Where(e => e.CanShutdown);

        /// <summary>
        /// The potential change in velocity for the vessel (in meters-per-second)
        /// </summary>
        public double DeltaV => ExhaustVelocity * Math.Log(Mass / DryMass, Math.E);

        /// <summary>
        /// The length of time to burn the remaining Delta-V (in seconds)
        /// </summary>
        public double TimeToBurnout => (Mass * ExhaustVelocity / Thrust) * (1.0 - Math.Exp(-1.0 * DeltaV / ExhaustVelocity));

        private double ExhaustVelocity => 9.807 * SpecificImpulse;
        private double PropellantFlowRate => Thrust / ExhaustVelocity;

        /// <summary>
        /// The time to decelerate the vehicle to a stop during a braking burn
        /// </summary>
        public double DecelerationTime
        {
            get
            {
                double sine = Math.Sin(Flight(Orbit.Body.ReferenceFrame).Pitch);
                double g = Orbit.Body.SurfaceGravity;
                double t = .95 * MaxThrust / Mass; // 5% safety margin
                double decel = 0.5 * (-2 * g * sine + Math.Sqrt(Math.Pow(2 * g * sine, 2) + 4 * (t * t - g * g)));
                return Speed / decel;
            }
        }

        public double TimeToLandingBurn
        {
            get
            {
                double radius = Orbit.Body.EquatorialRadius + MeanAltitude;
                double anomaly = -1 * Orbit.TrueAnomalyAtRadius(radius);
                double impactTime = Orbit.UTAtTrueAnomaly(anomaly);
                double burnTime = impactTime - (DecelerationTime / 2);
                return burnTime - Program.SpaceCenter.UT;
            }
        }

        ///// <summary>
        ///// The altitude after performing a braking burn maneuver
        ///// </summary>
        //public double DecelerationAltitude
        //{
        //    get
        //    {
        //        return SurfaceAltitude - DecelerationDistance;
        //        //return SurfaceAltitude - (VerticalSpeed * deltaT) + (((AvailableThrust / Mass) - Orbit.Body.SurfaceGravity) * deltaT * deltaT / 2);
        //    }
        //}

        ///// <summary>
        ///// The distance required for the vessel to decelerate during a braking burn at full thrust
        ///// </summary>
        //public double DecelerationDistance
        //{
        //    get
        //    {
        //        return (-1.0 * (VerticalSpeed * VerticalSpeed)) / (2 * NetGravity);
        //    }
        //}

        //public double NetGravity => (AvailableThrust / Mass) - Orbit.Body.SurfaceGravity;

        /// <summary>
        /// The current Thrust-Weight ratio for all active engines
        /// </summary>
        public double TWR => Thrust / (Mass * Orbit.Body.SurfaceGravity);


        #region Stream Properties

        /// <summary>
        /// The altitude above the body surface terrain or sea-level
        /// </summary>
        public double SurfaceAltitude
        {
            get
            {
                if (_surfaceAltitudeStream == null)
                    _surfaceAltitudeStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).SurfaceAltitude);
                return _gForceStream.Get();
            }
        }
        private Stream<double> _surfaceAltitudeStream;

        /// <summary>
        /// The altitude above sea-level
        /// </summary>
        public double MeanAltitude
        {
            get
            {
                if (_meanAltitudeStream == null)
                    _meanAltitudeStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).MeanAltitude);
                return _meanAltitudeStream.Get();
            }
        }
        private Stream<double> _meanAltitudeStream;

        /// <summary>
        /// The vertical speed component of the vessel in the celestial body reference frame
        /// </summary>
        public double VerticalSpeed
        {
            get
            {
                if (_verticalSpeedStream == null)
                    _verticalSpeedStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).VerticalSpeed);
                return _verticalSpeedStream.Get();
            }
        }
        private Stream<double> _verticalSpeedStream;

        /// <summary>
        /// The horizontal speed component of the vessel in the celestial body reference frame
        /// </summary>
        public double HorizontalSpeed
        {
            get
            {
                if (_horizontalSpeedStream == null)
                    _horizontalSpeedStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).HorizontalSpeed);
                return _horizontalSpeedStream.Get();
            }
        }
        private Stream<double> _horizontalSpeedStream;

        /// <summary>
        /// The speed of the vessel in the celestial body reference frame
        /// </summary>
        public double Speed
        {
            get
            {
                if (_speedStream == null)
                    _speedStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).Speed);
                return _speedStream.Get();
            }
        }
        private Stream<double> _speedStream;

        /// <summary>
        /// The sum of the forces acting on the vessel in m/s^2
        /// </summary>
        public float GForce
        {
            get
            {
                if (_gForceStream == null)
                    _gForceStream = Program.Connection.AddStream(() => Flight(ReferenceFrame).GForce);
                return _gForceStream.Get();
            }
        }
        private Stream<float> _gForceStream;

        /// <summary>
        /// The mach speed value of the vessel in the celestial body reference frame
        /// </summary>
        public float Mach
        {
            get
            {
                if (_machStream == null)
                    _machStream = Program.Connection.AddStream(() => Flight(Orbit.Body.ReferenceFrame).Mach);
                return _machStream.Get();
            }
        }
        private Stream<float> _machStream;

        #endregion
    }
}

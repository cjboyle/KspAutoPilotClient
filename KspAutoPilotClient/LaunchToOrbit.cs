using KRPC.Client.Services.SpaceCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KspAutoPilotClient;

namespace KspAutoPilotClient
{
    /// <summary>
    /// Basic program to launch a two-stage rocket payload to LEO (RO and RSS)
    /// </summary>
    public class LaunchToOrbit
    {
        /// <summary>
        /// The defacto altitude for outer space
        /// </summary>
        public const int KarmanLine = 100000;

        /// <summary>
        /// The altitude below which orbital objects experience rapidly decreasing altitude due to atmospheric drag
        /// </summary>
        public const int OrbitalDecayLine = 160000;
        public const int LowEarthOrbit = 2000000;
        public const int GeosynchronousOrbit = 35786000;

        public Vehicle LaunchVehicle { get; private set; }
        public Vehicle UpperStage { get; private set; }

        public float TargetApoapsis { get; private set; }
        public float TargetPeriapsis { get; private set; }
        public float TargetInclination { get; private set; }

        private List<IRunMode> _runModes;
        private double _mecoMach;

        /// <summary>
        /// Launch a vehicle into an eccentric orbit
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="apoapsis"></param>
        /// <param name="periapsis"></param>
        public LaunchToOrbit(Vehicle vehicle, float apoapsis, float periapsis)
        {
            LaunchVehicle = vehicle;
            TargetApoapsis = apoapsis;
            TargetPeriapsis = periapsis;

            _runModes = new List<IRunMode>();
        }

        /// <summary>
        /// Launch a vehicle into a ciruclarized orbit
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="altitude"></param>
        public LaunchToOrbit(Vehicle vehicle, float altitude) : this(vehicle, altitude, altitude)
        {
        }

        /// <summary>
        /// Set up the launch sequence and ascent profile
        /// </summary>
        public void Initialize()
        {
            //_mecoMach = 6.8; // GTO, ~2350 m/s
            //_mecoMach = 5.2; // MEO, ~1800 m/s
            _mecoMach = 4.8; // LEO, ~1650 m/s
            PreLaunch();
        }

        /// <summary>
        /// Pre-launch set up of the vehicle
        /// </summary>
        public void PreLaunch()
        {
            if (LaunchVehicle.Situation != VesselSituation.PreLaunch && LaunchVehicle.Situation != VesselSituation.Landed)
            {
                throw new Exception("Vehicle is not in pre-launch state: " + LaunchVehicle.Situation.ToString());
            }

            LaunchVehicle.Control.Throttle = 0;
            LaunchVehicle.Control.SAS = false;
            LaunchVehicle.Control.RCS = false;
            LaunchVehicle.Control.Brakes = false;
            LaunchVehicle.Control.Lights = true;
            LaunchVehicle.Control.SolarPanels = false;
            LaunchVehicle.Control.Antennas = false;
        }

        /// <summary>
        /// Launches with a ten second countdown timer (ignition at T-3 seconds)
        /// </summary>
        protected void LaunchWithCountdown()
        {
            LaunchVehicle.AutoPilot.Engage();
            LaunchVehicle.AutoPilot.TargetPitchAndHeading(90, 90);

            for (int i = 10; i >= 0; i--)
            {
                if (i == 3)
                {
                    new Task(() =>
                    {
                        LaunchVehicle.Control.Throttle = 1;
                        LaunchVehicle.Control.ActivateNextStage(); // Returns list of stage ejected?
                    }).Start();
                }
                Thread.Sleep(1000);
                Print("Launch in T-" + i, 0, 0);
            }

            while (LaunchVehicle.TWR <= 1.1) ; // Wait for engines to power up fully

            LaunchVehicle.Parts.LaunchClamps.AsParallel().ForAll(clamp => clamp.Release());
            Print("Lift off of the " + LaunchVehicle.Name + " rocket.", 0, 0);
        }

        protected void LaunchWithoutCountdown()
        {
            Thread.Sleep(5000); // For running from VS. Remove when in-game buttons or GUI implemented
            LaunchVehicle.AutoPilot.Engage();
            LaunchVehicle.AutoPilot.TargetPitchAndHeading(90, 90);

            LaunchVehicle.Control.Throttle = 1;
            LaunchVehicle.Control.ActivateNextStage(); // Returns list of stage ejected?

            while (LaunchVehicle.TWR <= 1.1) ; // Wait for engines to power up fully

            LaunchVehicle.Parts.LaunchClamps.AsParallel().ForAll(clamp => clamp.Release());
            Print("Lift off of the " + LaunchVehicle.Name + " rocket.", 0, 0);
        }

        /// <summary>
        /// Start and execute the flight sequence
        /// <param name="countdown"></param>
        /// </summary>
        public void Execute(bool countdown = true)
        {
            if (countdown)
                LaunchWithCountdown();
            else
                LaunchWithoutCountdown();

            while (UpperStage == null)
            {
                new LaunchGuidance.LiftOff(LaunchVehicle, 1, 0).Execute(until: () => LaunchVehicle.VerticalSpeed > 100);
                new LaunchGuidance.GravityTurn(LaunchVehicle, 1, 1).Execute(until: () => LaunchVehicle.MeanAltitude > _meco);
                var ss = new LaunchGuidance.StageSeparation(LaunchVehicle, 1, 2);
                ss.OnComplete += (s, e) => UpperStage = ss.GetNextStage();
            }

            UpperStage.AutoPilot.Engage();

            LaunchVehicle.AutoPilot.Engage();
            LaunchVehicle.Control.RCS = true;
            LaunchVehicle.AutoPilot.TargetPitchAndHeading(-5, 270);
            LaunchVehicle.ActivateCenterEngines();

            while (Math.Abs(LaunchVehicle.Flight(LaunchVehicle.Orbit.Body.ReferenceFrame).Heading - 270) < 10) ;

            UpperStage.Control.Throttle = 1;
            LaunchVehicle.Control.Throttle = 1;

        }

        /// <summary>
        /// Start and execute the maneuver sequence asynchronously
        /// </summary>
        /// <param name="countdown"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(bool countdown = true)
        {
            await Task.Run(() => Execute(countdown));
        }

        private void LiftOff(ref int runmode)
        {
            if (LaunchVehicle.VerticalSpeed > 1)
                LaunchVehicle.Control.Gear = false;

            LaunchVehicle.AutoPilot.TargetPitchAndHeading(90, 90);

            if (LaunchVehicle.VerticalSpeed > 100)
                runmode++; // Next runmode
        }

        private void GravityTurn(ref int runmode)
        {
            //LaunchVehicle.AutoPilot.TargetPitchAndHeading(Math.Max(5, (float)InterpolatePitch(LaunchVehicle.MeanAltitude)), 90);
            LaunchVehicle.AutoPilot.TargetPitchAndHeading(Math.Max(5, (float)VelocityAnglePitch()), 90);
            Print("Altitude: " + LaunchVehicle.MeanAltitude, 1, 0);
            Print("Pitch to: " + InterpolatePitch(LaunchVehicle.MeanAltitude), 2, 0);
            if (LaunchVehicle.MeanAltitude >= _meco)
            {
                LaunchVehicle.Control.Throttle = 0;
                //runmode++;

                //LaunchVehicle.Control.ActivateNextStage();
                LaunchVehicle.ActivateCenterEngines();
                Thread.Sleep(4000);
                LaunchVehicle.Control.Throttle = 1;
            }
        }

        /// <summary>
        /// Gets the pitch for the current altitude based on a curve interpolated from the SpaceX Zuma mission data.
        /// <para>https://www.flightclub.io/result?id=ed57a581-e1e9-41b0-a328-1c67ae465dbe</para>
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double InterpolatePitch(double x)
        {
            // Evaluated in Excel and Wolfram Alpha: trajectory pitch from vertical, where x = altitude, for LEO launches with MECO at ~65 km altitude
            //return 90 - (Math.Atan(0.3563 + (0.00001166 * x)) * 57.2958); // Convert the radians to degrees
            double val = 2.19.PQ(-26, x, 6) - 5.0.PQ(-21, x, 5) + 4.48.PQ(-16, x, 4) - 1.97.PQ(-11, x, 3) + 4.4.PQ(-7, x, 2) - 5.21.PQ(-3, x, 1) + 9.63;
            return Math.Min(90, val);
        }

        public double ProgradePitch()
        {
            var tempRF = LaunchVehicle.AutoPilot.ReferenceFrame;
            LaunchVehicle.AutoPilot.ReferenceFrame = LaunchVehicle.SurfaceVelocityReferenceFrame;
            var result = LaunchVehicle.Flight(LaunchVehicle.SurfaceVelocityReferenceFrame).Prograde;
            throw new NotImplementedException();
        }

        public double VelocityAnglePitch()
        {
            return Math.Atan(900 / LaunchVehicle.Speed).R2D();
        }

        private void Print(string text, int? y = null, int? x = null)
        {
            if (x != null && y != null)
            {
                Console.SetCursorPosition(x.Value, y.Value);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(x.Value, y.Value);
            }
            Console.WriteLine(text);
        }
    }
}

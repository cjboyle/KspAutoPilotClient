using KRPC.Client.Services.SpaceCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public class TestLandingScript
    {
        public TestLandingScript(Vehicle vehicle, int height)
        {
            LaunchVehicle = vehicle;
            this.height = height;
        }

        public Vehicle LaunchVehicle { get; }

        private readonly int height;

        /// <summary>
        /// Pre-launch set up
        /// </summary>
        public void Initialize()
        {
            if (LaunchVehicle.Situation != VesselSituation.PreLaunch)
            {
                throw new Exception("Vehicle is not in pre-launch state.");
            }

            LaunchVehicle.Control.Throttle = 0;
            LaunchVehicle.Control.SAS = false;
            LaunchVehicle.Control.RCS = false;
            LaunchVehicle.Control.Lights = true;
            LaunchVehicle.Control.SolarPanels = false;
            LaunchVehicle.Control.Antennas = false;

            if (LaunchVehicle.Parts.LaunchClamps.Any())
            {
                LaunchVehicle.Control.Gear = false;
            }
        }

        /// <summary>
        /// Start and execute launch sequence
        /// </summary>
        public void Execute(bool countdown = false)
        {
            #region LAUNCHPAD STATE

            Thread.Sleep(5000);
            Console.WriteLine("Surface Alt: " + LaunchVehicle.SurfaceAltitude);
            Thread.Sleep(2000);

            LaunchVehicle.AutoPilot.Engage();
            LaunchVehicle.AutoPilot.TargetPitchAndHeading(90, 90);

            var timer = DateTime.Now;
            LaunchVehicle.Control.ActivateNextStage(); // Returns list of stage ejected?
            LaunchVehicle.Control.Throttle = 1;

            while (LaunchVehicle.TWR <= 1.1) ; // Wait for engines to power up fully
            LaunchVehicle.MaxThrustIgnitionTime = (DateTime.Now - timer).TotalMilliseconds / 1000;
            LaunchVehicle.Parts.LaunchClamps.AsParallel().ForAll(clamp => clamp.Release()); // Release launch clamps

            int runmode = 1;

            #endregion

            while (runmode != 0)
            {
                LaunchVehicle.AutoPilot.TargetPitchAndHeading(90, 90);

                if (runmode == 1)
                {
                    
                    LaunchVehicle.Control.Throttle = 1;
                    LaunchVehicle.Control.Gear = false;

                    if (LaunchVehicle.SurfaceAltitude > height)
                    {
                        runmode = 2;
                        LaunchVehicle.Control.Throttle = 0;
                        LaunchVehicle.AutoPilot.SAS = true;
                        LaunchVehicle.Control.RCS = true;
                        LaunchVehicle.Control.Brakes = true;
                        LaunchVehicle.AutoPilot.SASMode = SASMode.AntiRadial;
                    }
                }
                else if (runmode == 2)
                {
                    var alt = LaunchVehicle.MeanAltitude;
                    Thread.Sleep(500);
                    if (LaunchVehicle.MeanAltitude < alt)
                        runmode = 3;
                }
                //else if (runmode == 3)
                //{
                //    if (25 >= LaunchVehicle.DecelerationAltitude)
                //    {
                //        Console.WriteLine("1 " + LaunchVehicle.DecelerationAltitude);
                //        LaunchVehicle.Control.Throttle = 1;
                //    }

                //    if (LaunchVehicle.SurfaceAltitude < 750) LaunchVehicle.Control.Gear = true;

                //    if (LaunchVehicle.Flight(LaunchVehicle.Orbit.Body.ReferenceFrame).VerticalSpeed < 1)
                //    {
                //        LaunchVehicle.Control.Throttle = 0;
                //        runmode = 0;
                //    }
                //}
            }
        }

        private void Print(string text, int? y = null, int? x = null)
        {
            if (x != null && y != null)
                Console.SetCursorPosition(x.Value, y.Value);
            Console.WriteLine(text);
        }
    }
}

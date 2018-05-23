using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    class Program : IDisposable
    {
        private static Connection _connection;
        private static System.Net.IPAddress _ipAddress = null;
        private static int _rpcPort = 50000;
        private static int _streamPort = 50001;

        /// <summary>
        /// Get the current kRPC connection to the game
        /// </summary>
        public static Connection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new Connection("C# AutoPilot", _ipAddress, _rpcPort, _streamPort);
                }
                return _connection;
            }
        }

        /// <summary>
        /// A reference to the kRPC SpaceCenter API on the current connection
        /// </summary>
        public static Service SpaceCenter
        {
            get
            {
                return Connection.SpaceCenter();
            }
        }

        public static void Main(string[] args)
        {
            //var vessel = new Vehicle(SpaceCenter.ActiveVessel);

            //foreach(var eng in vessel.LiquidFuelRocketEngines)
            //{
            //    var t = eng.Part.Position(vessel.ReferenceFrame);
            //    Console.WriteLine(t.Item1 + " : " + t.Item2 + " : " + t.Item3);
            //}
            //vessel.ActivateCenterEngines();
            //vessel.ActivateAllEngines();

            try
            {
                //var test = new LaunchToOrbit(null, 400000);
                //for (int i = 0; i < 3000; i += 100)
                //{
                //        Console.WriteLine(i + " :: " + test.InterpolatePitch(i));
                //}
                _ipAddress = System.Net.IPAddress.Loopback;
                var seq = new LaunchToOrbit(new Vehicle(SpaceCenter.ActiveVessel), 250000);
                //var seq = new TestLandingScript(new Vehicle(SpaceCenter.ActiveVessel), 5000);
                seq.Initialize();
                seq.Execute(true);
            }
            catch (RPCException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}

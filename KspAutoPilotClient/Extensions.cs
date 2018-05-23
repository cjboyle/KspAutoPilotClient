using KRPC.Client.Services.SpaceCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public static class Extensions
    {
        /// <summary>
        /// Approximation of the force gravity at a given altitude.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="altitude"></param>
        /// <returns></returns>
        public static double GravityAtAltitude(this CelestialBody body, double altitude)
        {
            float rad = body.EquatorialRadius;
            return body.SurfaceGravity * Math.Pow(rad / (rad + altitude), 2);
        }
    }
}

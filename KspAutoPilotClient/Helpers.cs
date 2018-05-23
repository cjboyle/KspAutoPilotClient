using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KspAutoPilotClient
{
    public static class Helpers
    {
        /// <summary>
        /// Converts a vector to a scalar value
        /// </summary>
        /// <param name="vec">A vector in 3D space</param>
        /// <returns></returns>
        public static double V2S(this Tuple<double, double, double> vec)
        {
            return Math.Sqrt(Math.Pow(vec.Item1, 2) + Math.Pow(vec.Item2, 2) + Math.Pow(vec.Item3, 2));
        }

        /// <summary>
        /// Converts a vector to a scalar value
        /// </summary>
        /// <param name="vec">A vector in 2D space</param>
        /// <returns></returns>
        public static double V2S(this Tuple<double, double> vec)
        {
            return Math.Sqrt(Math.Pow(vec.Item1, 2) + Math.Pow(vec.Item2, 2));
        }

        /// <summary>
        /// Converts a value from degrees to radians
        /// </summary>
        /// <param name="deg">A value in degrees</param>
        /// <returns></returns>
        public static double D2R(this double deg)
        {
            return deg * Math.PI / 180;
        }

        /// <summary>
        /// Converts a value from radians to degrees
        /// </summary>
        /// <param name="rad">A value in radians</param>
        /// <returns></returns>
        public static double R2D(this double rad)
        {
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Evaluates a polynomial value on a curve (e.g. 4.21E-26 x^3)
        /// </summary>
        /// <param name="coefficient"></param>
        /// <param name="exponent"></param>
        /// <param name="variable"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double PQ(this double coefficient, double exponent, double variable, double degree)
        {
            return coefficient * Math.Pow(10, exponent) * Math.Pow(variable, degree);
        }

        /// <summary>
        /// Evaluates a polynomial value on a curve (e.g. 4.21 x^3)
        /// </summary>
        /// <param name="coefficient"></param>
        /// <param name="exponent"></param>
        /// <param name="variable"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double PQ(this double coefficient, double variable, double degree)
        {
            return coefficient * Math.Pow(variable, degree);
        }
    }
}

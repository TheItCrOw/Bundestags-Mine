using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.Utility
{
    public static class MathHelper
    {
        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        /// <summary>
        /// Normalizes the vector by truning all values into the sum of 1
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<double> NormalizeVector(double[] vector)
        {
            var length = vector.Length;
            var result = new double[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = vector[i] / length;
            }
            return result.ToList();
        } 

        public static double GetDistanceBetweenTwoPoints(double x1, double x2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2));
        }
    }
}

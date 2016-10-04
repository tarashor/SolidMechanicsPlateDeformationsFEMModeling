using System;
using System.Collections.Generic;
using System.Text;

namespace FEMLibrary
{
    public class GaussianQuadrature
    {
        public static readonly double[] NODES = { -Math.Sqrt(3.0 / 5.0), 0, Math.Sqrt(3.0 / 5.0) };
        public static readonly double[] WEIGHT = { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 };
        public const int ORDER = 3;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramFEM
{
    public class Force
    {
        private double load;
        private double h;

        public Force(double load, double h)
        {
            this.h = h;
            this.load = load;
        }

        public double F10(double alfa1)
        {
            return 0;
            //return load;
        }
        public double F11(double alfa1)
        {
            return 0;
            //return load;
        }
        public double F12(double alfa1)
        {
            return 0;
            //return load;
        }
        public double F30(double alfa1)
        {
            //if (Math.Abs(alfa1 - 5) < 0.00001)
            //    return load;
            //else
            return 0.5 * load ;
        }
        public double F31(double alfa1)
        {
            //if (Math.Abs(alfa1 - 5) < 0.00001)
            //    return load;
            //else
            return 0.5 * load ;
        }
        public double F32(double alfa1)
        {
            //if (Math.Abs(alfa1 - 5) < 0.00001)
            //return load;
            //else
            return 2.0 * load / 3.0;
        }
    }
}

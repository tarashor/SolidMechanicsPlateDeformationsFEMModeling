using System;
using System.Collections.Generic;
using System.Text;
using AlgebraClasses;

namespace ProgramFEM
{
    public class AnaliticalMukha
    {
        double l;
        double h;
        double E;
        double v;
        double q;

        public AnaliticalMukha(double l, double h, double E, double v, double q)
        {
            this.l = l;
            this.h = h;
            this.E = E;
            this.v = v;
            this.q = q;
        }

        public Vector U(Vector alfa)
        {
            Vector res = new Vector(alfa.Length);

            double l_alfa2 = (l - alfa[0]) * (l - alfa[0]);
            double l_alfa3 = (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]);
            double l_alfa4 = (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]);
            double l_alfa5 = (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]);
            double l_alfa7 = (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]) * (l - alfa[0]);
            double l2 = l * l;
            double l3 = l * l * l;
            double l4 = l * l * l * l;
            double l5 = l * l * l * l * l;
            double l6 = l * l * l * l * l * l;
            double l7 = l * l * l * l * l * l * l;

            double y = 2 * (1 - v * v) * (l_alfa3 - l3) / (E * h * h * h);

            double w = (6 * (1 + v) * q / (5 * E * h)) * (l2 - l_alfa2)
                + (1 - v * v) * q * (l_alfa4 - l4) / (2 * E * h * h * h)
                + 2 * (1 - v * v) * q * l3 * alfa[0] / (E * h * h * h);

            double a22 = 5 * E * h / (12 * (1 + v));
            double a33 = E * h * h * h / (12 * (1 - v * v));

            double u1 = (l_alfa3 - l3) / (3 * a22 * a22);
            double u2 = (5 * l_alfa2 * l3 - 2 * l_alfa5 - 3 * l5) / (15 * a22 * a33);
            double u3 = (2 * l_alfa7 - 7 * l_alfa4 * l3 - 14 * l6 * alfa[0] + 5 * l7) / (126 * a33 * a33);


            double u = (u1 + u2 + u3) * q * q / 8;

            res[0] = u + alfa[2] * y;
            res[1] = 0;
            res[2] = w;

            return res;
        }
    }

    public class AnaliticalMarchuk
    {
        double l;
        double h;
        double E;
        double v;
        double q;
        public AnaliticalMarchuk(double l, double h, double E, double v, double q)
        {
            this.l = l;
            this.h = h;
            this.E = E;
            this.v = v;
            this.p = -q;

            D = E * h * h * h  / (12 * (1 - v * v));
            Lambda = 14 * E * h / (30 * (1 + v));
            l2 = l * l;
            l3 = l * l * l;
            l4 = l * l * l * l;
            l5 = l * l * l * l * l;
            l6 = l * l * l * l * l * l;
            l7 = l * l * l * l * l * l * l;
            

            wK1 = p / (2 * Lambda);
            wK2 = p / (6 * D);
        }

        double l2; 
        double l3;
        double l4;
        double l5;
        double l6;
        double l7;

        double wK1;
        double wK2;

        double p;
        double Lambda;
        double D;


        public Vector U(Vector alfa)
        {
            Vector res = new Vector(alfa.Length);

            double L = l / 2;

            double x = alfa[0] - L;
            double z = alfa[2];//- model.Shape.H / 2;

            
            double w = wK1 * ((x - L) * (x - L) - l2)
                - wK2 / 4 * ((x - L) * (x - L) * (x - L) * (x - L) - l4)
                - wK2 * (l3 * x + l3 * L);

            double u = p * p * (1 / (6 * Lambda * D)) * ((x - L) * (x - L) * (x - L) * (x - L) * (x - L) / 5 + l3 * (x - L) * (x - L) * 0.5 - 0.3 * l5) -
                        p * p / (2 * Lambda * Lambda) * ((x - L) * (x - L) * (x - L) / 3 + l3 / 3) -
                        p * p / (72 * D * D) * ((x - L) * (x - L) * (x - L) * (x - L) * (x - L) * (x - L) * (x - L) / 7 + (x - L) * (x - L) * (x - L) * (x - L) * l3 / 2 + l6 * x + l7 / 7);
                        //- p * p / (28 * D * D) * l7;


            double y = wK2 * ((x - L) * (x - L) * (x - L)+l3);

            res[0] = u + z * y;
            res[1] = 0;
            res[2] = w;

            return res;
        }


        //public Vector U(Vector alfa)
        //{
        //    Vector res = new Vector(alfa.Length);// 

        //    double alfa1 = ((1 + v) * v * v) / (1 - v - 2 * v * v);

        //    double D = E * h * h * h * (1 + alfa1)/ (12 * (1 - v * v));
        //    double Lambda = 14 * E * h / (30 * (1 + v));
        //    double p = -q;
        //    double l2 = l * l;
        //    double l3 = l * l * l;
        //    double l4 = l * l * l * l;
        //    double L= l/2;

        //    double wK1 = p / (2 * Lambda);
        //    double wK2 = p / (6 * D);


        //    double x = alfa[0] - L;

        //    double y = 0;
        //    double w = wK1 * ((x - L) * (x - L) - l2) 
        //        - wK2 / 4 * ((x - L) * (x - L) * (x - L) * (x - L) - l4) 
        //        - wK2 * (l3 * x + l3 * L);
        //    double u = 0;

        //    res[0] = u + alfa[2] * y;
        //    res[1] = 0;
        //    res[2] = w;

        //    return res;
        //}
    }
}

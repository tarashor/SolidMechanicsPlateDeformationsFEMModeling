using AlgebraClasses;
using System;

namespace FEMLibrary
{
    public delegate double Func1D(double alfa);

    public static class Constants
    {
        public const int DIMENSION_COUNT = 3;
        public const int APROXIMATION_FIRST = 3;
        public const int APROXIMATION_ONE_ELEMENT = 12;
        public const int NON_ZERO_DEFORMATION_TENSOR_ELEMENT = 9;
        public const int W_LENGTH = 3;
    }

    public class ApproximationResult
    {
        private ApproximationFunctionsFirst p;
        private Shape shape;
        Vector result;
        Vector nodes;
        int N;

        Func1D[] U1;
        Func1D[] U3;

        public ApproximationResult(Shape shape, Vector result)
        {
            this.shape = shape;
            this.result = result;
            N = result.Length / 6;
            p = new ApproximationFunctionsFirst(shape.H);
            nodes = new Vector(N);
            double interval = shape.L / (N - 1);
            for (int i = 0; i < N; i++)
            {
                nodes[i] = interval * i;
            }
            U1 = new Func1D[Constants.APROXIMATION_FIRST];
            U3 = new Func1D[Constants.APROXIMATION_FIRST];
            U1[0] = U10;
            U1[1] = U11;
            U1[2] = U12;
            U3[0] = U30;
            U3[1] = U31;
            U3[2] = U32;

        }

        int FindCurIndex(double alfa1)
        {
            int res = 0;
            if ((alfa1 <= shape.L) && (alfa1 >= 0))
            {
                res = 1;
                while ((res < N) && (nodes[res] < alfa1))
                {
                    res++;
                }
            }
            //else
            //{
            //    if (alfa1 >= shape.L)
            //    {
            //        res = nodes.Length;
            //    }
            //}
            return res;
        }

        double U10(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 6] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);

        }
        double U11(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 5] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k + 1] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);
        }
        double U12(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 4] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k + 2] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);
        }
        double U30(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 3] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k + 3] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);
        }
        double U31(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 2] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k + 4] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);
        }
        double U32(double alfa1)
        {
            int k = FindCurIndex(alfa1);
            /*if (k == 0) k = 1;
            if (k == nodes.Length) k = nodes.Length - 1;*/
            return result[6 * k - 1] * (nodes[k] - alfa1) / (nodes[k] - nodes[k - 1]) + result[6 * k + 5] * (alfa1 - nodes[k - 1]) / (nodes[k] - nodes[k - 1]);
        }

        public Vector U(Vector alfa)
        {
            if (alfa.Length == Constants.DIMENSION_COUNT)
            {
                Vector res = new Vector(Constants.DIMENSION_COUNT);
                for (int i = 0; i < Constants.APROXIMATION_FIRST; i++)
                {
                    res[0] += U1[i](alfa[0]) * p[i](alfa[2]);
                    res[2] += U3[i](alfa[0]) * p[i](alfa[2]);
                }
                res[1] = 0;
                return res;
            }
            else throw new Exception("Error while evaluating F Vector; Reason: Support only 3D!!!");
        }

    }

    public class ApproximationFunctionsP
    {
        private Func1D[] p = new Func1D[Constants.APROXIMATION_FIRST];
        public Func1D this[int index]
        { 
            get { return p[index];}  
            set { p[index] = value;}
        }
    }

    public class ApproximationFunctionsFirst
    {
        private double h;
        ApproximationFunctionsP p;

        public double p0(double alfa3)
        {
            return 0.5 - alfa3 / h;
        }
        public double p1(double alfa3)
        {
            return 0.5 + alfa3 / h;
        }
        public double p2(double alfa3)
        {
            return 1 - (4 * alfa3 * alfa3) / (h * h);
        }

        public Func1D this[int index]
        {
            get 
            {
                return p[index];
            }
        }
        public ApproximationFunctionsFirst(double h)
        {
            this.h = h;
            p = new ApproximationFunctionsP();
            p[0] = p0;
            p[1] = p1;
            p[2] = p2;
        }

    }
    
    public class Load
    {
        ApproximationFunctionsFirst p;
        private Shape shape;

        public Func1D[] F1
        {
            get;
            set;
        }

        public Func1D[] F3
        {
            get; 
            set;
        }
            

        public Load(Shape shape)
        {
            this.shape = shape;
            p = new ApproximationFunctionsFirst(shape.H);
            F1 = new Func1D[Constants.APROXIMATION_FIRST];
            F3 = new Func1D[Constants.APROXIMATION_FIRST];
        }


        public Vector F(Vector alfa)
        {
            if (alfa.Length == Constants.DIMENSION_COUNT)
            {
                Vector res = new Vector(Constants.DIMENSION_COUNT);
                for (int i = 0; i < Constants.APROXIMATION_FIRST; i++)
                {
                    res[0] += F1[i](alfa[0]) * p[i](alfa[2]);
                    res[2] += F3[i](alfa[0]) * p[i](alfa[2]);
                }
                res[1] = 0;
                return res;
            }
            else throw new Exception("Error while evaluating F Vector; Reason: Support only 3D!!!");
        }
    }

    public class Material
    {
        /// <summary>
        /// Young modulus
        /// </summary>
        public double E
        {
            get;
            set;
        }


        /// <summary>
        /// Poisson ratio
        /// </summary>
        public double V
        {
            get;
            set;
        }        
    }

    public class Shape
    {
        /// <summary>
        /// Thickness
        /// </summary>
        public double H
        {
            get;
            set;
        }

        /// <summary>
        /// Length
        /// </summary>
        public double L
        {
            get;
            set;
        }

        /// <summary>
        /// Curvature
        /// </summary>
        public double K
        {
            get;
            set;
        }
    }

    public class FiniteElement1D
    {
        public double End
        {
            get;
            set;
        }

        public double Begin
        {
            get;
            set;
        }

        public double Length()
        {
            return End - Begin;
        }

        public FiniteElement1D(double begin, double end)
        {
            Begin = begin;
            End = end;
        }
    }

    public class Mesh
    {
        FiniteElement1D[] mesh;

        public Mesh()
        {
            mesh = new FiniteElement1D[0];
        }

        public Mesh(Shape shape, int N)
        {
            mesh = new FiniteElement1D[N];
            double interval = shape.L / N;
            double begin = 0;
            double end = begin + interval;
            for (int i = 0; i < N; i++)
            {
                mesh[i] = new FiniteElement1D(begin, end);
                begin = end;
                end += interval;
            }
        }

        public FiniteElement1D this[int index]
        {
            get 
            {
                return mesh[index];
            }
        }

        public int Count 
        {
            get { return mesh.Length; }
        }
    }

    public class Model
    {

        #region - Construction Properties -

        public Material material { get; set; }
        public Shape shape { get; set; }
        
        #endregion

        public Load load
        {
            get;
            set;
        }

        public Mesh mesh
        {
            get;
            set;
        }

        public Model()
        {
            material = new Material();
            shape = new Shape();
            load = new Load(shape);
            mesh = new Mesh();
        }

        public Model(double e, double v, double h, double l, double k, int N)
        {
            material = new Material();
            shape = new Shape();
            material.E = e;
            material.V = v;
            shape.H = h;
            shape.L = l;
            shape.K = k;
            load = new Load(shape);
            mesh = new Mesh(shape, N);
        }

    }

}

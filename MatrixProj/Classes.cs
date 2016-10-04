using System;
using System.Collections.Generic;
using System.Text;

namespace AlgebraClasses
{
    public class CountException : Exception
    {
        public CountException() { }
        public CountException(string message) : base(message) { }
        public CountException(string message, Exception inner) : base(message, inner) { }
    }

   
    public class Vector
    {
        private double[] vector;
        private int n; // kilkist elementiv

        

        public Vector(int length)
        {
            vector = new double[length];
            n = length;
        }
        public Vector(Vector v)
        {
            this.n = v.n;
            vector = new double[this.n];
            for (int i = 0; i < this.n; i++)
            {
                vector[i] = v[i];
            }
        }

        #region - Properties -
        public int Length
        {
            get { return n; }
        }

        public double this[int i]
        {
            get
            {
                if ((0 <= i) && (i < n))
                    return vector[i];
                else throw new IndexOutOfRangeException();
            }
            set
            {
                if ((0 <= i) && (i <= n))
                    vector[i] = value;
                else throw new IndexOutOfRangeException();
            }
        }

        #endregion


        public override string ToString()
        {
            string str_vector = "{";
            for (int i = 0; i < n; i++)
            {
                str_vector += vector[i].ToString("00.000") + "   ";
            }
            str_vector += "}";
            return str_vector;
        }

        #region - Reload operation -
        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Length == v2.Length)
            {
                Vector res = new Vector(v1.Length);
                for (int i = 0; i < v1.Length; i++)
                {
                    res[i] = v1[i] + v2[i];
                }
                return res;
            }
            else
            {
                throw new CountException("Sum of vector cannot be count!!!");
            }
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Length == v2.Length)
            {
                Vector res = new Vector(v1.Length);
                for (int i = 0; i < v1.Length; i++)
                {
                    res[i] = v1[i] - v2[i];
                }
                return res;
            }
            else
            {
                throw new CountException("Sum of vector cannot be count!!!");
            }
        }

        public static Vector operator *(double c, Vector v)
        {
            Vector res = new Vector(v.Length);
            for (int i = 0; i < v.Length; i++)
            {
                res[i] = c * v[i];
            }
            return res;
        }

        public static Vector operator *(Vector v,double c)
        {
            Vector res = new Vector(v.Length);
            for (int i = 0; i < v.Length; i++)
            {
                res[i] = c * v[i];
            }
            return res;
        }

        public static double operator *(Vector v1, Vector v2)
        {
            if (v1.Length == v2.Length)
            {
                double res = 0;
                for (int i = 0; i < v1.Length; i++)
                {
                    res += v1[i] * v2[i];
                }
                return res;
            }
            else throw new CountException("Can't Vector multiplied by Vector!!!");
        }

        public static Vector operator *(Vector v, Matrix m)
        {
            if (v.n == m.CountRows)
            {
                Vector res = new Vector(m.CountColumns);
                for (int i = 0; i < m.CountColumns; i++)
                {
                    for (int j = 0; j < m.CountRows; j++)
                    {
                        res[i] += v[j] * m[j, i];
                    }
                }
                return res;
            }
            else throw new CountException("Can't Vector multiplied by Matrix!!!");
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            if (v.n == m.CountColumns)
            {
                Vector res = new Vector(m.CountRows);
                for (int i = 0; i < m.CountRows; i++)
                {
                    for (int j = 0; j < m.CountColumns; j++)
                    {
                        res[i] += v[j] * m[i, j];
                    }
                }
                return res;
            }
            else throw new CountException("Can't Matrix multiplied by Vector!!!");
        }
        #endregion

        public Vector SubVector(int start, int length)
        {
            Vector subvector = new Vector(length);
            for (int i = 0; i < length; i++)
            {
                subvector[i] = vector[start + i];
            }
            return subvector;
        }

        public static double Norm(Vector v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i] * v[i];
            }
            return Math.Sqrt(sum);
        }
    }

    public class Matrix
    {
        private double[,] matrix;
        private int n; // kilkist rjadky
        private int m; // kilkist stovpciv

        private Matrix L;
        private Matrix U;
        private bool GetLUPerformed;

        public Matrix(int n, int m)
        {
            matrix = new double[n, m];
            this.n = n;
            this.m = m;
            GetLUPerformed = false;
        }
        public Matrix(Matrix m)
        {
            this.n = m.n;
            this.m = m.m;
            matrix = new double[this.n, this.m];
            for (int i = 0; i < this.n; i++)
            {
                for (int j = 0; j < this.m; j++)
                {
                    matrix[i, j] = m[i, j];
                }
            }
            GetLUPerformed = false;
            
        }

        #region - Properties -
        public bool IsQuadratic
        {
            get { return (n == m); }
        }

        public int CountRows
        {
            get { return n; }
        }

        public int CountColumns
        {
            get { return m; }
        }

        public double this[int i, int j]
        {
            get
            {
                if (((0 <= i) && (i <= n)) && ((0 <= j) && (j <= m)))
                    return matrix[i, j];
                else throw new IndexOutOfRangeException();
            }
            set
            {
                if (((0 <= i) && (i <= n)) && ((0 <= j) && (j <= m)))
                    matrix[i, j] = value;
                else throw new IndexOutOfRangeException();
            }
        }

        #endregion

        #region - Algorithms -

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.CountColumns == m2.CountRows)
            {
                Matrix res = new Matrix(m1.CountRows, m2.CountColumns);
                int N = m1.CountColumns;
                for (int i = 0; i < m1.CountRows; i++)
                {
                    for (int j = 0; j < m2.CountColumns; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < N; k++)
                        {
                            sum += m1[i, k] * m2[k, j];
                        }
                        res[i, j] = sum;
                    }
                }
                return res;
            }
            else
            {
                throw new CountException();
            }
        }

        public static Matrix operator *(double d, Matrix m)
        {
            Matrix res = new Matrix(m);
            for (int i = 0; i < m.CountRows; i++)
            {
                for (int j = 0; j < m.CountColumns; j++)
                {
                    res[i, j] *= d;
                }
            }
            return res;
        }

        public static Matrix operator *(Matrix m, double d)
        {
            Matrix res = new Matrix(m);
            for (int i = 0; i < m.CountRows; i++)
            {
                for (int j = 0; j < m.CountColumns; j++)
                {
                    res[i, j] *= d;
                }
            }
            return res;
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if ((m1.CountColumns == m2.CountColumns) && (m1.CountRows == m2.CountRows))
            {
                Matrix res = new Matrix(m1.CountRows, m1.CountColumns);
                for (int i = 0; i < m1.CountRows; i++)
                {
                    for (int j = 0; j < m2.CountColumns; j++)
                    {
                        res[i, j] = m1[i, j] + m2[i, j];
                    }
                }
                return res;
            }
            else
            {
                throw new CountException();
            }
        }

        public static Matrix Transpose(Matrix mat)
        {
            Matrix res = new Matrix(mat.CountColumns, mat.CountRows);
            for (int i = 0; i < mat.CountRows; i++)
            {
                for (int j = 0; j < mat.CountColumns; j++)
                {
                    res[j, i] = mat[i, j];
                }
            }
            return res;
        }

        
        #region - Solving system equation -

        public double Determinant()
        {
            if (IsQuadratic)
            {
                int N = this.n;
                if (!GetLUPerformed)
                {
                    U = new Matrix(N, N);
                    L = new Matrix(N, N);

                    GetLUMatrixs();
                }

                double res = 1;
                for (int i = 0; i < N; i++)
                {
                    res *= U[i, i];
                }
                return res;
            }
            else throw new CountException("Matrix is not Quadratic!!!");
        }

        public void GetLUMatrixs()
        {
            if (IsQuadratic)
            {
                int N = this.m;
                L = new Matrix(N, N);
                U = new Matrix(N, N);
                for (int i = 0; i < N; i++)
                {
                    for (int j = i; j < N; j++)
                    {
                        double sum1 = 0;
                        for (int k = 0; k < i; k++)
                        {
                            sum1 += L[i, k] * U[k, j];
                        }
                        U[i, j] = this[i, j] - sum1;

                    }
                    for (int j = i + 1; j < N; j++)
                    {
                        double sum2 = 0;
                        for (int k = 0; k < i; k++)
                        {
                            sum2 += L[i, k] * U[k, j];
                        }
                        L[j, i] = (this[j, i] - sum2) / U[i, i];

                    }
                }
                for (int i = 0; i < N; i++)
                {
                    L[i, i] = 1;
                }
                GetLUPerformed = true;
            }
            else
            {
                GetLUPerformed = false;
                throw new CountException("Matrix is not Quadratic!!!");
            }
        }

        public Vector LUalgorithm(Vector b)
        {
            if (this.IsQuadratic)
            {
                int N = this.n;
                if (!GetLUPerformed)
                {
                    U = new Matrix(N, N);
                    L = new Matrix(N, N);

                    GetLUMatrixs();
                }

                double[] y = new double[N];
                for (int i = 0; i < N; i++)
                {
                    double sum1 = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum1 += L[i, k] * y[k];
                    }
                    y[i] = b[i] - sum1;
                }

                Vector x = new Vector(N);
                for (int i = N - 1; i >= 0; i--)
                {
                    double sum2 = 0;
                    for (int k = N - 1; k > i; k--)
                    {
                        sum2 += U[i, k] * x[k];
                    }
                    x[i] = (y[i] - sum2) / U[i, i];
                }
                return x;
            }
            else throw new CountException("Matrix is not Quadratic!!!");
        }
        
        #endregion

        #endregion

        public override string ToString()
        {
            string str_matrix = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    str_matrix += matrix[i, j].ToString("00.000") + "   ";
                }
                str_matrix += "\r\n";
            }
            return str_matrix;
        }


    }

}

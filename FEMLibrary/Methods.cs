using System;
using System.Collections.Generic;
using System.Text;
using AlgebraClasses;

namespace FEMLibrary
{
    public class SolidMechanicSolver
    {
        public static Vector Solve(Model model, double eps, bool linear, out int iter)
        {
            Matrix totalMatrix = CreateHardMatrix(model);
            //Boundary condition
            int N = model.mesh.Count;
            int count = 6 * (N + 1);
            totalMatrix[0, 0] *= 1000000000000;
            totalMatrix[1, 1] *= 1000000000000;
            totalMatrix[2, 2] *= 1000000000000;
            totalMatrix[3, 3] *= 1000000000000;
            totalMatrix[4, 4] *= 1000000000000;
            totalMatrix[5, 5] *= 1000000000000;
            totalMatrix[count - 1, count - 1] *= 100000000000;
            totalMatrix[count - 2, count - 2] *= 100000000000;
            totalMatrix[count - 3, count - 3] *= 100000000000;
            totalMatrix[count - 4, count - 4] *= 100000000000;
            totalMatrix[count - 5, count - 5] *= 100000000000;
            totalMatrix[count - 6, count - 6] *= 100000000000;
            
            Vector totalVector = CreateTotalVector(model);

            Vector resultCurrent = totalMatrix.LUalgorithm(totalVector);
            Vector resultPrevious = new Vector(count);

            iter = 0;

            if (!linear)
            {
                int k = 0;
                while ((Vector.Norm(resultCurrent - resultPrevious) > eps * Vector.Norm(resultCurrent)) && (k < 1000))
                {
                    k++;
                    resultPrevious = new Vector(resultCurrent);
                    Vector totalNonLinearVector = NonLinearPart(model, resultPrevious);
                    resultCurrent = totalMatrix.LUalgorithm(totalVector - totalNonLinearVector);
                }
                iter = k;
            }
            return resultCurrent;
        }

        #region - Total Vactor -
        public static Vector CreateTotalVector(Model model)
        {
            int N = model.mesh.Count;
            int count = 6 * (N + 1);
            Vector F = new Vector(count);
            
            for (int i = 0; i < N; i++)
            {
                Vector f = new Vector(Constants.APROXIMATION_ONE_ELEMENT);
                for (int j = 0; j < GaussianQuadrature.ORDER; j++)
                {
                    double alfa1 = (model.mesh[i].Begin * (1 - GaussianQuadrature.NODES[j]) + model.mesh[i].End * (1 + GaussianQuadrature.NODES[j])) / 2;
                    Vector FK = new Vector(Constants.APROXIMATION_ONE_ELEMENT);
                    FK[0] = model.load.F1[0](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[1] = model.load.F1[1](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[2] = model.load.F1[2](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[3] = model.load.F3[0](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[4] = model.load.F3[1](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[5] = model.load.F3[2](alfa1) * (1 - GaussianQuadrature.NODES[j]) / 2;
                    FK[6] = model.load.F1[0](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    FK[7] = model.load.F1[1](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    FK[8] = model.load.F1[2](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    FK[9] = model.load.F3[0](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    FK[10] = model.load.F3[1](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    FK[11] = model.load.F3[2](alfa1) * (1 + GaussianQuadrature.NODES[j]) / 2;
                    f += GaussianQuadrature.WEIGHT[j] * FK;
                }
                for (int k = 0; k < Constants.APROXIMATION_ONE_ELEMENT; k++)
                {
                    F[i * 6 + k] += f[k] * (model.mesh[i].End - model.mesh[i].Begin) / 2;
                }
            }
            return F;
        }
        #endregion

        #region - NonLinear Part-
        public static Vector NonLinearPart(Model model, Vector uPrevious)
        {
            //Matrix M = Matrix.Transpose(MatrixD(model.shape.K, 1 / model.shape.H)) * MatrixA(model.material.E, model.material.V, model.shape.H);
            Matrix M = MatrixA(model.material.E, model.material.V, model.shape.H) * MatrixD(model.shape.K, 1 / model.shape.H);
            int N = model.mesh.Count;
            int count = 6 * (N + 1);
            Vector totalNonLinearVector = new Vector(count);
            for (int i = 0; i < N; i++)
            {
                Vector nonLinearVector = new Vector(Constants.APROXIMATION_ONE_ELEMENT);
                for (int j = 0; j < GaussianQuadrature.ORDER; j++)
                {
                    double eta = GaussianQuadrature.NODES[j];
                    Matrix C = MatrixC(eta, model.mesh[i].Begin, model.mesh[i].End);

                    Vector q = VectorQ(model, i, uPrevious, eta);

                    nonLinearVector += GaussianQuadrature.WEIGHT[j] * q * M * C;
                }
                nonLinearVector *= ((model.mesh[i].End - model.mesh[i].Begin) / 2.0);

                //Adding nonLinearVector to totalNonLinearVector
                for (int k = 0; k < Constants.APROXIMATION_ONE_ELEMENT; k++)
                {
                    totalNonLinearVector[6 * i + k] += nonLinearVector[k];
                }
            }
            
            return totalNonLinearVector;
        }

        private static Matrix MatrixC(double eta, double begin, double end)
        {
            Matrix C = new Matrix(Constants.APROXIMATION_ONE_ELEMENT, Constants.APROXIMATION_ONE_ELEMENT);
            int m = 0;
            for (int j = 0; j < Constants.APROXIMATION_ONE_ELEMENT; j++)
            {
                if (j % 2 == 0)
                {
                    C[j, m] = (1 - eta) / 2;
                    C[j, m + 6] = (1 + eta) / 2;
                }
                else
                {
                    C[j, m] = 1 / (begin - end);
                    C[j, m + 6] = 1 / (end - begin);
                    m += 1;
                }
            }
            return C;
        }

        private static Vector VectorQ(Model model, int k, Vector uPrevious, double eta)
        {
            Vector q = new Vector(Constants.NON_ZERO_DEFORMATION_TENSOR_ELEMENT);
            Vector w = VectorW(model, k, uPrevious, eta);
            q[0] = q[3] = w * MatrixQ0(model.shape) * w;
            q[1] = q[4] = w * MatrixQ1(model.shape) * w;
            q[2] = q[5] = w * MatrixQ2(model.shape) * w;
            q[6] = q[7] = q[8] = 0;
            return q;
        }

        private static Vector VectorW(Model model, int k, Vector uPrevious, double eta)
        {
            Matrix V = MatrixV(model.shape);
            Matrix C = MatrixC(eta, model.mesh[k].Begin, model.mesh[k].End);
            Vector u = uPrevious.SubVector(6 * k, Constants.APROXIMATION_ONE_ELEMENT);
            Vector w = V * C * u; 
            return w;
        }

        private static Matrix MatrixV(Shape shape)
        {
            Matrix V = new Matrix(Constants.W_LENGTH, Constants.APROXIMATION_ONE_ELEMENT);
            double hInv = 1 / shape.H;
            V[0, 0] = -hInv + 1.5 * shape.K;
            V[0, 2] = hInv - 0.5 * shape.K;
            V[0, 4] = 4 * hInv - 2 * shape.K;
            V[1, 0] = -hInv - 0.5 * shape.K;
            V[1, 2] = hInv + 1.5 * shape.K;
            V[1, 4] = -4 * hInv - 2 * shape.K;
            V[2, 4] = 3 * shape.K;
            V[0, 7] = V[1, 9] = V[2, 11] = -1;

            return 0.5 * V;
        }

        private static Matrix MatrixQ0(Shape shape)
        {
            Matrix Q0 = new Matrix(Constants.W_LENGTH, Constants.W_LENGTH);
            double KH = shape.K * shape.H;
            Q0[0, 0] = (16 + 6 * KH + KH * KH) / 32;
            Q0[0, 1] = Q0[1, 0] = (2 * KH + KH * KH) / 32;
            Q0[0, 2] = Q0[2, 0] = (2 + KH) * (2 + KH) / 16;
            Q0[1, 1] = (-2 * KH + KH * KH) / 32;
            Q0[1, 2] = Q0[2, 1] = (-4 + KH * KH) / 16;
            Q0[2, 2] = (-4 + 2 * KH + KH * KH) / 8;
            return Q0;
        }

        private static Matrix MatrixQ1(Shape shape)
        {
            Matrix Q1 = new Matrix(Constants.W_LENGTH, Constants.W_LENGTH);
            double KH = shape.K * shape.H;
            Q1[0, 0] = (2 * KH + KH * KH) / 32;
            Q1[0, 1] = Q1[1, 0] = (-2 * KH + KH * KH) / 32;
            Q1[0, 2] = Q1[2, 0] = (-4 + KH * KH) / 16;
            Q1[1, 1] = (16 - 6 * KH + KH * KH) / 32;
            Q1[1, 2] = Q1[2, 1] = (2 - KH) * (2 - KH) / 16;
            Q1[2, 2] = (-4 - 2 * KH + KH * KH) / 8;
            return Q1;
        }

        private static Matrix MatrixQ2(Shape shape)
        {
            Matrix Q2 = new Matrix(Constants.W_LENGTH, Constants.W_LENGTH);
            double KH = shape.K * shape.H;
            Q2[0, 0] = -(2 + KH) * (2 + KH) / 32;
            Q2[0, 1] = Q2[1, 0] = (4 - KH * KH) / 32;
            Q2[0, 2] = Q2[2, 0] = (4 - 2 * KH - KH * KH) / 16;
            Q2[1, 1] = (-4 + 4 * KH - KH * KH) / 32;
            Q2[1, 2] = Q2[2, 1] = (4 + 2 * KH - KH * KH) / 16;
            Q2[2, 2] = (8 - KH * KH) / 8;
            return Q2;
        }


        #endregion

        #region - HardMatrix -
        public static Matrix CreateHardMatrix(Model model)
        {
            Matrix A = MatrixA(model.material.E, model.material.V, model.shape.H);

            double hInv = 1 / model.shape.H;

            Matrix D = MatrixD(model.shape.K, hInv);

            Matrix M = Matrix.Transpose(D) * A * D;

            int N = model.mesh.Count;
            int count = 6 * (N + 1);

            Matrix HardMatrix = new Matrix(count, count);

            for (int i = 0; i < N; i++)
            {
                Matrix localMatrix = GetLocalMatrix(M, model.mesh[i].Begin, model.mesh[i].End);
                HardMatrix = Sum(HardMatrix, localMatrix, i);
            }
            
            return HardMatrix;
        }

        private static Matrix MatrixA(double E, double v, double h)
        {
            Matrix A = new Matrix(9, 9);

            A[0, 0] = A[0, 2] = A[1, 1] = A[1, 2] = A[2, 0] = A[2, 1] = A[3, 3] = A[3, 5] = A[4, 4] = A[4, 5] = A[5, 3] = A[5, 4] = (E * h * (1 - v)) / (3 * (1 + v) * (1 - 2 * v));
            A[0, 1] = A[1, 0] = A[3, 4] = A[4, 3] = (E * h * (1 - v)) / (6 * (1 + v) * (1 - 2 * v));
            A[2, 2] = A[5, 5] = (8 * E * h * (1 - v)) / (5 * (1 + v) * (1 - 2 * v));


            A[0, 3] = A[0, 5] = A[1, 4] = A[1, 5] = A[2, 3] = A[2, 4] = A[3, 0] = A[3, 2] = A[4, 1] = A[4, 2] = A[5, 0] = A[5, 1] = (E * h * v) / (3 * (1 + v) * (1 - 2 * v));
            A[0, 4] = A[1, 3] = A[3, 1] = A[4, 0] = (E * h * v) / (6 * (1 + v) * (1 - 2 * v));
            A[2, 5] = A[5, 2] = (8 * E * h * v) / (5 * (1 + v) * (1 - 2 * v));

            A[6, 6] = A[6, 8] = A[7, 7] = A[7, 8] = A[8, 6] = A[8, 7] = (E * h) / (6 * (1 + v));
            A[6, 7] = A[7, 6] = (E * h) / (12 * (1 + v));
            A[8, 8] = (4 * E * h) / (15 * (1 + v));
            return A;
        }

        private static Matrix MatrixD(double K, double hInv)
        {
            Matrix D = new Matrix(9, 12);
            D[0, 1] = D[1, 3] = D[2, 5] = D[6, 7] = D[7, 9] = D[8, 11] = 1;
            D[0, 6] = D[1, 8] = D[2, 10] = D[8, 4] = K;
            D[5, 10] = 2 * K;

            D[3, 6] = -hInv + K / 2;
            D[3, 8] = D[6, 2] = D[7, 2] = hInv - K / 2;
            D[3, 10] = D[6, 4] = 4 * hInv - 2 * K;

            D[4, 6] = D[6, 0] = D[7, 0] = -hInv - K / 2;
            D[4, 8] = hInv + K / 2;
            D[4, 10] = D[7, 4] = -4 * hInv - 2 * K;
            return D;
        }

        private static Matrix GetLocalMatrix(Matrix M, double begin, double end)
        {
            Matrix localMatrix = new Matrix(Constants.APROXIMATION_ONE_ELEMENT, Constants.APROXIMATION_ONE_ELEMENT);
            for (int i = 0; i < GaussianQuadrature.ORDER; i++)
            {
                Matrix C = new Matrix(Constants.APROXIMATION_ONE_ELEMENT, Constants.APROXIMATION_ONE_ELEMENT);

                double eta = GaussianQuadrature.NODES[i];
                int m = 0;
                for (int j = 0; j < Constants.APROXIMATION_ONE_ELEMENT; j++)
                {
                    if (j % 2 == 0)
                    {
                        C[j, m] = (1 - eta) / 2;
                        C[j, m + 6] = (1 + eta) / 2;
                    }
                    else
                    {
                        C[j, m] = 1 / (begin - end);
                        C[j, m + 6] = 1 / (end - begin);
                        m += 1;
                    }
                }

                Matrix t = Matrix.Transpose(C) * M * C;

                localMatrix += GaussianQuadrature.WEIGHT[i] * t;
            }
            localMatrix *= ((end - begin) / 2.0);

            return localMatrix;
        }

        private static Matrix Sum(Matrix MMatrix, Matrix NMatrix, int m)
        {
            for (int i = 0; i < NMatrix.CountRows; i++)
            {
                for (int j = 0; j < NMatrix.CountColumns; j++)
                {
                    MMatrix[6 * m + i, 6 * m + j] += NMatrix[i, j];
                }
            }
            return MMatrix;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AlgebraClasses;
using FEMLibrary;


namespace ProgramFEM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static double load;
        static double sigma;
        static Model model;

        private double CountLoad(double sigma)
        {
            double res = 0;
            double N0 = sigma * model.shape.H;
            double Lambda = model.material.E * model.shape.H * 14 / (30 * (1 + model.material.V));
            double alfa = ((model.material.V + 1) * model.material.V * model.material.V) / (1 - model.material.V - 2 * model.material.V * model.material.V);
            double eps = model.shape.H / model.shape.L;
            double lambda_2 = (12 * N0 * (1 - model.material.V * model.material.V)) / (model.shape.H * model.shape.H * model.shape.H * model.material.E * (1 + alfa));
            double k_2 = Lambda / (Lambda + N0);
            double lambda1 = Math.Sqrt(k_2 * lambda_2);

            double delta = 1.0 / 3.0
                + (k_2 * k_2 * (Math.Sinh(lambda1 * model.shape.L / 2)*Math.Cosh(lambda1 * model.shape.L / 2) - (lambda1 * model.shape.L / 2)))
                    / ((Math.Sinh(lambda1 * model.shape.L / 2) * Math.Sinh(lambda1 * model.shape.L / 2) * lambda1 * model.shape.L))
                - 2 * (k_2 * (lambda1 * model.shape.L / 2 * Math.Cosh(lambda1 * model.shape.L / 2) - Math.Sinh(lambda1 * model.shape.L / 2))) 
                    / (Math.Sinh(lambda1 * model.shape.L / 2) * lambda1 * (model.shape.L / 4)* lambda1 * model.shape.L);

            res = 2 * eps * sigma * Math.Sqrt((2 * (1 - model.material.V * model.material.V) * sigma) / ((1 + alfa) * delta * model.material.E));
            return res;
        }

        private double W1(double x)
        {
            double res = 0;
            
            return res;
        }

        private double W2(double x)
        {
            double res = 0;
            double N0 = sigma * model.shape.H;
            double Lambda = model.material.E * model.shape.H * 14 / (30 * (1 + model.material.V));
            //double alfa = (model.material.V * model.material.V + model.material.V * model.material.V * model.material.V) / (1 - model.material.V - 2 * model.material.V * model.material.V);
            double alfa = ((model.material.V + 1) * model.material.V * model.material.V) / (1 - model.material.V - 2 * model.material.V * model.material.V);
            double eps = model.shape.H / model.shape.L;
            double lambda_2 = (12 * N0 * (1 - model.material.V * model.material.V)) / (model.shape.H * model.shape.H * model.shape.H * model.material.E * (1 + alfa));
            double k_2 = Lambda / (Lambda + N0);
            double lambda1 = Math.Sqrt(k_2 * lambda_2);

            
            double l = model.shape.L/2;
            double ksi = (2 * x - model.shape.L) / model.shape.L;

            res = (-model.shape.H / (4 * eps * eps)) * (-load / sigma) 
                * ((1 - ksi * ksi) / 2 
                    - (k_2 * (Math.Cosh(l * lambda1) - Math.Cosh((x-l) * lambda1))) / (l * lambda1*Math.Sinh(l * lambda1)));
            return res;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            tChart1.Series.Clear();
            double E = double.Parse(Etxt.Text);//модуль Юнга
            double v = double.Parse(Vtxt.Text);//коефіцієнт Пуасона
            double h = double.Parse(Htxt.Text);//товщина
            int N = int.Parse(Ntxt.Text);//к-сть розбиттів
            double l = double.Parse(Ltxt.Text);//довжина
            double K = double.Parse(Ktxt.Text);//кривизна
            double eps = double.Parse(EPStxt.Text);//точність
            sigma = double.Parse(Ftxt.Text);
            int count = 6 * (N + 1);
            model = new Model(E, v, h, l, K, N);

            load = sigma;//(-1)*CountLoad(sigma);
            Force F = new Force(load, h);

            model.load.F1[0] = F.F10;
            model.load.F1[1] = F.F11;
            model.load.F1[2] = F.F12;
            model.load.F3[0] = F.F30;
            model.load.F3[1] = F.F31;
            model.load.F3[2] = F.F32;
            
            bool linear = false;
            if (checkBox1.Checked)
                linear = true;


            int iter = 0;
            Vector uDiscrete = SolidMechanicSolver.Solve(model, eps, linear, out iter);
            
            txtNorm.Text += "Ітерацій: " + iter + "\r\nРозбиття: " + N + "\r\n";
           
            ApproximationResult result = new ApproximationResult(model.shape, uDiscrete);
            AnaliticalMarchuk analitical = new AnaliticalMarchuk(l, h, E, v, load);
            //AnaliticalMukha analitical = new AnaliticalMukha(l, h, E, v, load);

            Steema.TeeChart.Styles.Line lineU1 = new Steema.TeeChart.Styles.Line();
            lineU1.Title = "U1";
            lineU1.Color = Color.Red;
            tChart1.Series.Add(lineU1);

            Steema.TeeChart.Styles.Line lineU3 = new Steema.TeeChart.Styles.Line();
            lineU3.Title = "U3";
            lineU3.Color = Color.Green;
            tChart1.Series.Add(lineU3);
            /*Steema.TeeChart.Styles.Line lineU1a = new Steema.TeeChart.Styles.Line();
            lineU1a.Title = "U1 аналітичне";
            lineU1a.Color = Color.Aqua;
            tChart1.Series.Add(lineU1a);*/
            Steema.TeeChart.Styles.Line lineU3a = new Steema.TeeChart.Styles.Line();
            lineU3a.Title = "U3 аналітичне";
            lineU3a.Color = Color.Blue;
            tChart1.Series.Add(lineU3a);
            
            Vector alfa = new Vector(Constants.DIMENSION_COUNT);
            alfa[0] = model.mesh[0].Begin;
            alfa[1] = 0;
            alfa[2] = 0;
            lineU1.Add(model.mesh[0].Begin, result.U(alfa)[0]);
            lineU3.Add(model.mesh[0].Begin, result.U(alfa)[2]);
            //lineU1a.Add(model.mesh[0].Begin, analitical.U(alfa)[0]);
            lineU3a.Add(model.mesh[0].Begin, -0.01);//W2(alfa[0])); //analitical.U(alfa)[2]); //W2(alfa[0]));
            for (int i = 0; i < N ; i++)
            {
                double xCur = model.mesh[i].End;
                alfa = new Vector(Constants.DIMENSION_COUNT);
                alfa[0] = xCur;
                alfa[1] = 0;
                alfa[2] = 0;
                if (xCur > model.shape.L)
                {
                    xCur = model.shape.L;
                    alfa[0] = xCur;
                }
                Vector resFEM = result.U(alfa);
                //Vector resAnalit = W2(xCur);
                lineU1.Add(xCur, resFEM[0]);
                lineU3.Add(xCur, resFEM[2]);
                //lineU1a.Add(xCur, analitical.U(alfa)[0]);
                //lineU3a.Add(xCur, analitical.U(alfa)[2]); 
                //lineU1a.Add(xCur, resAnalit[0]);
                //lineU3a.Add(xCur, W2(xCur));
            }

            
            dataGridView1.RowCount = 1;
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "alfa1";
            dataGridView1.Columns[1].Name = "U1";
            dataGridView1.Columns[2].Name = "U1_analitical";
            dataGridView1.Columns[3].Name = "U3";
            dataGridView1.Columns[4].Name = "U3_analitical";

            

            double alfaCur = model.mesh[0].Begin;
            alfa[0] = alfaCur;
            alfa[1] = 0;
            alfa[2] = 0;
            List<string> row = new List<string>();
            row.Add(alfaCur.ToString());
            row.Add(result.U(alfa)[0].ToString("0.0000000000"));
            row.Add(analitical.U(alfa)[0].ToString("0.0000000000"));
            row.Add(result.U(alfa)[2].ToString("0.0000000000"));
            row.Add(analitical.U(alfa)[2].ToString("0.0000000000"));
            //row.Add(W2(alfa[0]).ToString("0.0000000000"));
            dataGridView1.Rows.Add(row.ToArray());
            
            for (int i = 0; i < N; i++)
            {
                alfaCur = model.mesh[i].End;
                alfa = new Vector(Constants.DIMENSION_COUNT);
                alfa[0] = alfaCur;
                alfa[1] = 0;
                alfa[2] = 0;
                if (alfaCur > model.shape.L)
                {
                    alfaCur = model.shape.L;
                    alfa[0] = alfaCur;
                }
                row = new List<string>();
                row.Add(alfaCur.ToString());
                row.Add(result.U(alfa)[0].ToString("0.0000000000"));
                row.Add(analitical.U(alfa)[0].ToString("0.0000000000"));
                row.Add(result.U(alfa)[2].ToString("0.0000000000"));
                //row.Add(analitical.U(alfa)[2].ToString("0.0000000000"));
                row.Add(W2(alfa[0]).ToString("0.0000000000"));
                dataGridView1.Rows.Add(row.ToArray());
            }
               
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EquilibriumPath frmPath = new EquilibriumPath(model, 0.001);
            frmPath.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Norma frmNorma = new Norma(model, 0.001, load);
            frmNorma.Show();
        }
     
    }
}
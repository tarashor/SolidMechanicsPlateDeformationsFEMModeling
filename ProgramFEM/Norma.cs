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
    public partial class Norma : Form
    {
        public Norma(Model model, double eps, double load)
        {
            InitializeComponent();

            AnaliticalMukha analitical = new AnaliticalMukha(model.shape.L, model.shape.H, model.material.E, model.material.V, load * model.shape.H);

            Steema.TeeChart.Styles.Line lineAnalit = new Steema.TeeChart.Styles.Line();
            lineAnalit.Title = "Analytical";
            lineAnalit.Color = Color.Red;
            tChart1.Series.Add(lineAnalit);

            Steema.TeeChart.Styles.Line line = new Steema.TeeChart.Styles.Line();
            line.Title = "Nonlinear FEM";
            line.Color = Color.Green;
            tChart1.Series.Add(line);

            Vector alfa = new Vector(3);
            alfa[0] = model.shape.L;
            alfa[1] = alfa[2] = 0;
            int iter = 0;

            dataGridView1.RowCount = 1;
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "N";
            dataGridView1.Columns[1].Name = "(Au,u)";


            

            for (int i = 20; i <= 200; i += 20)
            {
                model.mesh = new Mesh(model.shape, i);
                Vector uDiscrete = SolidMechanicSolver.Solve(model, eps, false, out iter);
                //Matrix K = SolidMechanicSolver.CreateHardMatrix(model);
                //Vector KNonLinear = SolidMechanicSolver.NonLinearPart(model, uDiscrete);
                Vector FNonlinear = SolidMechanicSolver.CreateTotalVector(model);

                double norm = FNonlinear * uDiscrete;//uDiscrete * K * uDiscrete + 2 * uDiscrete * KNonLinear;



                line.Add(i, norm);

                List<string> row = new List<string>();
                row.Add(i.ToString());
                row.Add(norm.ToString("0.000000000000"));
                dataGridView1.Rows.Add(row.ToArray());

                /*
                ApproximationResult result = new ApproximationResult(model.shape, uDiscrete);

                lineAnalit.Add(i, Vector.Norm(analitical.U(alfa)));
                line.Add(i, Vector.Norm(result.U(alfa)));*/
            }


            
            
            /*Steema.TeeChart.Styles.Line lineU1a = new Steema.TeeChart.Styles.Line();
            lineU1a.Title = "U1 аналітичне";
            tChart1.Series.Add(lineU1a);
            Steema.TeeChart.Styles.Line lineU3a = new Steema.TeeChart.Styles.Line();
            lineU3a.Title = "U3 аналітичне";
            tChart1.Series.Add(lineU3a);

            Vector alfa = new Vector(Constants.DIMENSION_COUNT);
            alfa[0] = model.mesh[0].Begin;
            alfa[1] = 0;
            alfa[2] = 0;
            lineU1.Add(model.mesh[0].Begin, result.U(alfa)[0]);
            lineU3.Add(model.mesh[0].Begin, result.U(alfa)[2]);
            lineU1a.Add(model.mesh[0].Begin, analitical.U(alfa)[0]);
            lineU3a.Add(model.mesh[0].Begin, analitical.U(alfa)[2]);
            for (int i = 0; i < N; i++)
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
                Vector resAnalit = analitical.U(alfa);
                lineU1.Add(xCur, resFEM[0]);
                lineU3.Add(xCur, resFEM[2]);
                lineU1a.Add(xCur, resAnalit[0]);
                lineU3a.Add(xCur, resAnalit[2]);
            }
*/
        }
    }
}

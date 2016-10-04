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
    public partial class EquilibriumPath : Form
    {
        public EquilibriumPath(Model model, double eps)
        {
            InitializeComponent();

            

            Steema.TeeChart.Styles.Line lineNL = new Steema.TeeChart.Styles.Line();
            lineNL.Title = "Нелінійна задача";
            lineNL.Color = Color.Red;
            tChart1.Series.Add(lineNL);

            Steema.TeeChart.Styles.Line lineL = new Steema.TeeChart.Styles.Line();
            lineL.Title = "Лінійна задача";
            lineL.Color = Color.Blue;
            tChart1.Series.Add(lineL);

            Vector alfa = new Vector(3);
            alfa[0] = model.shape.L;
            alfa[1] = alfa[2] = 0;
            int iter = 0;

            for (double load = 0; load < 1000; load+=20)
            {
                Force F = new Force((-1)*load, model.shape.H);

                model.load.F1[0] = F.F10;
                model.load.F1[1] = F.F11;
                model.load.F1[2] = F.F12;
                model.load.F3[0] = F.F30;
                model.load.F3[1] = F.F31;
                model.load.F3[2] = F.F32;
                Vector uDiscreteL = SolidMechanicSolver.Solve(model, eps, true, out iter);
                Vector uDiscreteNL = SolidMechanicSolver.Solve(model, eps, false, out iter);
                
                if (iter < 1000)
                {
                    ApproximationResult resultNL = new ApproximationResult(model.shape, uDiscreteNL);
                    lineNL.Add(Vector.Norm(resultNL.U(alfa)), load);
                    ApproximationResult resultL = new ApproximationResult(model.shape, uDiscreteL);
                    lineL.Add(Vector.Norm(resultL.U(alfa)), load);
                }
                else break;
            }
            /*
            */
            
            /*
            for (int i = 20; i <= 200; i += 20)
            {
                model.mesh = new Mesh(model.shape, i);
                

                lineAnalit.Add(i, Vector.Norm(analitical.U(alfa)));
                line.Add(i, Vector.Norm(result.U(alfa)));
            }
            */
        }
    }
}

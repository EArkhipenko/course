using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace course
{
    class DFModel
    {
        Vector<double> theta_ex;
        Vector<double> theta;
        Vector<double> y;
        Matrix<double> X;
        int k1, k2;
        public Tuple<double, double> Fa, Fb;
        public List<Tuple<string, double, double>> levels;

        public DFModel(int k1, int k2, double[] tet, double[][] X, double ro)
        {
            this.k1 = k1;
            this.k2 = k2;
            this.theta_ex = DenseVector.OfArray(tet);
            this.X = DenseMatrix.OfRowArrays(X);
            generate(ro);
        }

        public double[] tet()
        {
            return theta.ToArray();
        }

        private void generate(double ro)
        {
            var n = X.RowCount;
            var u = X * theta_ex;
            var u_mean = u - Statistics.Mean(u);
            var w2 = (u_mean * u_mean) / (n - 1);
            Vector<double> err = DenseVector.OfArray(Generate.Normal(n, 0, ro * w2));
            y = u + err;
        }

        public void estimate()
        {
            est_theta();
            est_factors();
            levels = new List<Tuple<string, double, double>>();
            var c = new double[theta.Count];
            // Уровни по первому фактору
            for (int i = 0; i < k1; ++i)
            {
                for (int j = i+1; j < k1; ++j)
                {
                    for (int k = 0; k < c.Length; k++)
                    {
                        c[k] = 0;
                        if (k == i + 1) { c[k] = 1; }
                        if (k == j + 1) { c[k] = -1; }
                    }
                    var l = est_levels(c, 0);
                    levels.Add(Tuple.Create(String.Format("Factor 1; levels {0}, {1}", i + 1, j + 1), l.Item1, l.Item2));
                }
            }

            // Уровни по второму фактору
            for (int i = 0; i < k2; ++i)
            {
                for (int j = i + 1; j < k2; ++j)
                {
                    for (int k = 0; k < c.Length; k++)
                    {
                        c[k] = 0;
                        if (k == i + 1 + k1) { c[k] = 1; }
                        if (k == j + 1 + k1) { c[k] = -1; }
                    }
                    var l = est_levels(c, 0);
                    levels.Add(Tuple.Create(String.Format("Factor 2; levels {0}, {1}", i + 1, j + 1), l.Item1, l.Item2));
                }
            }
        }

        private Matrix<double> get_u()
        {
            var l1 = new List<double>();
            l1.Add(0);
            l1.AddRange(Enumerable.Repeat(1.0, k1));
            l1.AddRange(Enumerable.Repeat(0.0, k2));
            var l2 = new List<double>();
            l2.Add(0);
            l2.AddRange(Enumerable.Repeat(0.0, k1));
            l2.AddRange(Enumerable.Repeat(1.0, k2));
            return DenseMatrix.OfRowArrays(new double[][] { l1.ToArray(), l2.ToArray() });
        }

        private void est_theta()
        {
            var U = get_u();
            var u = U * theta_ex;
            var r = (X.Transpose() * y + U.Transpose() * u);
            var l = (X.Transpose() * X + U.Transpose() * U);
            theta = l.Inverse() * r;
        }


        private double F(double a, double d1, double d2)
        {
            var fisher = new FisherSnedecor(d1, d2);
            var arr = new double[10000];
            fisher.Samples(arr);
            return Statistics.Quantile(arr, a);
        }

        private void est_factors()
        {
            double ssa = 0;
            for (int i = 0; i < k1; ++i)
            {
                ssa += Math.Pow(theta[i + 1] - theta[0], 2);
            }
            ssa *= k2;
            double ssb = 0;
            for (int i = 0; i < k2; ++i)
            {
                ssb += Math.Pow(theta[i + 1 + k1] - theta[0], 2);
            }
            ssb *= k1;
            
            var y_est = X * theta;
            var y_err = y - y_est;
            double sse = y_err * y_err;
            int N = X.RowCount;
            int r = X.Rank();
            Fa = Tuple.Create((k2 - 1) * ssa / sse, F(0.95, k1 - 1, N - r));
            Fb = Tuple.Create((k1 - 1) * ssb / sse, F(0.95, k2 - 1, N - r));
        }

        private Tuple<double, double> est_levels(double [] c, double m)
        {
            var C = DenseMatrix.OfColumnArrays(new double[][] { c });
            var U = get_u();
            var Ct = (C.Transpose() * theta - m)[0];
            // var xx = (X.Transpose() * X + U.Transpose() * U).Inverse();
            var XX = X.Transpose().Append(U.Transpose()).Transpose();
            var xx = (XX.Transpose() * XX).Inverse();
            var cxxc = C.Transpose() * xx * C;  
            double f = (Ct * cxxc.Inverse()[0,0] * Ct);
            int N = X.RowCount;
            int r = X.Rank();
            var y_est = X * theta;
            var y_err = y - y_est;
            double sse = y_err * y_err;
            return Tuple.Create(f * (N - r) / sse, F(0.95, 1, N - r));
        }
    }
}

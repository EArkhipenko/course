using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace course
{
    class DFModel
    {
        Vector<double> theta_ex;
        Vector<double> theta;
        Vector<double> y;
        Matrix<double> X;
        int k1, k2;
            
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
        }

        private void est_theta()
        {
            var l1 = new List<double>();
            l1.Add(0);
            l1.AddRange(Enumerable.Repeat(1.0, k1));
            l1.AddRange(Enumerable.Repeat(0.0, k2));
            var l2 = new List<double>();
            l2.Add(0);
            l2.AddRange(Enumerable.Repeat(0.0, k1));
            l2.AddRange(Enumerable.Repeat(1.0, k2));
            Matrix<double> U = DenseMatrix.OfRowArrays(new double[][]{ l1.ToArray(), l2.ToArray() });
            var u = U * theta_ex;
            var r = (X.Transpose() * y + U.Transpose() * u);
            var l = (X.Transpose() * X + U.Transpose() * U);
            theta = l.Inverse() * r;
        }
    }
}

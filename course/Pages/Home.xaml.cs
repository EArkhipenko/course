using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace course.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private DFModel model;
        private int k1 = 2;
        private int k2 = 2;
        private double ro = 0.1;

        public string K1
        {
            get { return k1.ToString(); }
            set
            {
                k1 = Convert.ToInt32(value);
                if (k1 < 2)
                {
                    k1 = 2;
                }
                if (k1 > 5)
                {
                    k1 = 5;
                }
            }
        }
        public string K2 {
            get { return k2.ToString(); }
            set
            {
                k2 = Convert.ToInt32(value);
                if (k2 < 2)
                {
                    k2 = 2;
                }
                if (k2 > 5)
                {
                    k2 = 5;
                }
            }
        }
        public Home()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() != true) return;
            model.estimate();
            using (var fout = new StreamWriter(saveFileDialog.FileName))
            {
                fout.WriteLine(String.Join(" ", model.tet().Select(v => v.ToString())));
                fout.WriteLine(String.Format("Fa = {0}, F = {1}, {2}", model.Fa.Item1, model.Fa.Item2, model.Fa.Item1 < model.Fa.Item2 ? "принимается": "отвергается"));
                fout.WriteLine(String.Format("Fb = {0}, F = {1}, {2}", model.Fb.Item1, model.Fb.Item2, model.Fb.Item1 < model.Fb.Item2 ? "принимается" : "отвергается"));
                foreach (var l in model.levels)
                {
                    fout.WriteLine(String.Format("{0}: f = {1}, F = {2}, {3}", l.Item1, l.Item2, l.Item3, l.Item2 < l.Item3 ? "принимается" : "отвергается"));
                }
            }

        }

        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true) return;
            using (StreamReader fin = new StreamReader(openFileDialog.FileName))
            {
                var tet = fin.ReadLine().Split().ToList().ConvertAll<double>(v => Convert.ToDouble(v)).ToArray();
                int n = Convert.ToInt32(fin.ReadLine());
                double[][] X = new double[n][];
                for (int i = 0; i < n; ++i)
                {
                    X[i] = fin.ReadLine().Split().ToList().ConvertAll<double>(v => Convert.ToDouble(v)).ToArray();
                }
                model = new DFModel(k1, k2, tet, X, ro);
            }
            
        }
    }
}

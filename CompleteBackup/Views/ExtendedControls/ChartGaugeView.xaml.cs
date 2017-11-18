using CompleteBackup.ViewModels.ExtendedControls;
using System;
using System.Collections.Generic;
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

namespace CompleteBackup.Views.ExtendedControls
{
    /// <summary>
    /// Interaction logic for ChartGaugeView.xaml
    /// </summary>
    public partial class ChartGaugeView : UserControl
    {
        public ChartGaugeView()
        {
            InitializeComponent();

            m_ViewModel = new ChartGaugeViewModel();

            this.DataContext = m_ViewModel;
        }


        public ChartGaugeView(SolidColorBrush section1, SolidColorBrush section2, SolidColorBrush section3)
        {
            InitializeComponent();

            m_ViewModel = new ChartGaugeViewModel(section1, section2, section3);

            this.DataContext = m_ViewModel;
        }


        public string PumpNumber { get { return m_ViewModel.PumpNumber; } set { m_ViewModel.PumpNumber = value; } }
        public float GaugeValue { get { return m_ViewModel.GaugeValue; } set { m_ViewModel.GaugeValue = (value > 1) ? 1 : value; } }

        private ChartGaugeViewModel m_ViewModel;



        private void DrawGraph()
        {
            string str = string.Format("{0:00%}", m_ViewModel.GaugeValue);
            m_ViewModel.Text = $"{m_ViewModel.PumpNumber} {str}";
            m_ViewModel.Utilization = str;

            int iRad = m_ViewModel.RadiusX2;

            var n1 = m_ViewModel.GaugeValue * 180 + 180;
            var n2 = m_ViewModel.GaugeValue * 180;
            float EndPointX1 = m_ViewModel.DotSize * (float)System.Math.Cos((m_ViewModel.GaugeValue * 180 + 180) * Math.PI / 180);
            float EndPointY1 = m_ViewModel.DotSize * (float)System.Math.Sin((m_ViewModel.GaugeValue * 180) * Math.PI / 180);

            float EndPointX2 = (iRad - 1) * (float)System.Math.Cos((m_ViewModel.GaugeValue * 180 + 180) * Math.PI / 180);
            float EndPointY2 = (iRad - 1) * (float)System.Math.Sin((m_ViewModel.GaugeValue * 180) * Math.PI / 180);


            m_ViewModel.GaugeX1 = iRad + (int)EndPointX1;
            m_ViewModel.GaugeX2 = iRad + (int)EndPointX2;
            m_ViewModel.GaugeY1 = iRad - (int)EndPointY1 - 1;
            m_ViewModel.GaugeY2 = iRad - (int)EndPointY2;

            m_ViewModel.GaugeValueSegment = new PointCollection() {
                new Point(0, 2 * m_ViewModel.Radius),
                new Point(2 * m_ViewModel.Radius, 2 * m_ViewModel.Radius),
                new Point(m_ViewModel.GaugeX2 + 1, m_ViewModel.GaugeY2 ),
                new Point(m_ViewModel.GaugeX2 + 1, 0 ),
            };
        }

        private void GaugeValueValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            DrawGraph();
        }
    }
}

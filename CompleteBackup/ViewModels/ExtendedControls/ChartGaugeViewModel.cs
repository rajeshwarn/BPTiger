using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CompleteBackup.ViewModels.ExtendedControls
{
    public class ChartGaugeViewModel : ObservableObject
    {
        public ChartGaugeViewModel()
        {
            _Section1Brush = Brushes.LightGray;
            _Section2Brush = Brushes.Green;
            _Section3Brush = Brushes.Red;
        }
        public ChartGaugeViewModel(SolidColorBrush section1, SolidColorBrush section2, SolidColorBrush section3)
        {
            _Section1Brush = section1;
            _Section2Brush = section2;
            _Section3Brush = section3;
        }


        private string _Text;
        public string Text { get { return _Text; } set { _Text = value; OnPropertyChanged("Text"); } }

        private string _Utilization;
        public string Utilization { get { return _Utilization; } set { _Utilization = value; OnPropertyChanged("Utilization"); } }



        public Brush MeterBrush { get; set; } = Brushes.DarkRed;
        private Brush _FillBrush = Brushes.LightGray;
        public Brush FillBrush { get { return _FillBrush; } set { _FillBrush = value; OnPropertyChanged(); } }
        public Brush BorderBrush { get; set; } = Brushes.Gray;
        public Brush DotBrush { get; set; } = Brushes.DarkGray;


        private SolidColorBrush _Section1Brush { get; set; }
        public SolidColorBrush Section1Brush { get { return _Section1Brush; } set { _Section1Brush = value; OnPropertyChanged(); } }
        private SolidColorBrush _Section2Brush { get; set; }
        public SolidColorBrush Section2Brush { get { return _Section2Brush; } set { _Section2Brush = value; OnPropertyChanged(); } }
        private SolidColorBrush _Section3Brush { get; set; }
        public SolidColorBrush Section3Brush { get { return _Section3Brush; } set { _Section3Brush = value; OnPropertyChanged(); } }


        private int m_GaugeSize = 10;
        private int m_DotSize = 6;
        public int GaugeSize { get { return m_GaugeSize; } set { m_GaugeSize = value; OnPropertyChanged(); } }

        public uint PumpNumber { get; set; }

        public float _GaugeValue { get; set; }
        public float GaugeValue
        {
            get { return _GaugeValue; }
            set
            {
                var newBrush = Section1Brush;
                if (value > 0.8)
                {
                    newBrush = Section3Brush;
                }
                else if (value > 0.3)
                {
                    newBrush = Section2Brush;
                }

                if (_GaugeValue != value)
                {
                    FillBrush = newBrush;
                }

                _GaugeValue = value;
                OnPropertyChanged();
                OnPropertyChanged("GaugeValueSegment");
            }
        }




        public int DotSize { get { return m_DotSize; } set { } }
        public int DotSizeX2 { get { return 2 * m_DotSize; } set { } }
        public int Radius { get { return m_GaugeSize; } set { } }
        public int RadiusX2 { get { return 2 * m_GaugeSize; } set { } }
        public int RadiusX4 { get { return 4 * m_GaugeSize; } set { } }

        public Rect ClipRect { get { return new Rect(0, 0, 4 * m_GaugeSize, 2 * m_GaugeSize); } set { } }
        public Rect DotClipRect { get { return new Rect(0, 0, 2 * m_DotSize, m_DotSize); } set { } }


        public int GaugeX1L { get { return GaugeX1 - 1; } set { } }
        public int GaugeX1R { get { return GaugeX1 + 1; } set { } }

        private int _GaugeX1;
        public int GaugeX1 { get { return _GaugeX1; } set { _GaugeX1 = value; OnPropertyChanged("GaugeX1"); OnPropertyChanged("GaugeX1L"); OnPropertyChanged("GaugeX1R"); } }

        private int _GaugeX2;
        public int GaugeX2 { get { return _GaugeX2; } set { _GaugeX2 = value; OnPropertyChanged("GaugeX2"); } }

        private int _GaugeY1;
        public int GaugeY1 { get { return _GaugeY1; } set { _GaugeY1 = value; OnPropertyChanged("GaugeY1"); } }

        private int _GaugeY2;
        public int GaugeY2 { get { return _GaugeY2; } set { _GaugeY2 = value; OnPropertyChanged("GaugeY2"); } }


        private PointCollection _GaugeValueSegment;// = new PointCollection() { new Point(), new Point(), new Point(), new Point()};
        public PointCollection GaugeValueSegment { get { return _GaugeValueSegment; } set { _GaugeValueSegment = value; OnPropertyChanged(); } }

    }

}

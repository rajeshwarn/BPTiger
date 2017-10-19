using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.ViewModels.MainWindow
{
    class MainWindowStatusBarViewModel : ObservableObject
    {
        private int _ProgressBarHeight = 15;
        public int ProgressBarHeight { get { return _ProgressBarHeight; } set { _ProgressBarHeight = value; OnPropertyChanged("ProgressBarHeight"); } }

        private bool _IsIndeterminate1 = false;
        public bool IsIndeterminate1 { get { return _IsIndeterminate1; } set { _IsIndeterminate1 = value; OnPropertyChanged("IsIndeterminate1"); } }
        private bool _IsIndeterminate2 = false;
        public bool IsIndeterminate2 { get { return _IsIndeterminate2; } set { _IsIndeterminate2 = value; OnPropertyChanged("IsIndeterminate2"); } }
        private bool _IsIndeterminate3 = false;
        public bool IsIndeterminate3 { get { return _IsIndeterminate3; } set { _IsIndeterminate3 = value; OnPropertyChanged("IsIndeterminate3"); } }


        private Visibility _IsCancelEnabled = Visibility.Collapsed;
        public Visibility IsCancelEnabled { get { return _IsCancelEnabled; } set { _IsCancelEnabled = value; OnPropertyChanged("IsCancelEnabled"); } }

        private Visibility _ProgressBar1Visibility = Visibility.Collapsed;
        public Visibility ProgressBar1Visibility { get { return _ProgressBar1Visibility; } set { _ProgressBar1Visibility = value; OnPropertyChanged("ProgressBar1Visibility"); } }

        private Visibility _ProgressBar2Visibility = Visibility.Collapsed;
        public Visibility ProgressBar2Visibility { get { return _ProgressBar2Visibility; } set { _ProgressBar2Visibility = value; OnPropertyChanged("ProgressBar2Visibility"); } }

        private Visibility _ProgressBar3Visibility = Visibility.Collapsed;
        public Visibility ProgressBar3Visibility { get { return _ProgressBar3Visibility; } set { _ProgressBar3Visibility = value; OnPropertyChanged("ProgressBar3Visibility"); } }


        private long _ProgressBar1Maximum = 0;
        public long ProgressBar1Maximum { get { return _ProgressBar1Maximum; } set { _ProgressBar1Maximum = value; OnPropertyChanged("ProgressBar1Maximum"); } }

        private long _ProgressBar2Maximum = 0;
        public long ProgressBar2Maximum { get { return _ProgressBar2Maximum; } set { _ProgressBar2Maximum = value; OnPropertyChanged("ProgressBar2Maximum"); } }

        private long _ProgressBar3Maximum = 0;
        public long ProgressBar3Maximum { get { return _ProgressBar3Maximum; } set { _ProgressBar3Maximum = value; OnPropertyChanged("ProgressBar3Maximum"); } }

        private long _ProgressBar1Value = 0;
        public long ProgressBar1Value { get { return _ProgressBar1Value; } set { _ProgressBar1Value = value; OnPropertyChanged("ProgressBar1Value"); } }

        private long _ProgressBar2Value = 0;
        public long ProgressBar2Value { get { return _ProgressBar2Value; } set { _ProgressBar2Value = value; OnPropertyChanged("ProgressBar2Value"); } }

        private long _ProgressBar3Value = 0;
        public long ProgressBar3Value { get { return _ProgressBar3Value; } set { _ProgressBar3Value = value; OnPropertyChanged("ProgressBar3Value"); } }

        private string _ProgressBarText = string.Empty;
        public string ProgressBarText { get { return _ProgressBarText; } set { _ProgressBarText = value; OnPropertyChanged("ProgressBarText"); } }

        private string _StatusBarText = string.Empty;
        public string StatusBarText { get { return _StatusBarText; } set { _StatusBarText = value; OnPropertyChanged("StatusBarText"); } }


//        public PersistentServer Server { get { return PersistentServer.Instance; } private set { } }
//        public OnlineJagClient JagClient { get { return OnlineJagClient.Instance; } private set { } }
//        public ICRGenericProtocolInterface ICRGenericProtocol { get { return ICRGenericProtocolInterface.Instance; } private set { } }

        private void SetProgress(int index, long progress)
        {
            bool bIndeterminate = !(progress > 0);

            switch (index)
            {
                case 0:
                    IsIndeterminate1 = bIndeterminate;
                    ProgressBar1Value = progress;
                    break;
                case 1:
                    IsIndeterminate2 = bIndeterminate;
                    ProgressBar2Value = progress;
                    break;
                case 2:
                    IsIndeterminate3 = bIndeterminate;
                    ProgressBar3Value = progress;
                    break;
            }
        }

        private void SetIndeterminate(int index, bool bIndeterminate)
        {
            switch (index)
            {
                case 0:
                    IsIndeterminate1 = bIndeterminate;
                    break;
                case 1:
                    IsIndeterminate2 = bIndeterminate;
                    break;
                case 2:
                    IsIndeterminate3 = bIndeterminate;
                    break;
            }
        }

        private void SetRange(int index, long range)
        {
            switch (index)
            {
                case 0:
                    ProgressBar1Maximum = range;
                    break;
                case 1:
                    ProgressBar2Maximum = range;
                    break;
                case 2:
                    ProgressBar3Maximum = range;
                    break;
            }
        }

        private void UpdateProgressUI()
        {
            if (_ProgressList.Count >= 3)
            {
                ProgressBarHeight = 4;

                ProgressBar1Visibility = Visibility.Visible;
                ProgressBar2Visibility = Visibility.Visible;
                ProgressBar3Visibility = Visibility.Visible;
            }
            else if (_ProgressList.Count >= 2)
            {
                ProgressBarHeight = 6;

                ProgressBar1Visibility = Visibility.Visible;
                ProgressBar2Visibility = Visibility.Visible;
                ProgressBar3Visibility = Visibility.Collapsed;
            }
            else if (_ProgressList.Count >= 1)
            {
                ProgressBarHeight = 15;

                ProgressBar1Visibility = Visibility.Visible;
                ProgressBar2Visibility = Visibility.Collapsed;
                ProgressBar3Visibility = Visibility.Collapsed;
            }
            else
            {
                ProgressBar1Visibility = Visibility.Collapsed;
                ProgressBar2Visibility = Visibility.Collapsed;
                ProgressBar3Visibility = Visibility.Collapsed;
            }

            if (_ProgressList.Count == 0)
            {
                ProgressBarText = string.Empty;
                StatusBarText = "Ready";
            }
            else
            {
                /*                if (_ProgressList.Count == 1)
                                {
                                    StatusBarText = $"Exporting ({_ProgressList.Count} file) ";
                                }
                                else
                                {
                                    StatusBarText = $"Exporting ({_ProgressList.Count} files) ";
                                }
                */
            }
        }


        List<Guid> _ProgressList = new List<Guid>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Release(Guid guid)
        {
            int i = _ProgressList.IndexOf(guid);
            if (i == 0)
            {
                ProgressBar1Maximum = ProgressBar2Maximum;
                ProgressBar1Value = ProgressBar2Value;

                ProgressBar2Maximum = ProgressBar3Maximum;
                ProgressBar2Value = ProgressBar3Value;
            }
            else if (i == 1)
            {
                ProgressBar2Maximum = ProgressBar3Maximum;
                ProgressBar2Value = ProgressBar3Value;
            }

            _ProgressList.Remove(guid);
            UpdateProgressUI();

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetIndeterminate(Guid guid, bool bIndeterminate)
        {
            if (!_ProgressList.Contains(guid))
            {
                _ProgressList.Add(guid);
                UpdateProgressUI();
            }

            SetIndeterminate(_ProgressList.IndexOf(guid), bIndeterminate);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetRange(Guid guid, long range)
        {
            if (!_ProgressList.Contains(guid))
            {
                _ProgressList.Add(guid);
                UpdateProgressUI();
            }

            SetRange(_ProgressList.IndexOf(guid), range);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetProgress(Guid guid, long progress)
        {
            if (!_ProgressList.Contains(guid))
            {
                _ProgressList.Add(guid);
                UpdateProgressUI();
            }

            SetProgress(_ProgressList.IndexOf(guid), progress);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetProgressText(Guid guid, string text)
        {
            StatusBarText = text;
        }

        public void SetStatusBarText(string text)
        {
            StatusBarText = text;
        }
    }
}

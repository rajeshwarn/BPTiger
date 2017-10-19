using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Views.MainWindow
{
    //!!This class is not thread safe!!
    public class GenericStatusBarView
    {

        public static GenericStatusBarView NewInstance { get { return new GenericStatusBarView(); } private set { } }

        //TODO guyprio2 -- This is bad!!!, use events to access Main Window and not a class pointer!!
        private static MainWindowStatusBarView _MainWindowStatusBarView = null;
        public static void SetMainWindowStatusBarViewInstance(MainWindowStatusBarView instance) { _MainWindowStatusBarView = instance; }

        GenericStatusBarView()
        {
            cancellationTimeDelayToken.Cancel();
            cancellationTimeDelayToken = new CancellationTokenSource();

            //set defaults
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _MainWindowStatusBarView?.SetRange(guid, m_Range);
                _MainWindowStatusBarView?.SetIndeterminate(guid, true);
                _MainWindowStatusBarView?.UpdateProgressBar(guid, 0);
            });
        }


        private static CancellationTokenSource cancellationTimeDelayToken = new CancellationTokenSource();

        public async void Release()
        {
            try
            {
//                UpdateProgressBar(100);
                await Task.Delay(1000, cancellationTimeDelayToken.Token);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Progress Release Exception (cancel): {ex.Message}");
            }

            Application.Current?.Dispatcher.Invoke(() =>
            {
                _MainWindowStatusBarView?.Release(guid);
            });
        }

        private long m_Range = 100;
        private long m_CurrentProgress = 0;
        private long m_StartMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        private Guid guid = Guid.NewGuid();

        private long lastProgress = 0;
        private long _CurrentSectionSize = -1;
        private bool m_bShowTimeEllapsed = false;

        public void ShowTimeEllapsed(bool bShowTimeEllapsed)
        {
            m_bShowTimeEllapsed = bShowTimeEllapsed;
        }

        public void StartSection(long iSectionSize)
        {
            _CurrentSectionSize = iSectionSize;
        }
        public void EndSection()
        {
            lastProgress += _CurrentSectionSize;
            _CurrentSectionSize = -1;
        }

        public void SetRange(long range)
        {
            m_Range = range;

            Application.Current?.Dispatcher.Invoke(() =>
            {
                _MainWindowStatusBarView?.SetRange(guid, range);
            });
        }


        public void UpdateProgressBarByOne()
        {
            UpdateProgressBar(++m_CurrentProgress);
        }


        public void UpdateProgressBar(long progress)
        {
            long currentProgress = progress;
            if (_CurrentSectionSize > -1)
            {
                currentProgress = lastProgress + (_CurrentSectionSize * progress / m_Range);
            }
            else
            {
                lastProgress = progress;
            }

            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (m_bShowTimeEllapsed)
                {
                    _MainWindowStatusBarView?.UpdateProgressBar(guid, GetTimeLeftString(currentProgress), currentProgress);
                }
                else
                {
                    _MainWindowStatusBarView?.UpdateProgressBar(guid, currentProgress);
                }
            });
        }


        public void UpdateProgressBar(string text, long progress)
        {
            long currentProgress = progress;
            if (_CurrentSectionSize > -1)
            {
                currentProgress = lastProgress + (_CurrentSectionSize * progress / m_Range);
            }
            else
            {
                lastProgress = progress;
            }

            Application.Current?.Dispatcher.Invoke(() =>
            {
                _MainWindowStatusBarView?.UpdateProgressBar(guid, text + GetTimeLeftString(currentProgress), currentProgress);
                //SplashScreenHelper.ShowText(text);
            });
        }

        public void UpdateProgressBar(string text)
        {
            //            if ((bool)!Application.Current?.Dispatcher.CheckAccess())
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _MainWindowStatusBarView?.UpdateProgressBar(guid, text);
                //SplashScreenHelper.ShowText(text);
            });
        }


        private long LastLeftSec = 0;
        private const int TimeLeftChangeMargin = 5;
        private string GetTimeLeftString(long currentProgress)
        {
            string timeLeft = string.Empty;

            if (m_bShowTimeEllapsed && currentProgress != 0)
            {
                long LeftSec = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - m_StartMilliseconds) * (m_Range - currentProgress) / currentProgress / 1000;
                long LeftMin = LeftSec / 60;
                long LeftHour = LeftSec / 60 / 60;
                if (LeftMin == 0)
                {
                    if (((LeftSec - LastLeftSec) <= TimeLeftChangeMargin) && ((LeftSec - LastLeftSec) > 0))
                    {
                        LeftSec = LastLeftSec;
                    }
                    else
                    {
                        LastLeftSec = LeftSec;
                    }

                    timeLeft = $" [{LeftSec} seconds left]";
                }
                else if (LeftHour == 0)
                {
                    LeftSec -= LeftMin * 60;
                    timeLeft = $" [About {LeftMin} minutes left]";
                }
                else
                {
                    LeftSec -= LeftMin * 60;
                    LeftMin -= LeftHour * 60;

                    if (LeftHour > 1)
                    {
                        timeLeft = $" [About {LeftHour} hours and {LeftMin} minutes left]";
                    }
                    else
                    {
                        timeLeft = $" [About an hour and {LeftMin} minutes left]";
                    }
                }
            }

            return timeLeft;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CompleteBackup.Models.Utilities
{
    public class BackupPerfectLogger : ObservableObject
    {
        public BackupPerfectLogger()
        {
            m_LoggerData = "Starting Backup Perfect Logger...\n";
        }

        public string m_LoggerData;
        public string LoggerData { get { return m_LoggerData; } set { m_LoggerData = value; OnPropertyChanged(); } }

        public void Clear()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoggerData = string.Empty;
            }));
        }

        long m_LastLogSec = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        string m_CachedLog = string.Empty;
        private static CancellationTokenSource cancellationTimeDelayToken = new CancellationTokenSource();

        public void Writeln(string text)
        {
            Write(text + "\n");
        }

        public async void Write(string text)
        {
            lock (this)
            {
                m_CachedLog += $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} - {text}";
            }

            try
            {
                await Task.Delay(5000, cancellationTimeDelayToken.Token);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"BackupPerfect Logger Exception (cancel): {ex.Message}");
            }

            lock (this)
            {
                var secNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                var diff = secNow - m_LastLogSec;

                if ((secNow - m_LastLogSec) > 5000)
                {
                    LoggerData += m_CachedLog;
                    m_CachedLog = string.Empty;
                    m_LastLogSec = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
            }
        }
    }
}

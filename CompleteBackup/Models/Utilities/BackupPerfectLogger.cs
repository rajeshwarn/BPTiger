using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CompleteBackup.Models.Utilities
{
    public class BackupPerfectLogger : ObservableObject
    {
        public static BackupPerfectLogger Instance { get; } = new BackupPerfectLogger();

        BackupPerfectLogger()
        {
            m_Logger = "Starting Backup Perfect Logger...\n";
        }

        public string m_Logger;
        public string Logger { get { return m_Logger; } set { m_Logger = value; OnPropertyChanged(); } }

        public void Clear()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Logger = string.Empty;
            }));
        }

        public void Writeln(string text)
        {
            Write(text + "\n");
        }
        public void Write(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Logger += $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} - {text}";
            }));
        }
    }
}

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

        public void Writeln(string text)
        {
            Write(text + "\n");
        }
        public void Write(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoggerData += $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} - {text}";
            }));
        }
    }
}

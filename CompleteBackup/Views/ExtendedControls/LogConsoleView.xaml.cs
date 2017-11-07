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

namespace CompleteBackup.Views
{
    /// <summary>
    /// Interaction logic for LogConsoleView.xaml
    /// </summary>
    public partial class LogConsoleView : UserControl
    {

        static LogConsoleView Instance;

        public static void Writeln(string text)
        {
            Write(text + "\n");
        }
        public static void Write(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Instance?.tbOutputBlock.Inlines.Add(text);
            }));
        }

        public LogConsoleView()
        {
            InitializeComponent();

            Instance = this;

            tbOutputBlock.Inlines.Add("Starting Application...\n");
        }
    }
}

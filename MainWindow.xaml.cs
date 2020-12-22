using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Reflection;
using Microsoft.Win32;

namespace CppMemoryVisualizer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ProcessStartInfo mStartInfo = new ProcessStartInfo();
        private Process mProcess = new Process();

        private Thread mThreadCdb;

        private string mLog;
        public string Log
        {
            get
            {
                return mLog;
            }
            set
            {
                mLog = value;
                OnPropertyChanged("Log");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Closing += OnWindowClosing;
            /*
            App app = Application.Current as App;

            mStartInfo.FileName = app.CdbPath;
            mStartInfo.WorkingDirectory = "C:/myapp/myapp/Debug/";
            mStartInfo.Arguments = "-o myapp.exe -y myapp.pdb -srcpath myapp.cpp -c \".expr /s c++;.lines -e;bu myapp!main;g;g\"";
            mStartInfo.CreateNoWindow = false;
            mStartInfo.UseShellExecute = false;
            mStartInfo.RedirectStandardInput = true;
            mStartInfo.RedirectStandardOutput = true;
            mStartInfo.RedirectStandardError = true;

            mProcess.StartInfo = mStartInfo;
            mProcess.OutputDataReceived += onOutputDataReceived;
            mProcess.ErrorDataReceived += onErrorDataReceived;

            mThreadCdb = new Thread(new ThreadStart(cmd));
            mThreadCdb.Start();
            */
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            //mProcess.StandardInput.WriteLine("q");
            //mThreadCdb.Join();
        }

        private void cmd()
        {
            mProcess.Start();
            mProcess.BeginErrorReadLine();
            mProcess.BeginOutputReadLine();
            mProcess.WaitForExit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mProcess.StandardInput.WriteLine(xText.Text);
            xText.Text = string.Empty;
            // .expr /s c++
        }

        void onOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log += e.Data;
            Log += Environment.NewLine;
        }

        void onErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //MessageBox.Show(e.Data);
        }
    }
}

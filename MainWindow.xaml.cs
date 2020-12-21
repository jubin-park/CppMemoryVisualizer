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

        private Thread mThreadCDB;

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

        private string mVsPathOrNull;
        private string mCdbPathOrNull;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Closing += OnWindowClosing;

            {
                Debug.WriteLine("Loading vswhere.exe...");

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = "vswhere.exe";
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;

                process.StartInfo = startInfo;
                process.Start();

                string propertyName = "installationPath: ";
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (line.Contains(propertyName))
                    {
                        mVsPathOrNull = line.Substring(propertyName.Length);
                        break;
                    }
                }
                if (mVsPathOrNull == null)
                {
                    MessageBoxResult result = MessageBox.Show("Visual Studio가 설치되지 않았습니다. 다운로드 페이지로 이동하시겠습니까?", "caption", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("https://visualstudio.microsoft.com/ko/vs/older-downloads/");
                    }
                }

                process.WaitForExit();
            }

            {
                RegistryKey regKeyOrNull = Environment.Is64BitOperatingSystem ?
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows Kits\\Installed Roots", false) :
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows Kits\\Installed Roots", false);
                
                if (regKeyOrNull == null)
                {
                    MessageBoxResult result = MessageBox.Show("WinDbg 가 설치되지 않았습니다. Windows 10 SDK 다운로드 페이지로 이동하시겠습니까?", "caption", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
                    }
                }
                object valueOrNull = regKeyOrNull.GetValue("KitsRoot10");
                if (valueOrNull == null)
                {
                    MessageBox.Show("레지스트리 KitsRoot10 키가 존재하지 않습니다.", "caption", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    mCdbPathOrNull = System.IO.Path.Combine(valueOrNull.ToString(), "Debuggers\\x86\\cdb.exe");
                    if (!File.Exists(mCdbPathOrNull))
                    {
                        MessageBox.Show(string.Format("`{0}' 파일이 존재하지 않습니다.", mCdbPathOrNull), "caption", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            mStartInfo.FileName = mCdbPathOrNull;
            //mStartInfo.Arguments = "-o \"C:/myapp/myapp/Debug/myapp.exe\" -y \"C:/myapp/myapp/Debug/myapp.pdb\" -srcpath \"C:/myapp/myapp/Debug/myapp.cpp\"";
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

            mThreadCDB = new Thread(new ThreadStart(cmd));
            mThreadCDB.Start();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            mProcess.StandardInput.WriteLine("q");
            mThreadCDB.Join();
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

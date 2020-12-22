using CppMemoryVisualizer.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CppMemoryVisualizer.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadSourceFileCommand { get; }
        public ICommand DebugCommand { get; }

        private Process mProcessCdbOrNull;
        public Process ProcessCdbOrNull
        {
            get
            {
                return mProcessCdbOrNull;
            }
            set
            {
                mProcessCdbOrNull = value;
            }
        }

        private Thread mThreadCdbOrNull;
        public Thread ThreadCdbOrNull
        {
            get
            {
                return mThreadCdbOrNull;
            }
            set
            {
                mThreadCdbOrNull = value;
            }
        }

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


        private string mSourcePathOrNull;
        public string SourcePathOrNull
        {
            get { return mSourcePathOrNull; }
            set
            {
                mSourcePathOrNull = value;
                OnPropertyChanged("SourcePathOrNull");
            }
        }

        private string mSourceCode = string.Empty;
        public string SourceCode
        {
            get { return mSourceCode; }
            set
            {
                mSourceCode = value;
                OnPropertyChanged("SourceCode");
            }
        }

        public MainViewModel()
        {
            LoadSourceFileCommand = new LoadSourceFileCommand(this);
            DebugCommand = new DebugCommand(this);
        }

        public void ExecuteCdb(ProcessStartInfo processInfo)
        {
            mProcessCdbOrNull = new Process();
            mProcessCdbOrNull.StartInfo = processInfo;
            mProcessCdbOrNull.OutputDataReceived += onOutputDataReceived;
            mProcessCdbOrNull.ErrorDataReceived += onErrorDataReceived;

            mThreadCdbOrNull = new Thread(new ThreadStart(cmd));
            mThreadCdbOrNull.Start();
        }

        private void cmd()
        {
            mProcessCdbOrNull.Start();
            mProcessCdbOrNull.BeginErrorReadLine();
            mProcessCdbOrNull.BeginOutputReadLine();
        }

        private void onOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log += e.Data;
            Log += Environment.NewLine;
        }

        private void onErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //MessageBox.Show(e.Data);
        }
    }
}

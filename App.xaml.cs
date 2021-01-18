using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CppMemoryVisualizer
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static string WINDOW_TITLE = "C++ Memory Visualizer";

        private string mVsPathOrNull;
        public string VsPath
        {
            get
            {
                return mVsPathOrNull;
            }
        }

        private string mCdbPathOrNull;
        public string CdbPath
        {
            get
            {
                return mCdbPathOrNull;
            }
        }

        public App()
        {
            {
                Debug.Write("Loading vswhere.exe ... ");

                Process process = new Process();
                ProcessStartInfo processInfo = new ProcessStartInfo();

                processInfo.FileName = "vswhere.exe";
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process.StartInfo = processInfo;
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
                    if (MessageBoxResult.Yes == MessageBox.Show("Visual Studio가 설치되지 않았습니다. 다운로드 페이지로 이동하시겠습니까?", App.WINDOW_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
                    {
                        Process.Start("https://visualstudio.microsoft.com/ko/vs/older-downloads/");
                    }

                    process.WaitForExit();
                    Current.Shutdown();
                }

                process.WaitForExit();
                process.Close();

                Debug.WriteLine("SUCCESS");
            }

            {
                Debug.Write("Loading cdb.exe ... ");

                RegistryKey regKeyOrNull = Environment.Is64BitOperatingSystem ?
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows Kits\\Installed Roots", false) :
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows Kits\\Installed Roots", false);

                if (regKeyOrNull == null)
                {
                    if (MessageBoxResult.Yes == MessageBox.Show("WinDbg 가 설치되지 않았습니다. Windows 10 SDK 다운로드 페이지로 이동하시겠습니까?", App.WINDOW_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
                    {
                        Process.Start("https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
                    }

                    Current.Shutdown();
                }

                object valueOrNull = regKeyOrNull.GetValue("KitsRoot10");
                if (valueOrNull == null)
                {
                    MessageBox.Show("레지스트리 KitsRoot10 키가 존재하지 않습니다.", App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);

                    Current.Shutdown();
                }
                else
                {
                    mCdbPathOrNull = System.IO.Path.Combine(valueOrNull.ToString(), "Debuggers\\x86\\cdb.exe");
                    if (!File.Exists(mCdbPathOrNull))
                    {
                        MessageBox.Show(string.Format("`{0}' 파일이 존재하지 않습니다.", mCdbPathOrNull), App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);

                        Current.Shutdown();
                    }
                }

                Debug.WriteLine("SUCCESS");
            }
        }
    }
}

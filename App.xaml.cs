using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace CppMemoryVisualizer
{
    public partial class App : Application
    {
        public static string WINDOW_TITLE = "C++ Memory Visualizer";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            List<string> shouldInstallPackageNames = new List<string>();
            string[] programNames = { "gcc", "gdb" };
            foreach (var name in programNames)
            {
                Process process = new Process();
                ProcessStartInfo processInfo = new ProcessStartInfo();

                processInfo.FileName = name;
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;

                process.StartInfo = processInfo;

                try
                {
                    Debug.Write($"Loading {name} ... ");
                    process.Start();
                    process.Close();
                    Debug.WriteLine("SUCCESS");
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    shouldInstallPackageNames.Add(name);
                    Debug.WriteLine("FAILED");
                }
            }

            if (shouldInstallPackageNames.Count > 0)
            {
                string msg = string.Format("아래 {0}개의 GNU 패키지가 설치되지 않았습니다.{1}msys2 설치 및 설정 후, 아래의 패키지를 설치하십시오.", shouldInstallPackageNames.Count, Environment.NewLine);
                msg += Environment.NewLine;
                msg += Environment.NewLine;
                foreach (var name in shouldInstallPackageNames)
                {
                    msg += "- " + name + Environment.NewLine;
                }
                msg += Environment.NewLine;
                msg += "msys2 다운로드 페이지로 이동하시겠습니까?";

                if (MessageBoxResult.Yes == MessageBox.Show(msg, WINDOW_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
                {
                    Process.Start("https://www.msys2.org/");
                }

                Current.Shutdown();
            }
        }
    }
}

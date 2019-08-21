using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Interop;
using System.Diagnostics;

namespace AlarmClock
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process proc = Process.GetCurrentProcess();
            int count = Process.GetProcesses().Where(p =>
                p.ProcessName == proc.ProcessName).Count();

            if (count > 1)
            {
                MessageBox.Show("已经有一个进程在运行了！");
                App.Current.Shutdown();
            }
        }
    }
}

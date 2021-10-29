﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace FastGithub.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private const string FAST_GITHUB = "FastGithub";
        private const string PROJECT_URI = "https://github.com/dotnetcore/FastGithub";

        public MainWindow()
        {
            InitializeComponent();

            var about = new MenuItem("关于(&A)");
            about.Click += (s, e) => Process.Start(PROJECT_URI);

            var exit = new MenuItem("退出(&C)");
            exit.Click += (s, e) => this.Close();

            this.notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = FAST_GITHUB,
                ContextMenu = new ContextMenu(new[] { about, exit }),
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath)
            };

            this.notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Show();
                    this.Activate();
                    this.WindowState = WindowState.Normal;
                }
            };

            var fileName = $"{FAST_GITHUB}.exe";
            if (File.Exists(fileName) == true)
            {
                var version = FileVersionInfo.GetVersionInfo(fileName);
                this.Title = $"{FAST_GITHUB} v{version.ProductVersion}";
            }

            this.InitFlowChart();
        }

        private async void InitFlowChart()
        {
            var httpClient = new HttpClient();
            while (true)
            {
                try
                {
                    var response = await httpClient.GetAsync("http://127.0.0.1/flowRates");
                    var json = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
                    var flowRate = Newtonsoft.Json.JsonConvert.DeserializeObject<FlowRate>(json);
                    this.flowChart.Add(flowRate);
                }
                catch (Exception)
                {
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1d));
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(WndProc);

            IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                const int WM_SYSCOMMAND = 0x112;
                const int SC_MINIMIZE = 0xf020;

                if (msg == WM_SYSCOMMAND)
                {
                    if (wParam.ToInt32() == SC_MINIMIZE)
                    {
                        this.Hide();
                        handled = true;
                    }
                }

                return IntPtr.Zero;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this.notifyIcon.Icon = null;
            this.notifyIcon.Dispose();
            base.OnClosed(e);
        }
    }
}

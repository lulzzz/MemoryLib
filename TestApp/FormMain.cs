using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using MemLib;

namespace TestApp {
    public partial class FormMain : Form {
        public FormMain() {
            InitializeComponent();
            Logging.SetLogControl(TextBoxOutput);
        }

        private void FormMain_Load(object sender, EventArgs e) { }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) { }

        public static string BytesToString(IEnumerable<byte> array, string separator = "") {
            return array == null ? string.Empty : string.Join(separator, array.Select(v => $"{v:X2}"));
        }

        private static int _counter;
        private void ButtonTest1_Click(object sender, EventArgs e) {
            Logging.Clear();
            Logging.Log($"Counter={++_counter}");
            var proc = Process.GetCurrentProcess();
            //proc = Process.GetProcessesByName("sekiro").FirstOrDefault();
            //proc = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            //proc = Process.GetProcessesByName("notepad++").FirstOrDefault();
            //proc = Process.GetProcessesByName("ReClass.NET").FirstOrDefault();
            proc = Process.GetProcessesByName("mspaint").FirstOrDefault();
            var swTotal = Stopwatch.StartNew();

            using (var mem = new RemoteProcess(proc)) {

            }

            swTotal.Stop();
            Logging.Log($"TotalTime: {swTotal.Elapsed.TotalMilliseconds:N1} ms ({swTotal.Elapsed.Ticks:N1} ticks)");
        }
    }
}
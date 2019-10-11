using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BgWorker
{
    public partial class Form2 : Form
    {
        List<string> fileNames;
        int x, y = 0;
        public Form2()
        {
            InitializeComponent();
        }
        public Form2(List<string> fNames )
        {
            InitializeComponent();
            fileNames = fNames;
            x = Convert.ToInt32(Math.Ceiling(100.0 / fileNames.Count));
        }
        public Form2(List<string> fNames, int ThreadingTime)
        {
            InitializeComponent();
            fileNames = fNames;
            x = Convert.ToInt32(Math.Ceiling(100.0 / fileNames.Count));
            y = ThreadingTime;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            for (int i = 0; i < fileNames.Count; i++)
            {

                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    simulateHeavyJob();
                    backgroundWorker1.ReportProgress(i);
                }
            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (x + progressBar1.Value <= 100)
            {
                progressBar1.Value += x;
            }
            else
            {
                this.Close();
            }
            percentageLabel.Text = (progressBar1.Value).ToString() + " %";
            label2.Text = fileNames[e.ProgressPercentage];
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                display("You have Cancelled");
                progressBar1.Value = 0;
                percentageLabel.Text = "0 %";
            }
            else
            {
                this.Close();

            }
        }

        private void simulateHeavyJob()
        {
            if (y == 0)
                Thread.Sleep(x * 10);
            else
            {
                Thread.Sleep(y);
            }
        }

        private void display(String text)
        {
            MessageBox.Show(text);
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
    }
}

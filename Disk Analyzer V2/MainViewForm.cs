using BgWorker;
using DeskAnalyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;
using LiveCharts;
using LiveCharts.Wpf;

namespace DiskAnalyzer
{
    public partial class MainViewForm : Form
    {
        string fullPath = "";
        int panelWidth, panelHight;
        bool Hidden, hid;
        string path;
        public MainViewForm() : base()
        {
            InitializeComponent();
            panelWidth = 200;
            Hidden = true;
            panelHight = StartPanel.Height - 1;
            hid = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Ps2.Hide();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (hid)
                timer2.Start();
            Ps2.Visible = false;

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Ps2.Show();
                button2.Enabled = true; button2.Visible = true;
                button3.Enabled = true; button3.Visible = true;
                button5.Enabled = true; button5.Visible = true;
                btnTreeView.Enabled = true; btnTreeView.Visible = true;
                path = folderBrowserDialog1.SelectedPath;

                try
                {
                    List<string> s = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
                    Form2 f = new BgWorker.Form2(s);
                    f.Show();
                    Parallel.Invoke(() => { ListDirectory(treeView, path); }, () => { listView(path); });
                }
                catch
                {

                }
            }

        }
        private void ListDirectory(TreeView treeView, string path)
        {
            try
            {
                treeView.Nodes.Clear();
                var rootDirectoryInfo = new DirectoryInfo(path);
                treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
            }
            catch { }
        }
        private TreeNode CreateDirectoryNode(DriveInfo Drive)
        {
            var directoryNode = new TreeNode(Drive.Name);

            try
            {
                if (Drive.DriveType == DriveType.Fixed)
                {
                    DirectoryInfo d = Drive.RootDirectory;
                    foreach (var directory in d.GetDirectories())
                    {
                        directoryNode.Nodes.Add(CreateDirectoryNode(directory));
                    }
                    foreach (var file in d.GetFiles())
                    {
                        directoryNode.Nodes.Add(new TreeNode(file.Name));
                    }
                }

            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            return directoryNode;
        }
        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {

            var directoryNode = new TreeNode(directoryInfo.Name);

            foreach (var directory in directoryInfo.GetDirectories())
            {
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                directoryNode.Nodes.Add(new TreeNode(file.Name));
            }
            return directoryNode;
        }
        private static double GetSize(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                FileInfo f = new FileInfo(folderPath);
                return f.Length;
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
        }
        private string checkSize(double x)
        {
            string s = "";
            if (x >= Math.Pow(1024, 3))
                s = Math.Round((x / Math.Pow(1024, 3)), 2).ToString() + " GB";
            else if (x >= Math.Pow(1024, 2))
                s = Math.Round((x / Math.Pow(1024, 2)), 2).ToString() + " MB";
            else
                s = Math.Round((x / Math.Pow(1024, 1)), 2).ToString() + " KB";
            return s;
        }
        string FixNameLenght(string s)
        {
            if (s.Length > 20)
                s = s.Substring(0, 20) + "...";

            return s;
        }


        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                string[] f = path.Split('\\');
                string s = "";
                for (int i = 0; i < f.Length - 1; i++) s += f[i] + "\\";
                s += e.Node.FullPath;
                fullPath = s;
                DrawPieChart(s, e.Node.Text);
                DrawLivePieChart(s, e.Node.Text);
                listView(s);
                textBox1.Text = checkSize(GetSize(s));
                if (File.Exists(s))
                {
                    if (Path.GetExtension(s) == ".txt")
                        treeView.ContextMenuStrip = contextMenuStrip1;
                    else
                        treeView.ContextMenuStrip = contextMenuStrip3;
                }
                else
                {
                    treeView.ContextMenuStrip = contextMenuStrip2;
                }
            }
            catch { }
        }

        private void DrawPieChart(string path, string NodeName)
        {
            try
            {
                chart1.Series.Clear();
                var series = new System.Windows.Forms.DataVisualization.Charting.Series { ChartType = SeriesChartType.Pie };

                chart1.Series.Add(series);


                DirectoryInfo directory = new DirectoryInfo(path);
                int count = 0;
                chart1.Legends[0].Title = NodeName;
                chart1.Titles[0].Text = NodeName;
                chart1.Legends[0].TextWrapThreshold = 50;

                if (File.Exists(path))
                {
                    FileInfo file = new FileInfo(path);
                    series.Points.AddXY(FixNameLenght(file.Name), file.Length);
                    series.Points[count].Label = FixNameLenght(file.Name);
                    series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round((1.00), 2), checkSize(Math.Round(Convert.ToDouble(file.Length), 2)));

                }
                else
                {
                    double DirSize = GetSize(path), TempSize, other = 0;
                    List<DirectoryInfo> direcories = directory.GetDirectories().ToList();
                    List<FileInfo> files = directory.GetFiles().ToList();
                    if (direcories.Count + files.Count <= 10)
                    {
                        foreach (var dir in direcories)
                        {
                            TempSize = GetSize(dir.FullName);
                            series.Points.AddXY(FixNameLenght(dir.Name), TempSize);
                            series.Points[count].Label = FixNameLenght(dir.Name);
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                        }
                        foreach (var file in files)
                        {
                            TempSize = GetSize(file.FullName);
                            series.Points.AddXY(FixNameLenght(file.Name), TempSize);
                            series.Points[count].Label = FixNameLenght(file.Name);
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                        }
                    }
                    else
                    {
                        foreach (var dir in direcories)
                        {
                            TempSize = GetSize(dir.FullName);
                            if (TempSize > .05 * DirSize)
                            {
                                series.Points.AddXY(FixNameLenght(dir.Name), TempSize);
                                series.Points[count].Label = FixNameLenght(dir.Name);
                                series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                            }
                            else
                            {
                                other += TempSize;
                            }
                        }
                        foreach (var file in files)
                        {
                            TempSize = GetSize(file.FullName);
                            if (TempSize > .05 * DirSize)
                            {
                                series.Points.AddXY(FixNameLenght(file.Name), TempSize);
                                series.Points[count].Label = FixNameLenght(file.Name);
                                series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                            }
                            else
                            {
                                other += TempSize;
                            }
                        }
                        if (other > DirSize * .03)
                        {
                            series.Points.AddXY("Other", other);
                            series.Points[count].Label = FixNameLenght("Other");
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(other / DirSize) * 100, 2), checkSize(other));

                        }

                    }

                }
            }
            catch { }
        }
        private void DrawLivePieChart(string path, string NodeName)
        {
            try
            {
                LivePieChart.Series.Clear();

                var series = new System.Windows.Forms.DataVisualization.Charting.Series { ChartType = SeriesChartType.Pie };

                Func<ChartPoint, string> labelPoint = chartPoint =>
               string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation);
                chart1.Series.Add(series);
                LiveCharts.SeriesCollection ss = new LiveCharts.SeriesCollection();
                LivePieChart.Series = ss;
                DirectoryInfo directory = new DirectoryInfo(path);
                int count = 0;
                //chart1.Legends[0].Title = NodeName;
                //chart1.Titles[0].Text = NodeName;
                //chart1.Legends[0].TextWrapThreshold = 50;
                LivePieChart.LegendLocation = LegendLocation.Bottom;
                if (File.Exists(path))
                {
                    FileInfo file = new FileInfo(path);
                    ss.Add(new PieSeries
                    {
                        Title = FixNameLenght(file.Name),
                        Values = new ChartValues<double> { file.Length },
                        PushOut = 8,
                        DataLabels = true,
                        LabelPoint = labelPoint
                        

                    });
                    series.Points.AddXY(FixNameLenght(file.Name), file.Length);
                    series.Points[count].Label = FixNameLenght(file.Name);
                    series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round((1.00), 2), checkSize(Math.Round(Convert.ToDouble(file.Length), 2)));

                }
                else
                {
                    double DirSize = GetSize(path), TempSize, other = 0;
                    List<DirectoryInfo> direcories = directory.GetDirectories().ToList();
                    List<FileInfo> files = directory.GetFiles().ToList();
                    if (direcories.Count + files.Count <= 10)
                    {
                        foreach (var dir in direcories)
                        {
                            TempSize = GetSize(dir.FullName);
                            ss.Add(new PieSeries {
                                Title = FixNameLenght(dir.Name),
                                Values = new ChartValues<double> { TempSize },
                                PushOut = 8,
                                DataLabels = true,
                                LabelPoint = labelPoint

                            });
                           
                            series.Points.AddXY(FixNameLenght(dir.Name), TempSize);
                            series.Points[count].Label = FixNameLenght(dir.Name);
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                        }
                        foreach (var file in files)
                        {
                            TempSize = GetSize(file.FullName);
                            ss.Add(new PieSeries
                            {
                                Title = FixNameLenght(file.Name),
                                Values = new ChartValues<double> { TempSize },
                                PushOut = 8,
                                
                            
                                DataLabels = true,
                                LabelPoint = labelPoint

                            });
                            
                            series.Points.AddXY(FixNameLenght(file.Name), TempSize);
                            series.Points[count].Label = FixNameLenght(file.Name);
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                        }
                    }
                    else
                    {
                        foreach (var dir in direcories)
                        {
                            TempSize = GetSize(dir.FullName);
                           

                          
                            if (TempSize > .05 * DirSize)
                            {
                                ss.Add(new PieSeries
                                {
                                    Title = FixNameLenght(dir.Name),
                                    Values = new ChartValues<double> { TempSize },
                                    PushOut = 8,
                                    DataLabels = true,
                                    LabelPoint = labelPoint

                                });
                                series.Points.AddXY(FixNameLenght(dir.Name), TempSize);
                                series.Points[count].Label = FixNameLenght(dir.Name);
                                series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                            }
                            else
                            {
                                other += TempSize;
                            }
                        }
                        foreach (var file in files)
                        {
                            TempSize = GetSize(file.FullName);
                            if (TempSize > .05 * DirSize)
                            {                        
                                ss.Add(new PieSeries
                                {
                                    Title = FixNameLenght(file.Name),
                                    Values = new ChartValues<double> { TempSize },
                                    PushOut = 8,


                                    DataLabels = true,
                                    LabelPoint = labelPoint

                                });
                                series.Points.AddXY(FixNameLenght(file.Name), TempSize);
                                series.Points[count].Label = FixNameLenght(file.Name);
                                series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(TempSize / DirSize) * 100, 2), checkSize(TempSize));

                            }
                            else
                            {
                                other += TempSize;
                            }
                        }
                        if (other > DirSize * .03)
                        {
                          
                            ss.Add(new PieSeries
                            {
                                Title = FixNameLenght("Other"),
                                Values = new ChartValues<double> { DirSize },
                                PushOut = 8,


                                DataLabels = true,
                                LabelPoint = labelPoint

                            });
                            series.Points.AddXY("Other", other);
                            series.Points[count].Label = FixNameLenght("Other");
                            series.Points[count++].LegendText = string.Format("{0} % \r\n{1}", Math.Round(Convert.ToDouble(other / DirSize) * 100, 2), checkSize(other));

                        }

                    }

                }
            }
            catch { }
        }
        private void listView(string path)
        {
            try
            {
                double DirSize = GetSize(path);
                var allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                Dictionary<string, double> Extensions = new Dictionary<string, double>();
                string Extension;
                listView1.Items.Clear();
                foreach (var file in allFiles)
                {
                    Extension = Path.GetExtension(Path.GetFullPath(file).ToString());

                    if (Extensions.ContainsKey(Extension) == true)
                    {
                        Extensions[Extension] += GetSize(Path.GetFullPath(file).ToString()); ;
                    }
                    else
                    {
                        Extensions.Add(Extension, GetSize(Path.GetFullPath(file).ToString()));
                    }
                }
                double SizeOfOtherExtensions = 0.0;
                string[] arr = new string[3];
                foreach (var pair in Extensions)
                {

                    ListViewItem itm;

                    if (pair.Value >= .01 * DirSize)
                    {
                        arr[0] = pair.Key;
                        arr[1] = (Math.Round((pair.Value / DirSize), 3) * 100).ToString() + " % ";
                        arr[2] = checkSize(pair.Value);

                        itm = new ListViewItem(arr);
                        listView1.Items.Add(itm);
                    }
                    else
                    {
                        SizeOfOtherExtensions += pair.Value;
                    }

                }
                ListViewItem itm2;
                arr[0] = "Other";
                arr[1] = (Math.Round((SizeOfOtherExtensions / DirSize), 3) * 100).ToString() + " % ";
                arr[2] = checkSize(SizeOfOtherExtensions);
                itm2 = new ListViewItem(arr);
                listView1.Items.Add(itm2);
            }
            catch { }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            // Process.Start("http://www.google.com");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(fullPath);
        }

        private void PanelSlide_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Hidden)
            {
                PanelSlide.Width += 10;
                if (PanelSlide.Width >= panelWidth)
                {
                    timer1.Stop();
                    Hidden = false;
                    this.Refresh();
                }
            }
            else
            {
                PanelSlide.Width -= 10;
                if (PanelSlide.Width <= 0)
                {
                    timer1.Stop();
                    Hidden = true;
                    this.Refresh();
                }
            }
        }
        public void Encrypt(int Option)
        {
            FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string content = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            if (content.Length > 3 && content[content.Length - 1] == '*' && content[content.Length - 2] == '*' && content[content.Length - 3] == '*')
            {
                if (Option == 1)
                    MessageBox.Show("This file is already Encrypted");
            }
            else
            {
                FileStream fs1 = new FileStream(fullPath, FileMode.Truncate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                string NewContent = "";
                for (int i = 0; i < content.Length; i++)
                {
                    NewContent += (char)(content[i] + 65);
                }
                NewContent += "***";
                sw.Write(NewContent);
                sw.Close();
                fs1.Close();
            }
        }
        public void Decrypt(int Option)
        {
            FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string content = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            if (content.Length > 3 && content[content.Length - 1] != '*' && content[content.Length - 2] != '*' && content[content.Length - 3] != '*')
            {
                if (Option == 1)
                    MessageBox.Show("This file is already Decrypted");
            }
            else
            {
                FileStream fs1 = new FileStream(fullPath, FileMode.Truncate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                string NewContent = "";
                for (int i = 0; i < content.Length - 3; i++)
                {
                    NewContent += (char)(content[i] - 65);
                }
                sw.Write(NewContent);
                sw.Close();
                fs1.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Stay tund for Updates ^_^",
                "coming soon",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
        }

        private void button6_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.ShowDialog();
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Path.GetExtension(Path.GetFullPath(fullPath).ToString()) == ".txt")
            {
                Encrypt(1);
            }
            else
            {
                MessageBox.Show("Sorry Text Files Only :) ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void encryptAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(fullPath))
            {
                string[] allFiles = Directory.GetFiles(fullPath, "*.txt");
                Form2 f = new BgWorker.Form2(allFiles.ToList(), 500);
                f.Show();
                foreach (var file in allFiles)
                {
                    fullPath = file;
                    Encrypt(2);
                }
            }
            else
            {
                MessageBox.Show("working with Directory Only :) ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Path.GetExtension(Path.GetFullPath(fullPath).ToString()) == ".txt")
            {
                Decrypt(1);
            }
            else
            {
                MessageBox.Show("Sorry Text Files Only :) ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripTextBox2_Enter(object sender, EventArgs e)
        {
            //string x = toolStripTextBox3.Text;


            //if (Directory.Exists(fullPath))
            //{
            //    toolStripTextBox2.Text = "";
            //    string[] allFiles = Directory.GetFiles(x, "*.txt");
            //    int index = allFiles.Length;
            //    string global = textBox2.Text;
            //    for (int i = 0; i < allFiles.Length; i++)
            //    {
            //        StreamReader sr = new StreamReader(allFiles[i]);
            //        string line = sr.ReadLine();
            //        int count = 0;

            //        while (line != null)
            //        {

            //            string[] file = line.Split();

            //            for (int j = 0; j < file.Length; j++)
            //            {
            //                if (file[j] == textBox2.Text)
            //                {
            //                    count++;
            //                }
            //            }
            //            line = sr.ReadLine();
            //        }
            //        if (count != 0)
            //            textBox3.Text += allFiles[i] + ": number of rebeat this word in this file is " + count.ToString() + "\r\n";

            //    }
            //}
        }

        private void toolStripTextBox1_Enter(object sender, EventArgs e)
        {
            toolStripTextBox1.Text = "";
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
           
        }
        private int SearchWord(string x,string path)
        {
            string word ;
            int count = 0;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            string Content = sr.ReadToEnd();
            for (int i = 0; i < Content.Length - x.Length; i++)
            {
                word = Content.Substring(i, x.Length);
                if (word == x)
                    count++;
            }
            sr.Close();
            fs.Close();

            return count;
        }
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int count = SearchWord(toolStripTextBox1.Text,fullPath);
                if (count == 0)
                {
                    MessageBox.Show("Not Found :(  ", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(string.Format("Found {0} times ^_^ ", count), "Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
            }
        }

        private void toolStripTextBox1_MouseLeave(object sender, EventArgs e)
        {
            toolStripTextBox1.Text = "Search For The Word You Need";
        }

        private void toolStripTextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {   
                string[] allFiles = Directory.GetFiles(fullPath, "*.txt");
                int count=0,countFiles = 0, countTimes = 0;
                Form2 f = new BgWorker.Form2(allFiles.ToList(), 10);
                f.Show();
                foreach (var file in allFiles)
                {
                    count = SearchWord(toolStripTextBox3.Text,file);
                    countTimes += count;
                    if (count > 0)
                        countFiles++;
                }
                if (countTimes == 0)
                {
                    MessageBox.Show("Not Found :(  ", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(string.Format("Found {0} times in {1} Files ^_^ ", countTimes,countFiles), "Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
        }

        private void toolStripTextBox3_MouseEnter(object sender, EventArgs e)
        {
            toolStripTextBox3.Text = "";
        }

        private void toolStripTextBox3_MouseLeave(object sender, EventArgs e)
        {
            toolStripTextBox3.Text = "Search For The Word You Need";
        }

        private void decryptAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(fullPath))
            {
                string[] allFiles = Directory.GetFiles(fullPath, "*.txt");
                Form2 f = new BgWorker.Form2(allFiles.ToList(), 500);
                f.Show();
                foreach (var file in allFiles)
                {
                    fullPath = file;
                    Decrypt(2);
                }
            }
            else
            {
                MessageBox.Show("working with Directory Only :) ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            if (hid)
            {
                Ps2.Height += 10;
                if (Ps2.Height >= panelHight)
                {
                    timer2.Stop();
                    hid = false;
                    this.Refresh();
                }
            }
            else
            {
                Ps2.Height -= 10;
                if (Ps2.Height <= 0)
                {
                    timer2.Stop();
                    hid = true;
                    this.Refresh();
                }
            }
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}

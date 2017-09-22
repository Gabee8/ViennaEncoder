using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace ViennaEncoder
{
    public partial class splash : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
        public splash()
        {
            InitializeComponent();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            
        }
        BackgroundWorker worker = new BackgroundWorker();
        int n = 0;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkgWorker = (BackgroundWorker)sender;
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(400);
                bkgWorker.ReportProgress(i);
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            n++;
            if (n == 0)
            {
                label1.Text = "Indítás";
            }
            if (n == 1)
            {
                label1.Text = "Indítás.";
            }
            if (n == 2)
            {
                label1.Text = "Indítás..";
            }

            if (n == 3)
            {
                label1.Text = "Indítás...";
            }
            if (n == 4)
            {
                n = 0;
                label1.Text = "Indítás";
            }
        }

        private void splash_Load(object sender, EventArgs e)
        {
            label2.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('.', '0');
            this.worker.RunWorkerAsync();
            this.Opacity = 0;
            timer2.Enabled = true;
        }


        string[] checkfiles =
	            {
@"MediaInfo.dll",
@"Settings.ini", 
@"Microsoft.WindowsAPICodePack.dll",
@"Microsoft.WindowsAPICodePack.Shell.dll",
@"vcodecs.xml",
@"Vlc.DotNet.Core.dll",
@"Vlc.DotNet.Core.Interops.dll",
@"ShadowPanel.dll",
@"System.Windows.Forms.Ribbon35.dll",
@"Windows7ToolStripRenderer.dll",
@"WindowsFormsAero.dll",
@"Codecs\FFMpeg\ffmpeg.exe", 
@"Codecs\FFMpeg\ffprobe.exe",
@"Codecs\metamp3.exe",
@"Codecs\AtomicParsley.exe",
@"Codecs\mkvmerge.exe",
@"Codecs\freac\freaccmd.exe",
@"Codecs\Vlc\libvlc.dll",
@"Images\bg1.jpg",
@"Interop.WMPLib.dll",
@"AxInterop.WMPLib.dll"
	            };
        int g = -1;
        List<string> errorfiles = new List<string>();
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (g < checkfiles.Length - 1)
            {
                g++;
                string nonnnn = checkfiles[g];
                label3.Text = "Inicializálás: " + nonnnn;
                if (File.Exists(nonnnn) == false)
                {
                    errorfiles.Add(nonnnn);
                }
            }

            else
            {
                label3.Text = "";
                timer1.Stop();
                if (errorfiles.Count > 0)
                {
                    string line = string.Join("\n", errorfiles.ToArray());
                    MessageBox.Show(line + "\nNem található!", "Figyelem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Close();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.07;
            if (this.Opacity == 1)
            {
                timer1.Enabled = true;
                timer1.Start();
                timer2.Stop();
            }
        }

    }
}

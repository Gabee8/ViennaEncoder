using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using MediaInfoLib;

namespace ViennaEncoder
{
    public partial class VideoPlayerForm : Form
    {
        private readonly Form1 _form1;
        //Videoplay
        Vlc.DotNet.Core.VlcMediaPlayer vmpl;
        private string filePath;
        public string videofile;
        //Imagelistview kijelölés
        public const int LVM_FIRST = 0x1000; //Value from http://www.winehq.org/pipermail/wine-devel/2002-October/009527.html
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        public const int LVS_EX_DOUBLEBUFFER = 0x00010000;
        //Imports the UXTheme DLL
        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd, String textSubAppName, String textSubIdList);
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
        int Msg, int wParam, int lParam);

        public VideoPlayerForm(Form1 form1)
        {
            _form1 = form1;
            InitializeComponent();
            statusStrip1.Renderer = Antiufo.Controls.Windows7Renderer.Instance;
            SetWindowTheme(listView1.Handle, "explorer", null); //Explorer style
            SendMessage(listView1.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

            //VideoPlayer
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            DirectoryInfo VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @".\Codecs\Vlc\"));
            vmpl = new Vlc.DotNet.Core.VlcMediaPlayer(VlcLibDirectory);
            vmpl.PositionChanged += new System.EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs>(this.OnVlcPositionChanged);
            vmpl.VideoHostControlHandle = panel1.Handle;
            vmpl.Playing += new System.EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs>(this.OnPlaying);

        }
        TimeSpan startTime = new TimeSpan();
        TimeSpan endTime = new TimeSpan();

        string VideoPositionString;
        string lenghtString;
        private delegate void UpdateTextDelegate();
        private void OnVlcPositionChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs e)
        {
            var position = vmpl.GetMedia().Duration.Ticks * e.NewPosition;
            VideoPositionString = new DateTime((long)position).ToString("T");
            actualPoz.BeginInvoke(new UpdateTextDelegate(UpdateText));

        }
        TimeSpan positionactual = new TimeSpan();
        int _currentTime = 0;
        private void UpdateText()
        {
            actualPoz.Text = VideoPositionString;
            TimeSpan time = vmpl.GetMedia().Duration;
            vidLenght.Text = String.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
            //label1.Text = vmpl.GetMedia().Duration.ToString();
            updateTrackBar();
            positionactual = TimeSpan.Parse(VideoPositionString);
            _currentTime = positionactual.Hours * 60 + positionactual.Minutes * 60 + positionactual.Seconds;
        }
        private void updateTrackBar()
        {
            trackBar1.Tag = false;
            try
            {
                int newTrackbarPositionValue = (int)(vmpl.Position * 1000);
                if (newTrackbarPositionValue != trackBar1.Value)
                {
                    trackBar1.Value = newTrackbarPositionValue > trackBar1.Maximum
                                                ? trackBar1.Maximum
                                                : newTrackbarPositionValue;
                }
            }
            finally
            {
                trackBar1.Tag = true;
            }

        }
        private void OnPlaying(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {

        }
        int startvideo = 0;
        private void cancelButton_Click(object sender, EventArgs e)
        {
            vmpl.Stop();
            Close();
        }

        private void vidMute_Click(object sender, EventArgs e)
        {
            vmpl.Audio.ToggleMute();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                TrackBar trackBar = ((TrackBar)sender);
                if ((trackBar.Tag is Boolean) && ((Boolean)trackBar.Tag))
                {
                    float val = 1f * trackBar.Value / trackBar.Maximum;
                    if (val != vmpl.Position)
                        vmpl.Position = val;
                }
            }
            catch (Exception exc)
            {

                //
                MessageBox.Show(String.Format("Cannot change position : {0}", exc));
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            try
            {
                TrackBar trackBar = (TrackBar)sender;
                vmpl.Audio.Volume = (int)((1.0 * trackBar.Value / trackBar.Maximum) * 100);
            }
            catch (Exception exc)
            {
                MessageBox.Show(String.Format("Cannot change volume level: {0}", exc));
            }
        }

        private void vidPlay_Click(object sender, EventArgs e)
        {
            

                filePath = loadedVideo.Text;
                FileInfo file = new FileInfo(filePath);
                MediaInfo MediaInfoLib = new MediaInfo();
                MediaInfoLib.Open(filePath);

                vmpl.SetMedia(file);
                vmpl.Play();

        }

        private void VideoPlayerForm_Load(object sender, EventArgs e)
        {
            if (videofile != null)
            {
                loadedVideo.Text = videofile;
            }
            else
            {
                loadedVideo.Text = "Nincs";
            }
            
        }

        private void VideoPlayerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            vmpl.Stop();
        }

        private void vidPause_Click(object sender, EventArgs e)
        {
            vmpl.Pause();
        }

        private void vidRew_Click(object sender, EventArgs e)
        {
            float val = 1f * trackBar1.Value - 10;
            trackBar1.Value = Convert.ToInt32(val);
            if (val != vmpl.Position)
                vmpl.Position = val;
        }

        private void vidFF_Click(object sender, EventArgs e)
        {
            float val = 1f * trackBar1.Value + 10;
            trackBar1.Value = Convert.ToInt32(val);
            if (val != vmpl.Position)
                vmpl.Position = val;
        }

        private void vidBChap_Click(object sender, EventArgs e)
        {
            vmpl.Chapters.Previous();
        }

        private void vidNChap_Click(object sender, EventArgs e)
        {
            vmpl.Chapters.Next();
        }

        private void vidCutS_Click(object sender, EventArgs e)
        {
            lblStartpoz.Text = VideoPositionString;
            startTime = TimeSpan.Parse(VideoPositionString);
            differenceTime();
            startvideo = this._currentTime;
        }
        int osszes = 0;
        private void vidCutE_Click(object sender, EventArgs e)
        {
            lblEndpoz.Text = VideoPositionString;
            endTime = TimeSpan.Parse(VideoPositionString);
            differenceTime();
            osszes = this._currentTime - startvideo;
        }

        private void vidStop_Click(object sender, EventArgs e)
        {
            vmpl.Stop();
        }

        private void vidCut_Click(object sender, EventArgs e)
        {
            if (videofile != null)
            {
                string fileName = Path.GetFullPath(videofile);
                string outputName = Path.GetFileNameWithoutExtension(videofile) + "_cut_" + listView1.Items.Count.ToString();
                string Endstring = lblEndpoz.Text;
                string Startstring = lblStartpoz.Text;
                string cutduration = differentLb.Text;
                string[] row = { outputName, Startstring, Endstring, cutduration };
                var listViewItem = new ListViewItem(row);
                listViewItem.SubItems[2].Tag = osszes.ToString();
                listView1.Items.Add(listViewItem);
                itemsCount();
                //listView1.SelectedItems[0].SubItems[2].Tag = osszes;
            }
            
            
        }
        private void itemsCount()
        {
            TimeSpan osszeg = new TimeSpan();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                osszeg += TimeSpan.Parse(listView1.Items[i].SubItems[3].Text);
            }
            countLabel.Text = listView1.Items.Count.ToString() + " elem, " + osszeg.ToString();
        }
        private void differenceTime()
        {
            TimeSpan duration = endTime - startTime;
            differentLb.Text = duration.ToString();
        }

        private void ListDelBt_Click(object sender, EventArgs e)
        {
            for (int i = listView1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listView1.Items.RemoveAt(listView1.SelectedIndices[i]);
                itemsCount();
                //countLabel.Text = listView1.Items.Count + " elem";
            }
        }

        private void ListDownBt_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selected = listView1.SelectedItems[0];
                int indx = selected.Index;
                int totl = listView1.Items.Count;

                if (indx == totl - 1)
                {
                    listView1.Items.Remove(selected);
                    listView1.Items.Insert(0, selected);
                }
                else
                {
                    listView1.Items.Remove(selected);
                    listView1.Items.Insert(indx + 1, selected);
                }
            }
        }

        private void ListUpBt_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selected = listView1.SelectedItems[0];
                int indx = selected.Index;
                int totl = listView1.Items.Count;

                if (indx == 0)
                {
                    listView1.Items.Remove(selected);
                    listView1.Items.Insert(totl - 1, selected);
                }
                else
                {
                    listView1.Items.Remove(selected);
                    listView1.Items.Insert(indx - 1, selected);
                }
            }
        }


        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           
        }
        
        private void applyButton_Click(object sender, EventArgs e)
        {
            
            vmpl.Stop();
            string[] videofileadd = new string[listView1.Items.Count];
            string[] startadd = new string[listView1.Items.Count];
            string[] endadd = new string[listView1.Items.Count];
            string[] ossz = new string[listView1.Items.Count];

            for (int i = 0; i < listView1.Items.Count; i++)
            {

                videofileadd[i] = listView1.Items[i].SubItems[0].Text;

                videofileadd[i] = listView1.Items[i].SubItems[0].Text;
               
                startadd[i] = listView1.Items[i].SubItems[1].Text;
                
                endadd[i] = listView1.Items[i].SubItems[2].Text;

                ossz[i] = listView1.Items[i].SubItems[2].Tag.ToString();
                
            }
            _form1.cutVideos(listView1.Items.Count, videofileadd, startadd, endadd, ossz);
            this.Close();
        }

    }
}

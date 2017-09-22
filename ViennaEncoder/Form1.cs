using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using MediaInfoLib;
using siof = System.IO.File;
using Ini;
using System.Globalization;
using System.Resources;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using Etier.IconHelper;
using RemeiningTimerProject;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Drawing.Drawing2D;
using WindowsFormsAero.TaskDialog;
using System.Drawing.Imaging;
using WMPLib;

namespace ViennaEncoder
{



    public partial class Form1 : Form//RibbonForm
    {

        private bool _canShow = false;
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

        private ImageList _smallImageList = new ImageList();
        private ImageList _largeImageList = new ImageList();
        private IconListManager _iconListManager;
        XmlDocument xmldoc = new XmlDocument();


        //Konfigurációfájl helye
        IniFile ini = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        IniFile english = new IniFile(Environment.CurrentDirectory + "\\Language\\english.ini");
        IniFile hungarian = new IniFile(Environment.CurrentDirectory + "\\Language\\hungarian.ini");
        //string presetname;
        string temp = Path.GetTempPath();
        string windir = Environment.GetEnvironmentVariable("WINDIR");
        string programfiles = Environment.GetEnvironmentVariable("PROGRAMFILES");
        string allusers = Environment.GetEnvironmentVariable("USERPROFILE");
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
        int shutcommand = 0;
        int statusprocess = 1;
        int pid = 0;
        //int listcount = 0;
        //int listcountper = 0;
        string liststatus = "Várakozik: ";
        string actualoutputfilename = "";
        string errorcode = "";
        int listcheckbox = 0;
        int pass = 0;
        TimeSpan alltimes;
        bool waitingVideoMergeEnd = false;

        private ThumbnailToolBarButton thumbconvertButton;
        private ThumbnailToolBarButton thumbstopButton;
        private ThumbnailToolBarButton thumbpauseButton;

        private string filePath;

        private int _currentTime = 0;
        Stopwatch elapsedTime = new Stopwatch();
        string suboraudiomessenge = "";
        //Videoplay
        Vlc.DotNet.Core.VlcMediaPlayer vmpl;

        bool progressbar2state = false;
        int progressbar2value = 0;

        public Form1()
        {

            InitializeComponent();

            toolStrip1.Renderer = Antiufo.Controls.Windows7Renderer.Instance;
            statusStrip1.Renderer = Antiufo.Controls.Windows7Renderer.Instance;
            contextMenuStrip1.Renderer = Antiufo.Controls.Windows7Renderer.Instance;
            PlayerControlMenu.Renderer = new MySR();
            toolStrip2.Renderer = new MySR();
            toolStrip3.Renderer = new MySR();
            //toolStrip3.Renderer = Antiufo.Controls.Windows7Renderer.Instance;
            contextMenuStrip2.Renderer = Antiufo.Controls.Windows7Renderer.Instance;

            SetWindowTheme(listView1.Handle, "explorer", null); //Explorer style
            SendMessage(listView1.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);


            _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            _largeImageList.ColorDepth = ColorDepth.Depth32Bit;

            _smallImageList.ImageSize = new System.Drawing.Size(16, 16);
            _largeImageList.ImageSize = new System.Drawing.Size(32, 32);

            _iconListManager = new IconListManager(_smallImageList, _largeImageList);

            listView1.SmallImageList = _smallImageList;
            listView1.LargeImageList = _largeImageList;

            //VideoPlayer
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            DirectoryInfo VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @".\Codecs\Vlc\"));
            vmpl = new Vlc.DotNet.Core.VlcMediaPlayer(VlcLibDirectory);
            vmpl.PositionChanged += new System.EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs>(this.OnVlcPositionChanged);
            vmpl.VideoHostControlHandle = VideoPanel.Handle;
            vmpl.Playing += new System.EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs>(this.OnPlaying);
            vmpl.Stop();

            if (ini.IniReadValue("Settings", "ListCheckbox") == "false")
            {
                listcheckbox = 0;
                checkAllcb.Visible = false;
            }
            if (ini.IniReadValue("Settings", "ListCheckbox") == "true")
            {
                listcheckbox = 1;
                checkAllcb.Visible = true;
            }
            if (ini.IniReadValue("Settings", "Coder") == "FFMpeg")
            {
                ffmpegToolStripMenuItem.Checked = true;
                libavToolStripMenuItem.Checked = false;
            }
            if (ini.IniReadValue("Settings", "Coder") == "Libav")
            {
                libavToolStripMenuItem.Checked = true;
                ffmpegToolStripMenuItem.Checked = false;
            }

            if (ini.IniReadValue("Settings", "Language") == "Hungarian")
                ribbonButton98.PerformClick();
            else if (ini.IniReadValue("Settings", "Language") == "English")
            {
                ribbonButton99.PerformClick();
                try
                {
                    using (StreamReader reader = new StreamReader(english.path))
                    {
                        reader.Close();
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("A fájl nem található!\n" + english.path, "Figyelem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ribbonButton98.PerformClick();
                }
            }

            if (ini.IniReadValue("Settings", "Color") == "Fekete")
                Theme.ThemeColor = RibbonTheme.Black;
            else if (ini.IniReadValue("Settings", "Color") == "Zöld")
                Theme.ThemeColor = RibbonTheme.Green;
            else if (ini.IniReadValue("Settings", "Color") == "Lila")
                Theme.ThemeColor = RibbonTheme.Purple;
            else if (ini.IniReadValue("Settings", "Color") == "JellyBelly")
                Theme.ThemeColor = RibbonTheme.JellyBelly;
            else if (ini.IniReadValue("Settings", "Color") == "Halloween")
                Theme.ThemeColor = RibbonTheme.Halloween;
            else
                Theme.ThemeColor = RibbonTheme.Normal;
            this.Refresh();

            if (ini.IniReadValue("Settings", "DestinationPath") != null)
            {
                TargetTb.Text = ini.IniReadValue("Settings", "DestinationPath");
            }
            if (ini.IniReadValue("Settings", "FileOverWrite") != null)
            {
                FileExtCB.SelectedIndex = int.Parse(ini.IniReadValue("Settings", "FileOverWrite"));
            }
            if (ini.IniReadValue("Settings", "Help") == "Disabled")
            {
                helpOrbMenu.Visible = false;
                helpOrbMenu.Enabled = false;
            }
            if (ini.IniReadValue("Settings", "EnableSleep") == "false")
            {
                var previousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            }

            try
            {
                dataSet1.ReadXml("vcodecs.xml");
                dataSet2.ReadXml("acodecs.xml");
                if (dataSet1.Tables != null)
                {
                    ribbonComboBox1.DropDownItems.Clear();
                    for (int i = 0; i < dataSet1.Tables[0].Rows.Count; i++)
                    {
                        DataRow rw = dataSet1.Tables[0].Rows[i];
                        RibbonButton rb = new RibbonButton();
                        rb.Text = rw[0].ToString();
                        rb.Value = rw[1].ToString();
                        ribbonComboBox1.DropDownItems.Add(rb);
                    }
                }
                if (dataSet2.Tables != null)
                {
                    ribbonComboBox2.DropDownItems.Clear();
                    for (int i = 0; i < dataSet2.Tables[0].Rows.Count; i++)
                    {
                        DataRow rw = dataSet2.Tables[0].Rows[i];
                        RibbonButton rb = new RibbonButton();
                        rb.Text = rw[0].ToString();
                        rb.Value = rw[1].ToString();
                        ribbonComboBox2.DropDownItems.Add(rb);
                    }
                }

            }

            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
            }
            try
            {
                if (ini.IniReadValue("Settings", "LastPreset") == "true")
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load("lastpreset.xml");
                    XmlNodeList vcodecs = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Vcodecs");
                    string attrVcodecs = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Vcodecs/@Value").Value;

                    XmlNodeList acodecs = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Acodecs");
                    string attracodecs = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Acodecs/@Value").Value;
                    XmlNodeList Container = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Container");
                    string attrContainer = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Container/@Value").Value;
                    XmlNodeList Resolution = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Resolution");
                    string attrResolution = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Resolution/@Value").Value;
                    XmlNodeList VBitrate = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/VBitrate");
                    XmlNodeList ABitrate = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/ABitrate");
                    string attrABitrate = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/ABitrate/@Value").Value;
                    XmlNodeList AscpetRatio = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/AscpetRatio");
                    XmlNodeList ASample = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/ASample");
                    string attrASample = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/ASample/@Value").Value;
                    XmlNodeList AChannel = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/AChannel");
                    string attrAChannel = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/AChannel/@Value").Value;
                    XmlNodeList Subtitle = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Subtitle");
                    string attrSubtitle = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Subtitle/@Value").Value;
                    XmlNodeList Colorspace = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Colorspace");
                    string attrColorspace = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/Colorspace/@Value").Value;
                    XmlNodeList FPS = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/FPS");
                    string attrFPS = xmldoc.SelectSingleNode("FFMpegPresets/CustomSettings/FPS/@Value").Value;
                    XmlNodeList VideoDisable = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/VideoDisable");
                    XmlNodeList CopyVCodecs = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/CopyVCodecs");
                    XmlNodeList CopyACodecs = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/CopyACodecs");
                    XmlNodeList OnlyAudio = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/OnlyAudio");
                    XmlNodeList DisableAudio = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/DisableAudio");
                    XmlNodeList IDv3 = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/IDv3");
                    XmlNodeList CustomResolutions = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/CustomResolution");
                    XmlNodeList Rotate = xmldoc.SelectNodes("FFMpegPresets/CustomSettings/Rotate");

                    //videó kodek
                    RibbonButton rbvcodecs = new System.Windows.Forms.RibbonButton();
                    rbvcodecs.Text = vcodecs.Item(0).InnerText;
                    rbvcodecs.Value = attrVcodecs;
                    ribbonComboBox1.SelectedItem = rbvcodecs;
                    //audio kodek
                    RibbonButton rbacodecs = new System.Windows.Forms.RibbonButton();
                    rbacodecs.Text = acodecs.Item(0).InnerText;
                    rbacodecs.Value = attracodecs;
                    ribbonComboBox2.SelectedItem = rbacodecs;
                    //konténer
                    RibbonButton rbcontainer = new System.Windows.Forms.RibbonButton();
                    rbcontainer.Text = Container.Item(0).InnerText;
                    rbcontainer.Value = attrContainer;
                    ribbonComboBox8.SelectedItem = rbcontainer;
                    Extension = attrContainer;
                    //label1.Text = ribbonComboBox8.SelectedItem.Value;
                    //ribbonComboBox8.SelectedItem.Value = attrContainer;
                    //felbontás
                    RibbonButton rbResolution = new System.Windows.Forms.RibbonButton();
                    rbResolution.Text = Resolution.Item(0).InnerText;
                    rbResolution.Value = attrResolution;
                    ribbonComboBox6.SelectedItem = rbResolution;
                    //video bitráta
                    ribbonUpDown1.TextBoxText = VBitrate.Item(0).InnerText;
                    //audió bitráta
                    RibbonButton rbABitrate = new System.Windows.Forms.RibbonButton();
                    rbABitrate.Text = ABitrate.Item(0).InnerText;
                    rbABitrate.Value = attrABitrate;
                    ribbonComboBox3.SelectedItem = rbABitrate;
                    //audió mintavétel
                    RibbonButton rbASample = new System.Windows.Forms.RibbonButton();
                    rbASample.Text = ASample.Item(0).InnerText;
                    rbASample.Value = attrASample;
                    ribbonComboBox5.SelectedItem = rbASample;
                    //audió Csatorna
                    RibbonButton rbAChannel = new System.Windows.Forms.RibbonButton();
                    rbAChannel.Text = AChannel.Item(0).InnerText;
                    rbAChannel.Value = attrAChannel;
                    ribbonComboBox7.SelectedItem = rbAChannel;
                    //Felirat
                    if (Subtitle != null)
                    {
                        RibbonButton rbSubtitle = new System.Windows.Forms.RibbonButton();
                        rbSubtitle.Text = Subtitle.Item(0).InnerText;
                        rbSubtitle.Value = attrSubtitle;
                        ribbonComboBox4.SelectedItem = rbSubtitle;
                    }
                    //fps
                    if (FPS.Item(0).InnerText != "disabled")
                    {
                        RibbonButton rbFPS = new System.Windows.Forms.RibbonButton();
                        rbFPS.Text = FPS.Item(0).InnerText;
                        rbFPS.Value = attrFPS;
                        ribbonComboBox10.SelectedItem = rbFPS;
                    }
                    //színtér
                    if (Colorspace.Item(0).InnerText != "disabled")
                    {
                        RibbonButton rbColorspace = new System.Windows.Forms.RibbonButton();
                        rbColorspace.Text = Colorspace.Item(0).InnerText;
                        rbColorspace.Value = attrColorspace;
                        ribbonComboBox9.SelectedItem = rbColorspace;
                    }
                    //Képarány
                    videoaspect = AscpetRatio.Item(0).InnerText;
                    if (videoaspect == "-aspect 16:9 ")
                    {
                        asceptwBt.Checked = true;
                        asceptsBt.Checked = false;
                    }
                    if (videoaspect == "-aspect 4:3 ")
                    {
                        asceptwBt.Checked = false;
                        asceptsBt.Checked = true;
                    }
                    if (videoaspect != "-aspect 4:3 " && videoaspect != "-aspect 16:9 " && videoaspect != "")
                    {
                        asceptwBt.Checked = false;
                        asceptsBt.Checked = false;
                        asceptcusBt.Checked = true;
                        customarXYTb.TextBoxText = videoaspect.Remove(videoaspect.Length - 1).Replace("-aspect ", "");
                    }
                    if (VideoDisable.Item(0).InnerText == "true")
                    {
                        videodisabledCb.Checked = true;
                    }
                    if (VideoDisable.Item(0).InnerText == "false")
                    {
                        videodisabledCb.Checked = false;
                    }
                    //Original Video Codecs
                    if (CopyVCodecs.Item(0).InnerText == "true")
                    {
                        originalcodecsCb.Checked = true;
                    }
                    if (CopyVCodecs.Item(0).InnerText == "false")
                    {
                        originalcodecsCb.Checked = false;
                    }
                    //Original Audio Codecs
                    if (CopyACodecs.Item(0).InnerText == "true")
                    {
                        audiocopyCb.Checked = true;
                    }
                    if (CopyACodecs.Item(0).InnerText == "false")
                    {
                        audiocopyCb.Checked = false;
                    }
                    //Only Audio
                    if (OnlyAudio.Item(0).InnerText == "true")
                    {
                        onlyaudioCb.Checked = true;
                        OnlyAudioCheck(null, EventArgs.Empty);
                    }
                    if (OnlyAudio.Item(0).InnerText == "false")
                    {
                        onlyaudioCb.Checked = false;
                    }
                    //Audio Disable
                    if (DisableAudio.Item(0).InnerText == "true")
                    {
                        audiodisabledCb.Checked = true;
                    }
                    if (DisableAudio.Item(0).InnerText == "false")
                    {
                        audiodisabledCb.Checked = false;
                    }
                    //IDv3
                    if (IDv3.Item(0).InnerText == "true")
                    {
                        idv3Cb.Checked = true;
                    }
                    if (IDv3.Item(0).InnerText == "false")
                    {
                        idv3Cb.Checked = false;
                    }
                    //CustomResolution
                    if (CustomResolutions.Item(0).InnerText == "true")
                    {
                        CustomResLael.Enabled = true;
                        ribbonItemGroup1.Enabled = true;
                        string xy = attrResolution.Remove(attrResolution.Length - 1).Replace("-s ", "");
                        CusResXTb.TextBoxText = xy.Substring(0, xy.IndexOf('x'));
                        CusResYTb.TextBoxText = xy.Substring(xy.IndexOf('x') + 1);
                    }
                    if (CustomResolutions.Item(0).InnerText == "false")
                    {
                        CustomResLael.Enabled = false;
                        ribbonItemGroup1.Enabled = false;
                    }
                    //Rotate
                    if (Rotate.Item(0).InnerText == "0")
                    {
                        rotateCb.SelectedItem = rotate0;
                        rotateCb.Value = rotate0.Value;
                    }
                    if (Rotate.Item(0).InnerText == "1")
                    {
                        rotateCb.SelectedItem = rotate1;
                        rotateCb.Value = rotate1.Value;
                    }
                    if (Rotate.Item(0).InnerText == "2")
                    {
                        rotateCb.SelectedItem = rotate2;
                        rotateCb.Value = rotate2.Value;
                    }


                }
                else if (ini.IniReadValue("Settings", "LastPreset") == "false")
                {
                    ribbonComboBox1.SelectedItem = h264Bt;
                    ribbonComboBox2.SelectedItem = mp3codecsBt;
                    ribbonComboBox3.SelectedItem = audio128Bt;
                    ribbonComboBox5.SelectedItem = audio44100Bt;
                    ribbonComboBox7.SelectedItem = stereoBt;
                    ribbonComboBox6.SelectedItem = screenoriginalBt;
                    ribbonComboBox4.SelectedItem = onBt;
                    ribbonComboBox8.SelectedItem = aviBt;
                }
            }
            catch (Exception)
            {
                ribbonComboBox1.SelectedItem = h264Bt;
                ribbonComboBox2.SelectedItem = mp3codecsBt;
                ribbonComboBox3.SelectedItem = audio128Bt;
                ribbonComboBox5.SelectedItem = audio44100Bt;
                ribbonComboBox7.SelectedItem = stereoBt;
                ribbonComboBox6.SelectedItem = screenoriginalBt;
                ribbonComboBox4.SelectedItem = onBt;
                ribbonComboBox8.SelectedItem = aviBt;
                Extension = ribbonComboBox8.SelectedItem.Value.ToString();
            }
            ToolTip buttonToolTip = new ToolTip();
            buttonToolTip.ToolTipTitle = "Tipp...";
            buttonToolTip.UseFading = true;
            buttonToolTip.UseAnimation = true;
            buttonToolTip.IsBalloon = false;
            buttonToolTip.ShowAlways = true;
            buttonToolTip.AutoPopDelay = 5000;
            buttonToolTip.InitialDelay = 100;
            buttonToolTip.ReshowDelay = 500;
            buttonToolTip.ToolTipIcon = ToolTipIcon.Info;
            buttonToolTip.SetToolTip(pictureBox2, ini.IniReadValue("Settings", "Tipps"));
            //tipsLabel.Text = ini.IniReadValue("Settings", "Tipps");
            //Előzmények
            if (ini.IniReadValue("Settings", "Recent") == "true")
            {
                int count = 0;
                for (int i = 0; i < 12; i++)
                {
                    count++;
                    if (ini.IniReadValue("History", "Item" + count.ToString()) != "")
                    {
                        RibbonButton rb = new RibbonButton();
                        rb.Text = count.ToString() + ". " + ini.IniReadValue("History", "Item" + count.ToString());
                        rb.Value = ini.IniReadValue("History", "Item" + count.ToString());
                        rb.SmallImage = ViennaEncoder.Properties.Resources.historyIcon;
                        rb.ToolTip = rb.Value;
                        rb.Click += new EventHandler(rb_Click);
                        ribbon1.OrbDropDown.RecentItems.Add(rb);
                    }
                }
                RibbonButton rem = new RibbonButton();
                rem.Text = "Lista ürítése";
                rem.SmallImage = ViennaEncoder.Properties.Resources.remove;
                rem.Click += new EventHandler(rem_Click);
                ribbon1.OrbDropDown.RecentItems.Add(rem);
            }
            //eddig előtöltés
            //CD Meghajtó lekérdezése betöltése....
            var drives = from drive in DriveInfo.GetDrives() where drive.DriveType == DriveType.CDRom select drive;
            for (int i = 0; i < drives.ToList().Count; i++)
            {
                RibbonButton rb = new RibbonButton();
                rb.Text = drives.ToList()[i].ToString();
                rb.CheckOnClick = true;
                rb.SmallImage = Properties.Resources.cdDrive16;
                rb.Value = i.ToString();
                AudioCDSoureBt.DropDownItems.Add(rb);
            }

            presetsloading();

            splash sp = new splash();
            sp.FormClosed += new FormClosedEventHandler(frm_FormClosed);
            sp.Show();


        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]

            public static extern uint SetThreadExecutionState(uint esFlags);

            public const uint ES_CONTINUOUS = 0x80000000;

            public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        }

        private void TempFolderCreate()
        {
            if (!Directory.Exists(temp + "\\vienna-encoder"))
            {
                Directory.CreateDirectory(temp + "\\vienna-encoder");
            }
            
        }

        void frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _canShow = true;
            this.Show();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(_canShow && value);
        }

        private void TreeviewInfo()
        {
            //Video
            if (VCodec != "")
            {
                sumvid1.Text = "Kodek:" + VCodec.Remove(0, 4);
            }
            else
            {
                sumvid1.Text = "Kodek: Nincs";
            }
            if (VBitrate != "")
            {
                sumvid2.Text = "Bitráta:" + VBitrate.Remove(0, 4);
            }
            else
            {
                sumvid2.Text = "Bitráta: Nincs";
            }
            if (SizeString != "")
            {
                sumvid3.Text = "Felbontás:" + SizeString.Remove(0, 3);
            }
            else
            {
                sumvid3.Text = "Felbontás: Változatlan";
            }
            sumvid4.Text = "Konténer: " + Extension;
            //Audio
            if (ACodec != "")
            {
                sumaud1.Text = "Kodek:" + ACodec.Remove(0, 7);
            }
            else
            {
                sumaud1.Text = "Kodek:";
            }
            if (ABitrate != "")
            {
                if (ribbonComboBox3.SelectedValue.ToString() == "0" || ribbonComboBox3.SelectedValue.ToString() == "1" || ribbonComboBox3.SelectedValue.ToString() == "2" || ribbonComboBox3.SelectedValue.ToString() == "3" || ribbonComboBox3.SelectedValue.ToString() == "4" || ribbonComboBox3.SelectedValue.ToString() == "5" || ribbonComboBox3.SelectedValue.ToString() == "6" || ribbonComboBox3.SelectedValue.ToString() == "7" || ribbonComboBox3.SelectedValue.ToString() == "8")
                {
                    sumaud2.Text = "Bitráta:" + ribbonComboBox3.TextBoxText;
                }
                else
                {
                    sumaud2.Text = "Bitráta:" + ABitrate.Remove(0, 3);
                }
            }
            else
            {
                sumaud2.Text = "Bitráta: Nincs beállítva";
            }
            if (AChannels != "")
            {
                sumaud3.Text = "Csatorna:" + AChannels.Remove(0, 3);
            }
            else
            {
                sumaud3.Text = "Csatorna: Nincs beállítva";
            }
            //Felirat
            sumsub1.Text = "Felirat: " + ribbonComboBox4.TextBoxText;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (ini.IniReadValue("Settings", "TaskbarControl") != null)
            {
                if (ini.IniReadValue("Settings", "TaskbarControl") == "enable")
                {

                    if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)
                    {
                            TaskbarManager taskbar = TaskbarManager.Instance;

                            thumbconvertButton = new ThumbnailToolBarButton(ViennaEncoder.Properties.Resources.convert_thumb, "Konvertálás");
                            thumbconvertButton.Click += ConvertButton_ButtonClick;

                            thumbstopButton = new ThumbnailToolBarButton(ViennaEncoder.Properties.Resources.Stop_thumb, "Leállítás");
                            thumbstopButton.Click += stopBT_Click;

                            thumbpauseButton = new ThumbnailToolBarButton(ViennaEncoder.Properties.Resources.Pause_thumb, "Szünet/Folytatás");
                            thumbpauseButton.Click += PauseBT_Click;

                            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(this.Handle, thumbconvertButton, thumbstopButton, thumbpauseButton);
                    }
                }
            }


        }

        RemainingTimer RT = new RemainingTimer();
        double RemainValue = 0.0;
        private void ribbonOrbMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }


        //Only Audio Event!
        private void OnlyAudioCheck(object sender, EventArgs e)
        {
            ribbonCheckBox6_CheckBoxCheckChanged(sender, e);
        }

        private void ribbonUpDown1_DownButtonClicked(object sender, MouseEventArgs e)
        {
            int i = int.Parse(ribbonUpDown1.TextBoxText);
            i--;
            ribbonUpDown1.TextBoxText = i.ToString();
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";
            TreeviewInfo();
        }

        private void ribbonUpDown1_UpButtonClicked(object sender, MouseEventArgs e)
        {
            int i = int.Parse(ribbonUpDown1.TextBoxText);
            i++;
            ribbonUpDown1.TextBoxText = i.ToString();
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";
            TreeviewInfo();
        }

        private void ribbonUpDown1_TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";
        }

        private void ribbonUpDown4_DownButtonClicked(object sender, MouseEventArgs e)
        {
            int i = int.Parse(ribbonUpDown4.TextBoxText);
            i--;
            ribbonUpDown4.TextBoxText = i.ToString();
        }

        private void ribbonUpDown4_UpButtonClicked(object sender, MouseEventArgs e)
        {
            int i = int.Parse(ribbonUpDown4.TextBoxText);
            i++;
            ribbonUpDown4.TextBoxText = i.ToString();
        }

        private void ribbonUpDown4_TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar)
        && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void ribbonButton1_Click(object sender, EventArgs e)
        {
            if (asceptwBt.Checked == true)
            {
                videoaspect = "-aspect 16:9 ";
            }
            else
            {
                videoaspect = "";
            }
        }

        private void ribbonButton2_Click(object sender, EventArgs e)
        {
            if (asceptsBt.Checked == true)
            {
                videoaspect = "-aspect 4:3 ";
            }
            else
            {
                videoaspect = "";
            }
        }



        private void ribbonOrbMenuItem3_Click(object sender, EventArgs e)
        {
            settings frmK = new settings();
            //frmK.aboutControl1.Visible = true;
            frmK.ShowDialog();
        }

        private void ribbonOrbMenuItem2_Click(object sender, EventArgs e)
        {
            CustomPreset cstfrm = new CustomPreset(this);
            cstfrm.Show();
        }
        //Egyedi Beállítás mentése
        public void custompresetSave(string Iconnum, string nameString)
        {
            //Gyökér elemen belüli elemek létrehozása
            XDocument doc = XDocument.Load("presets.xml");
            XElement root = new XElement("PresetList");
            root.Add(new XElement("PresetName", new XAttribute("Value", "Custom"), nameString));
            root.Add(new XElement("PresetIcon", Iconnum));
            root.Add(new XElement("Vcodecs", new XAttribute("Value", ribbonComboBox1.SelectedValue), ribbonComboBox1.TextBoxText));
            root.Add(new XElement("Acodecs", new XAttribute("Value", ribbonComboBox2.SelectedValue), ribbonComboBox2.TextBoxText));
            root.Add(new XElement("Container", new XAttribute("Value", ribbonComboBox8.SelectedValue), ribbonComboBox8.TextBoxText));
            root.Add(new XElement("Resolution", new XAttribute("Value", ribbonComboBox6.SelectedValue), ribbonComboBox6.TextBoxText));
            root.Add(new XElement("VBitrate", ribbonUpDown1.TextBoxText));
            root.Add(new XElement("ABitrate", new XAttribute("Value", ribbonComboBox3.SelectedValue), ribbonComboBox3.TextBoxText));
            if (ribbonComboBox9.SelectedValue != null)
            {
                root.Add(new XElement("Colorspace", new XAttribute("Value", ribbonComboBox9.SelectedValue), ribbonComboBox9.TextBoxText));
            }
            else
            {
                root.Add(new XElement("Colorspace", new XAttribute("Value", "disabled"), "disabled"));
            }
            if (ribbonComboBox10.SelectedValue != null)
            {
                root.Add(new XElement("FPS", new XAttribute("Value", ribbonComboBox10.SelectedValue), ribbonComboBox10.TextBoxText));
            }
            else
            {
                root.Add(new XElement("FPS", new XAttribute("Value", "disabled"), "disabled"));
            }
            root.Add(new XElement("AscpetRatio", videoaspect));
            root.Add(new XElement("ASample", new XAttribute("Value", ribbonComboBox5.SelectedValue), ribbonComboBox5.TextBoxText));
            root.Add(new XElement("AChannel", new XAttribute("Value", ribbonComboBox7.SelectedValue), ribbonComboBox7.TextBoxText));
            if (ribbonComboBox4.SelectedValue != null)
            {
                root.Add(new XElement("Subtitle", new XAttribute("Value", ribbonComboBox4.SelectedValue), ribbonComboBox4.TextBoxText));
            }
            if (ribbonComboBox4.SelectedValue == null)
            {
                root.Add(new XElement("Subtitle", new XAttribute("Value", ""), ribbonComboBox4.TextBoxText));
            }

            if (videodisabledCb.Checked == true)
            {
                root.Add(new XElement("VideoDisable", "true"));
            }
            else
            {
                root.Add(new XElement("VideoDisable", "false"));
            }
            //Original Video Codecs
            if (originalcodecsCb.Checked == true)
            {
                root.Add(new XElement("CopyVCodecs", "true"));
            }
            else
            {
                root.Add(new XElement("CopyVCodecs", "false"));
            }
            //Original Audio Codecs
            if (audiocopyCb.Checked == true)
            {
                root.Add(new XElement("CopyACodecs", "true"));
            }
            else
            {
                root.Add(new XElement("CopyACodecs", "false"));
            }
            //Only Audio
            if (onlyaudioCb.Checked == true)
            {
                root.Add(new XElement("OnlyAudio", "true"));
            }
            else
            {
                root.Add(new XElement("OnlyAudio", "false"));
            }
            //Audio Disable
            if (audiodisabledCb.Checked == true)
            {
                root.Add(new XElement("DisableAudio", "true"));
            }
            else
            {
                root.Add(new XElement("DisableAudio", "false"));
            }
            //IDv3
            if (idv3Cb.Checked == true)
            {
                root.Add(new XElement("IDv3", "true"));
            }
            else
            {
                root.Add(new XElement("IDv3", "false"));
            }
            //Custom Resolution
            if (CustomResLael.Enabled == true)
            {
                root.Add(new XElement("CustomResolution", "true"));
            }
            else if (CustomResLael.Enabled == false)
            {
                root.Add(new XElement("CustomResolution", "false"));
            }
            //Rotate
            if (rotateCb.Value != "")
            {
                root.Add(new XElement("Rotate", rotateCb.Value));
            }
            //Itt a gyökér nevét kell megadni
            doc.Element("Presets").Add(root);
            doc.Save("presets.xml");
            presetsloading();
        }

        private void ribbonCheckBox4_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            if (labelCb.Checked == true)
            {
                ribbonTextBox2.Enabled = true;
                ribbonTextBox3.Enabled = true;
                ribbonTextBox4.Enabled = true;
                ribbonTextBox5.Enabled = true;
                ribbonTextBox6.Enabled = true;
                ribbonTextBox7.Enabled = true;
                ribbonTextBox8.Enabled = true;
                ribbonTextBox9.Enabled = true;
            }
            if (labelCb.Checked == false)
            {
                ribbonTextBox2.Enabled = false;
                ribbonTextBox3.Enabled = false;
                ribbonTextBox4.Enabled = false;
                ribbonTextBox5.Enabled = false;
                ribbonTextBox6.Enabled = false;
                ribbonTextBox7.Enabled = false;
                ribbonTextBox8.Enabled = false;
                ribbonTextBox9.Enabled = false;
            }

        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int i;
            for (i = 0; i < s.Length; i++)
                listView1.Items.Add(s[i]);
        }
        /// <summary>
        /// Form Bezárása!!!!!!!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            vmpl.Stop();
            
            ini.IniWriteValue("Settings", "FileOverWrite", FileExtCB.SelectedIndex.ToString());
            if (TargetTb.Text == "")
            {

            }
            else
            {
                ini.IniWriteValue("Settings", "DestinationPath", TargetTb.Text);
            }
            if (ini.IniReadValue("Settings", "LastPreset") == "true")
            {
                //XML létrehozása és a gyökér elem létrehozása
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("\t");
                settings.OmitXmlDeclaration = false;
                settings.Encoding = Encoding.UTF8;
                XmlWriter writer = XmlWriter.Create("lastpreset.xml", settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("FFMpegPresets");
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
                //XML létrehozás vége....
                //Gyökér elemen belüli elemek létrehozása
                XDocument doc = XDocument.Load("lastpreset.xml");
                XElement root = new XElement("CustomSettings");
                root.Add(new XElement("Vcodecs", new XAttribute("Value", ribbonComboBox1.SelectedValue), ribbonComboBox1.TextBoxText));
                root.Add(new XElement("Acodecs", new XAttribute("Value", ribbonComboBox2.SelectedValue), ribbonComboBox2.TextBoxText));
                root.Add(new XElement("Container", new XAttribute("Value", ribbonComboBox8.SelectedValue), ribbonComboBox8.TextBoxText));
                root.Add(new XElement("Resolution", new XAttribute("Value", ribbonComboBox6.SelectedValue), ribbonComboBox6.TextBoxText));
                root.Add(new XElement("VBitrate", ribbonUpDown1.TextBoxText));
                root.Add(new XElement("ABitrate", new XAttribute("Value", ribbonComboBox3.SelectedValue), ribbonComboBox3.TextBoxText));
                if (ribbonComboBox9.SelectedValue != null)
                {
                    root.Add(new XElement("Colorspace", new XAttribute("Value", ribbonComboBox9.SelectedValue), ribbonComboBox9.TextBoxText));
                }
                else
                {
                    root.Add(new XElement("Colorspace", new XAttribute("Value", "disabled"), "disabled"));
                }
                if (ribbonComboBox10.SelectedValue != null)
                {
                    root.Add(new XElement("FPS", new XAttribute("Value", ribbonComboBox10.SelectedValue), ribbonComboBox10.TextBoxText));
                }
                else
                {
                    root.Add(new XElement("FPS", new XAttribute("Value", "disabled"), "disabled"));
                }
                root.Add(new XElement("AscpetRatio", videoaspect));
                root.Add(new XElement("ASample", new XAttribute("Value", ribbonComboBox5.SelectedValue), ribbonComboBox5.TextBoxText));
                root.Add(new XElement("AChannel", new XAttribute("Value", ribbonComboBox7.SelectedValue), ribbonComboBox7.TextBoxText));
                if (ribbonComboBox4.SelectedValue != null)
                {
                    root.Add(new XElement("Subtitle", new XAttribute("Value", ribbonComboBox4.SelectedValue), ribbonComboBox4.TextBoxText));
                }
                if (ribbonComboBox4.SelectedValue == null)
                {
                    root.Add(new XElement("Subtitle", new XAttribute("Value", ""), ribbonComboBox4.TextBoxText));
                }

                if (videodisabledCb.Checked == true)
                {
                    root.Add(new XElement("VideoDisable", "true"));
                }
                else
                {
                    root.Add(new XElement("VideoDisable", "false"));
                }
                //Original Video Codecs
                if (originalcodecsCb.Checked == true)
                {
                    root.Add(new XElement("CopyVCodecs", "true"));
                }
                else
                {
                    root.Add(new XElement("CopyVCodecs", "false"));
                }
                //Original Audio Codecs
                if (audiocopyCb.Checked == true)
                {
                    root.Add(new XElement("CopyACodecs", "true"));
                }
                else
                {
                    root.Add(new XElement("CopyACodecs", "false"));
                }
                //Only Audio
                if (onlyaudioCb.Checked == true)
                {
                    root.Add(new XElement("OnlyAudio", "true"));
                }
                else
                {
                    root.Add(new XElement("OnlyAudio", "false"));
                }
                //Audio Disable
                if (audiodisabledCb.Checked == true)
                {
                    root.Add(new XElement("DisableAudio", "true"));
                }
                else
                {
                    root.Add(new XElement("DisableAudio", "false"));
                }
                //IDv3
                if (idv3Cb.Checked == true)
                {
                    root.Add(new XElement("IDv3", "true"));
                }
                else
                {
                    root.Add(new XElement("IDv3", "false"));
                }
                //Custom Resolution
                if (CustomResLael.Enabled == true)
                {
                    root.Add(new XElement("CustomResolution", "true"));
                }
                else if (CustomResLael.Enabled == false)
                {
                    root.Add(new XElement("CustomResolution", "false"));
                }
                //Rotate
                if (rotateCb.SelectedValue != "")
                {
                    root.Add(new XElement("Rotate", rotateCb.Value));
                }
                if (rotateCb.SelectedValue == "")
                {
                    root.Add(new XElement("Rotate", ""));
                }
                //Itt a gyökér nevét kell megadni
                doc.Element("FFMpegPresets").Add(root);
                doc.Save("lastpreset.xml");
                //FF leáll!
                Process[] prs = Process.GetProcesses();
                foreach (Process pr in prs)
                {
                    if (pr.ProcessName == "ffmpeg")
                    {
                        pr.Kill();
                    }
                }

            }
        }

        private void ribbonComboBox1_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            XDocument xdoc3 = new XDocument();
            string s = ribbonComboBox1.TextBoxText as string;

            string filePath = Environment.CurrentDirectory + "\\vcodecs.xml";

            xdoc3 = XDocument.Load(filePath);

            if (s != null)
            {
                var list = (from xNode in xdoc3.Root.Descendants("CodecsList") where xNode.Element("CodecsName").Value == s select xNode);

                string VCodecsName = "";
                string ContainerName = "";
                string attrVcodecs = "";
                string attrContainer = "";
                foreach (XElement ele in list)
                {
                    VCodecsName = ele.Element("CodecsName").Value;
                    attrVcodecs = ele.Element("CodecsParam").Value;
                    if (ele.Element("CodecsExt")!= null)
                    {
                        ContainerName = ele.Element("CodecsExt").Value;
                        attrContainer = ele.Element("CodecsExt").FirstAttribute.Value;
                        //konténer
                        RibbonButton rbcontainer = new System.Windows.Forms.RibbonButton();
                        rbcontainer.Text = ContainerName;
                        rbcontainer.Value = attrContainer;
                        ribbonComboBox8.SelectedItem = rbcontainer;
                        Extension = attrContainer;
                    }
                    //videó kodek
                    RibbonButton rbvcodecs = new System.Windows.Forms.RibbonButton();
                    rbvcodecs.Text = VCodecsName;
                    rbvcodecs.Value = attrVcodecs;
                    ribbonComboBox1.SelectedItem = rbvcodecs;
                   
                }
            }
            VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
        }

        private void ribbonComboBox2_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            ACodec = ribbonComboBox2.SelectedValue.ToString();
            //video disabled
            if (videodisabledCb.Checked == true)
            {
                disable_video = "-vn ";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
            }
            //audio disabled
            if (audiodisabledCb.Checked == false)
            {
                disable_audio = "";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
            }
            if (originalcodecsCb.Checked == true)
            {
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "-c:v copy ";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                //copyvideocodecs = "-c:v copy ";
            }
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            File.Copy(temp + "vienna-encoder\\out.txt", temp + "vienna-encoder\\file-new.txt", true);
            String last = "";
            if (new FileInfo(temp + "vienna-encoder\\file-new.txt").Length > 0)
            {
                last = File.ReadAllLines(temp + "vienna-encoder\\file-new.txt").Last();
                //textBox2.Text = last;
                if (last.Contains("Warning:"))
                {
                    List<string> text = File.ReadAllLines(temp + "vienna-encoder\\file-new.txt").Reverse().Take(2).ToList();
                    last = text[1];
                }
                convertInfoLabel.Text = last;
            }

            if (last.Contains("time="))
            {
                string pat = @"\d{2}:\d{2}:\d{2}";
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                Match m = r.Match(last);
                string last2 = m.ToString();
                ActualPositionTB.Text = last2;

                string kbs = @"\d{2,5}\.\dkbits\/s";
                Regex r3 = new Regex(kbs, RegexOptions.IgnoreCase);
                Match m3 = r3.Match(last);
                string out3 = m3.ToString();
                BitrateTB.Text = out3;

                string sizes = @"\d{1,8}kB";
                Regex r2 = new Regex(sizes, RegexOptions.IgnoreCase);
                Match m2 = r2.Match(last);
                string out2 = m2.ToString();
                var filesize = Regex.Split(out2, @"[\s,kB]+");
                double size = double.Parse(filesize[0].ToString()) / 1024;
                FileSizeTB.Text = string.Format("{0:0.000}", size) + " MB";
                //textBox2.Text = last2;
                //Progressbarhoz
                if (progressBar1.Style != ProgressBarStyle.Marquee)
                {
                    char[] arr = new char[] { ':', ' ' };
                    last2 = last2.TrimStart(arr);
                    var values = Regex.Split(last2, @"[\s,:]+");
                    int ora = int.Parse(values[0]);
                    int perc = int.Parse(values[1]);
                    int mp = int.Parse(values[2]);
                    int osszeg2 = ora * 3600 + 60 * perc + mp;
                    progressBar1.Value = osszeg2;
                    double ertekk = osszeg2 / (double)progressBar1.Maximum * 100;
                    double calculated;
                    calculated = size / ertekk * 100;
                    CalculatedSizeTb.Text = string.Format("{0:00.0}", calculated) + " MB";

                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        if (Environment.OSVersion.Version.Minor >= 1)
                        {
                            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                            Thread.Sleep(50);
                            prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                        }
                    }
                    //Százalék
                    int percent = (int)(((double)progressBar1.Value / (double)progressBar1.Maximum) * 100);
                    label4.Text = percent.ToString() + "%";
                    RemainValue = osszeg2;
                    RT.Mark(RemainValue);
                    RemainTB.Text = RT.ToString();
                    TimeSpan ts = elapsedTime.Elapsed;
                    elapsedTimeTb.Text = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                    //progressbar2value = progressbar2value + percent;
                    progressBar2.Value = progressbar2value + percent;
                }
                //Log...

                else
                {
                    label4.Text = "N/A";
                }

            }

        }


        public void ribbonCheckBox6_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            if (onlyaudioCb.Checked == true)
            {
                disable_video = "";
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                videodisabledCb.Checked = true;
                //ribbonButton66.Enabled = false;
                //ribbonComboBox8.DropDownItems.ElementAt(2).Visible = false;
                ribbonComboBox1.Enabled = false;
                ribbonComboBox6.Enabled = false;
                ribbonComboBox9.Enabled = false;
                ribbonComboBox10.Enabled = false;
                ribbonUpDown1.Enabled = false;
                for (int i = 0; i < 20; i++)
                {
                    ribbonComboBox8.DropDownItems.ElementAt(i).Visible = false;
                }
                ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(20);
                Extension = ribbonComboBox8.SelectedValue.ToString();
                x264crf = "";
                vidmaxrate = "";
                vidminrate = "";
                VbitrateMaxTb.Enabled = false;
                VbitrateMinTb.Enabled = false;
                ribbonLabel2.Enabled = false;
                ribbonLabel3.Enabled = false;
                VideoBitrateIcon.Enabled = false;
                VideoBitrateLabel.Enabled = false;
                CompressionTypeCb.Enabled = false;
                CompressionTypeCb.Checked = false;
            }
            else
            {
                videodisabledCb.Checked = false;
                for (int i = 0; i < 20; i++)
                {
                    ribbonComboBox8.DropDownItems.ElementAt(i).Visible = true;
                }
                ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(0);
                ribbonComboBox1.Enabled = true;
                ribbonComboBox6.Enabled = true;
                ribbonComboBox9.Enabled = true;
                ribbonComboBox10.Enabled = true;
                ribbonUpDown1.Enabled = true;
                VideoBitrateIcon.Checked = false;
                VideoBitrateIcon.Enabled = true;
                VideoBitrateLabel.Enabled = true;
                CompressionTypeCb.Enabled = true;
            }
        }

        private void ribbonButton91_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(10);
        }

        private void ribbonButton89_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(0);
        }

        private void ribbonButton90_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(14);
        }

        private void ribbonButton80_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(6);
        }

        private void ribbonButton92_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(5);
        }

        private void ribbonButton93_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(7);
        }

        private void ribbonButton96_Click(object sender, EventArgs e)
        {
            ribbonComboBox2.SelectedItem = ribbonComboBox2.DropDownItems.ElementAt(16);
        }

        private void ribbonButton98_Click(object sender, EventArgs e)
        {
            ini.IniWriteValue("Settings", "Language", "Hungarian");
            try
            {

                label2.Text = hungarian.IniReadValue("Language", "Label2_text");
                label3.Text = hungarian.IniReadValue("Language", "Label3_text");
                PlayerControlMenu.Text = hungarian.IniReadValue("Language", "LenghtLB");
                LenghtLB.Text = hungarian.IniReadValue("Language", "LenghtLB");
                ConvertButton.Text = hungarian.IniReadValue("Language", "Convert_text");
                sourceBrovseBT.Text = hungarian.IniReadValue("Language", "Button2_text");
                OutputBrowseBt.Text = hungarian.IniReadValue("Language", "Button3_text");
                //button4.Text = hungarian.IniReadValue("Language", "Button4_text");
                stopBT.Text = hungarian.IniReadValue("Language", "Button5_text");
                ListMoveUStrip.ToolTipText = hungarian.IniReadValue("Language", "Button6_tooltip");
                ListMoveDStrip.ToolTipText = hungarian.IniReadValue("Language", "Button7_tooltip");
                ListDelStrip.ToolTipText = hungarian.IniReadValue("Language", "Button8_tooltip");
                //checkBox1.Text = hungarian.IniReadValue("Language", "CheckBox1_text");
                //checkBox2.Text = hungarian.IniReadValue("Language", "CheckBox2_text");
                GroupSumLb.Text = hungarian.IniReadValue("Language", "GroupSum_text");

                rotateCb.Text = hungarian.IniReadValue("Language", "Rotate_text");
                label1.Text = hungarian.IniReadValue("Language", "Label1_text");
                FileSizeLB.Text = hungarian.IniReadValue("Language", "FileSizeLB_text");
                VBitrateLB.Text = hungarian.IniReadValue("Language", "VBitrateLB_text");
                RemainLB.Text = hungarian.IniReadValue("Language", "RemainLB_text");
                elapsedTimeLb.Text = hungarian.IniReadValue("Language", "elapsedLB_text");
                PauseBT.Text = hungarian.IniReadValue("Language", "PauseBT_text");
                LogBT.Text = hungarian.IniReadValue("Language", "LogBT_text");
                FileExtLB.Text = hungarian.IniReadValue("Language", "FileExtLB_text");

                ribbonTab1.Text = hungarian.IniReadValue("Language", "RibbonTab1");
                ribbonTab2.Text = hungarian.IniReadValue("Language", "RibbonTab2");
                ribbonTab3.Text = hungarian.IniReadValue("Language", "RibbonTab3");
                ribbonTab4.Text = hungarian.IniReadValue("Language", "RibbonTab4");
                ribbonTab5.Text = hungarian.IniReadValue("Language", "RibbonTab5");
                ribbonTab6.Text = hungarian.IniReadValue("Language", "RibbonTab6");
                ribbonTab7.Text = hungarian.IniReadValue("Language", "RibbonTab7");
                ribbonOrbMenuItem1.Text = hungarian.IniReadValue("Language", "RibbonOrb1");
                ribbonOrbMenuItem2.Text = hungarian.IniReadValue("Language", "RibbonOrb2");
                ribbonOrbMenuItem3.Text = hungarian.IniReadValue("Language", "RibbonOrb3");
                ribbonOrbMenuItem4.Text = hungarian.IniReadValue("Language", "RibbonOrb4");
                helpOrbMenu.Text = hungarian.IniReadValue("Language", "HelpMenu");
                VideoBitrateLabel.Text = hungarian.IniReadValue("Language", "RibbonUpDown1");
                ribbonUpDown4.Text = hungarian.IniReadValue("Language", "RibbonUpDown4");
                ribbonPanel1.Text = hungarian.IniReadValue("Language", "RibbonPanel1");
                ribbonPanel2.Text = hungarian.IniReadValue("Language", "RibbonPanel2");
                ribbonPanel3.Text = hungarian.IniReadValue("Language", "RibbonPanel3");
                ribbonPanel4.Text = hungarian.IniReadValue("Language", "RibbonPanel4");
                ribbonPanel5.Text = hungarian.IniReadValue("Language", "RibbonPanel5");
                ribbonPanel6.Text = hungarian.IniReadValue("Language", "RibbonPanel6");
                ribbonPanel7.Text = hungarian.IniReadValue("Language", "RibbonPanel7");
                ribbonPanel8.Text = hungarian.IniReadValue("Language", "RibbonPanel8");
                ribbonPanel9.Text = hungarian.IniReadValue("Language", "RibbonPanel9");
                ribbonPanel10.Text = hungarian.IniReadValue("Language", "RibbonPanel10");
                ribbonPanel11.Text = hungarian.IniReadValue("Language", "RibbonPanel11");
                ribbonPanel12.Text = hungarian.IniReadValue("Language", "RibbonPanel12");
                asceptwBt.Text = hungarian.IniReadValue("Language", "AspectWide");
                asceptsBt.Text = hungarian.IniReadValue("Language", "AspectStandard");
                asceptcusBt.Text = hungarian.IniReadValue("Language", "AspectOther");
                xyLabel.Text = hungarian.IniReadValue("Language", "XYLabel");
                screen1920x1080Bt.Text = hungarian.IniReadValue("Language", "RibbonButton4");
                onBt.Text = hungarian.IniReadValue("Language", "RibbonButton5");
                offBt.Text = hungarian.IniReadValue("Language", "RibbonButton6");
                screencomputerBt.Text = hungarian.IniReadValue("Language", "RibbonButton7");
                screen320x240Bt.Text = hungarian.IniReadValue("Language", "RibbonButton8");
                screen426x240Bt.Text = hungarian.IniReadValue("Language", "RibbonButton9");
                screendvdBt.Text = hungarian.IniReadValue("Language", "RibbonButton31");
                screen1280x720Bt.Text = hungarian.IniReadValue("Language", "RibbonButton32");
                monoBt.Text = hungarian.IniReadValue("Language", "RibbonButton59");
                stereoBt.Text = hungarian.IniReadValue("Language", "RibbonButton60");
                homethreadBt.Text = hungarian.IniReadValue("Language", "RibbonButton64");
                screen720x576Bt.Text = hungarian.IniReadValue("Language", "RibbonButton65");
                screen1440x816Bt.Text = hungarian.IniReadValue("Language", "RibbonButton86");
                screen640x480Bt.Text = hungarian.IniReadValue("Language", "RibbonButton87");
                screenoriginalBt.Text = hungarian.IniReadValue("Language", "RibbonButton88");
                ribbonButton98.Text = hungarian.IniReadValue("Language", "RibbonButton98");
                ribbonButton99.Text = hungarian.IniReadValue("Language", "RibbonButton99");
                ribbonButton100.Text = hungarian.IniReadValue("Language", "RibbonButton100");
                ribbonComboBox1.Text = hungarian.IniReadValue("Language", "ribbonComboBox1_text");
                ribbonComboBox2.Text = hungarian.IniReadValue("Language", "ribbonComboBox2_text");
                ribbonComboBox3.Text = hungarian.IniReadValue("Language", "ribbonComboBox3_text");
                ribbonComboBox4.Text = hungarian.IniReadValue("Language", "ribbonComboBox4_text");
                ribbonComboBox5.Text = hungarian.IniReadValue("Language", "ribbonComboBox5_text");
                ribbonComboBox6.Text = hungarian.IniReadValue("Language", "ribbonComboBox6_text");
                ribbonComboBox7.Text = hungarian.IniReadValue("Language", "ribbonComboBox7_text");
                ribbonComboBox8.Text = hungarian.IniReadValue("Language", "ribbonComboBox8_text");
                audiodisabledCb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox1_text");
                videodisabledCb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox2_text");
                originalcodecsCb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox3_text");
                audiocopyCb.Text = hungarian.IniReadValue("Language", "AudioCopy_string");
                subtitleTb.Text = hungarian.IniReadValue("Language", "subtitleTb_text");
                SubtitleBt.Text = hungarian.IniReadValue("Language", "SubtitleBt_text");
                StartPozTb.Text = hungarian.IniReadValue("Language", "StartPozTb_text");
                EndPozTb.Text = hungarian.IniReadValue("Language", "EndPozTb_text");

                labelCb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox4_text");
                idv3Cb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox5_text");
                onlyaudioCb.Text = hungarian.IniReadValue("Language", "RibbonCheckBox6_text");
                ribbonTextBox1.Text = hungarian.IniReadValue("Language", "RibbonTextBox1_text");
                ribbonTextBox2.Text = hungarian.IniReadValue("Language", "RibbonTextBox2_text");
                ribbonTextBox3.Text = hungarian.IniReadValue("Language", "RibbonTextBox3_text");
                ribbonTextBox4.Text = hungarian.IniReadValue("Language", "RibbonTextBox4_text");
                ribbonTextBox5.Text = hungarian.IniReadValue("Language", "RibbonTextBox5_text");
                ribbonTextBox6.Text = hungarian.IniReadValue("Language", "RibbonTextBox6_text");
                ribbonTextBox7.Text = hungarian.IniReadValue("Language", "RibbonTextBox7_text");
                ribbonTextBox8.Text = hungarian.IniReadValue("Language", "RibbonTextBox8_text");
                ribbonTextBox9.Text = hungarian.IniReadValue("Language", "RibbonTextBox9_text");
                coverpathTb.Text = hungarian.IniReadValue("Language", "coverpathTb_text");
                ribbonComboBox9.Text = hungarian.IniReadValue("Language", "colorspace_text");
                InfoVidCodecsTb.Text = hungarian.IniReadValue("Language", "InfoCodecs_text");
                InfoAudCodecsTb.Text = hungarian.IniReadValue("Language", "InfoCodecs_text");
                InfoAud2CodecsTb.Text = hungarian.IniReadValue("Language", "InfoCodecs_text");
                InfoVidBitrateTb.Text = hungarian.IniReadValue("Language", "InfoBitrate_text");
                InfoAudBitrTb.Text = hungarian.IniReadValue("Language", "InfoBitrate_text");
                InfoAud2BitrTb.Text = hungarian.IniReadValue("Language", "InfoBitrate_text");
                InfoLenghtTb.Text = hungarian.IniReadValue("Language", "InfoLenght_text");
                InfoSizeTb.Text = hungarian.IniReadValue("Language", "InfoSize_text");
                InfoContainerTb.Text = hungarian.IniReadValue("Language", "InfoContainer_text");
                InfoVidResTb.Text = hungarian.IniReadValue("Language", "InfoVidRes_text");
                InfoVidArTb.Text = hungarian.IniReadValue("Language", "InfoVidAr_text");
                InfoVidFpsTb.Text = hungarian.IniReadValue("Language", "InfoVidFps_text");
                InfoVidStandTb.Text = hungarian.IniReadValue("Language", "InfoVidStand_text");
                InfoVidColorTb.Text = hungarian.IniReadValue("Language", "InfoVidColor_text");
                InfoVidFormatTb.Text = hungarian.IniReadValue("Language", "InfoVidFormat_text");
                InfoAudSampleTb.Text = hungarian.IniReadValue("Language", "InfoAudSample_text");
                InfoAud2SampleTb.Text = hungarian.IniReadValue("Language", "InfoAudSample_text");
                InfoAudChanTb.Text = hungarian.IniReadValue("Language", "InfoAudChannel_text");
                InfoAud2ChanTb.Text = hungarian.IniReadValue("Language", "InfoAudChannel_text");
                listView1.Columns[0].Text = hungarian.IniReadValue("Language", "ListviewName_text");
                listView1.Columns[1].Text = hungarian.IniReadValue("Language", "ListviewSize_text");
                listView1.Columns[2].Text = hungarian.IniReadValue("Language", "ListviewLenght_text");
                listView1.Columns[3].Text = hungarian.IniReadValue("Language", "ListviewOutName_text");
                listView1.Columns[4].Text = hungarian.IniReadValue("Language", "ListviewStartPos_text");
                listView1.Columns[5].Text = hungarian.IniReadValue("Language", "ListviewEndPos_text");
                listView1.Columns[6].Text = hungarian.IniReadValue("Language", "ListviewSub_text");
                listView1.Columns[7].Text = hungarian.IniReadValue("Language", "ListviewAudio_text");
                autoshut.Text = hungarian.IniReadValue("Language", "ConvertAfter_text");
                shutdownBT.Text = hungarian.IniReadValue("Language", "Shutdown_text");
                rebootBt.Text = hungarian.IniReadValue("Language", "Reboot_text");
                logoutBT.Text = hungarian.IniReadValue("Language", "Logout_text");
                hibernateBt.Text = hungarian.IniReadValue("Language", "Hibernate_text");
                sleepBt.Text = hungarian.IniReadValue("Language", "Sleep_text");
                nothingBt.Text = hungarian.IniReadValue("Language", "Nothing_text");
                ribbonOrbMenuItem5.Text = hungarian.IniReadValue("Language", "About_text");
                ActualPosVidLb.Text = hungarian.IniReadValue("Language", "ActualPosVid_text");
                CompressionTypeCb.Text = hungarian.IniReadValue("Language", "CompressionType");
                JoinVideosLb.Text = hungarian.IniReadValue("Language", "Join_text");
                LenghtVidLb.Text = hungarian.IniReadValue("Language", "RibbonTextBox1_text");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //Magyar
                label1.Text = "Aktuális Pozíció:";
                label2.Text = "Cél:";
                label3.Text = "Előbeállítások";
                PlayerControlMenu.Text = "Teljes hossz:";
                //checkBox1.Text = "Hardver képesség";
                //checkBox2.Text = "Konvertáló ablak rejtése";
                ConvertButton.Text = "Konvertálás";
                sourceBrovseBT.Text = "Tallózás";
                OutputBrowseBt.Text = "Célmappa megadása";
               //button4.Text = "Betöltés...";
                stopBT.Text = "Stop!";
                ListMoveUStrip.ToolTipText = "Kijelölt elem mozgatása fel";
                ListMoveDStrip.ToolTipText = "Kijelölt elem mozgatása le";
                ListDelStrip.ToolTipText = "Kijelölt elem eltávolítása";
                GroupSumLb.Text = "Konvertálási Infó";
                ribbonTab1.Text = "Videó";
                ribbonTab2.Text = "Audió";
                ribbonTab3.Text = "Cimkék és Borító";
                ribbonTab4.Text = "Hossz és Felirat";
                ribbonTab5.Text = "Fájl Információ";
                ribbonTab6.Text = "3D Videó";
                ribbonTab7.Text = "Audio CD Bemásolás";
                ribbonOrbMenuItem1.Text = "Kilépés";
                ribbonOrbMenuItem2.Text = "Egyedi Beállítás Mentése";
                ribbonOrbMenuItem3.Text = "Beállítás";
                ribbonOrbMenuItem4.Text = "Nyelv";
                helpOrbMenu.Text = "Súgó";
                VideoBitrateLabel.Text = "Videó Bitráta:";
                ribbonUpDown4.Text = "Hangerő (256=normál):";
                ribbonPanel1.Text = "Oldalarány";
                ribbonPanel2.Text = "Videó Kódolás";
                ribbonPanel3.Text = "Kódolás";
                ribbonPanel4.Text = "Extra Audió Beállítás";
                ribbonPanel5.Text = "Felirat";
                ribbonPanel6.Text = "Címkék és Borító";
                ribbonPanel7.Text = "Extra Videó Beállítás";
                ribbonPanel8.Text = "Hosszúság";
                ribbonPanel9.Text = "Általános";
                ribbonPanel10.Text = "Videó";
                ribbonPanel11.Text = "Audió 1";
                ribbonPanel12.Text = "Audió 2";
                asceptwBt.Text = "Szélesvásznú (16:9)";
                asceptsBt.Text = "Normál (4:3)";
                asceptcusBt.Text = "Egyéni";
                xyLabel.Text = "X:Y vagy (Példa: 1.3333)";
                screen1920x1080Bt.Text = "Nagy Felbontású megjelenítéshez (1080p)";
                onBt.Text = "Be";
                offBt.Text = "Ki";
                screencomputerBt.Text = "Számítógéphez (854x480)";
                screen320x240Bt.Text = "Mobil 320 x 240";
                screen426x240Bt.Text = "Mobil (kis méret 426x240)";
                screendvdBt.Text = "DVD (720 x 480)";
                screen1280x720Bt.Text = "HD Felbontás (720p)";
                monoBt.Text = "Monó";
                stereoBt.Text = "Sztereó";
                homethreadBt.Text = "5.1 (házimozi)";
                screen720x576Bt.Text = "Szélesvásznú DVD (720x576)";
                screen1440x816Bt.Text = "HDTV (1440 x 816)";
                screen640x480Bt.Text = "Standard TV (640 x 480)";
                screenoriginalBt.Text = "Eredeti Felbontás";
                ribbonButton98.Text = "Magyar";
                ribbonButton99.Text = "Angol";
                ribbonButton100.Text = "AlbumBorító";
                ribbonComboBox1.Text = "Videó Kodek:";
                ribbonComboBox2.Text = "Audió Kodek:";
                ribbonComboBox3.Text = "Audió Bitráta:";
                ribbonComboBox4.Text = "Felirat Be-Ki:";
                ribbonComboBox5.Text = "Audió Mintavételezés:";
                ribbonComboBox6.Text = "Felbontás:";
                ribbonComboBox7.Text = "Csatornák Száma:";
                ribbonComboBox8.Text = "Videó/Audió Konténer:";
                audiodisabledCb.Text = "Audió Letiltása";
                videodisabledCb.Text = "Videó Letiltása";
                originalcodecsCb.Text = "Eredeti Kodek(Másolás)";
                labelCb.Text = "";
                idv3Cb.Text = "IDv3 Megőrzése (és borító)";
                onlyaudioCb.Text = "Csak Audió Konvertálás";
                ribbonTextBox1.Text = "Hossz:";
                ribbonTextBox2.Text = "Cím:";
                ribbonTextBox3.Text = "Szerző:";
                ribbonTextBox4.Text = "Komment:";
                ribbonTextBox5.Text = "Album:";
                ribbonTextBox6.Text = "Szerző:";
                ribbonTextBox7.Text = "Év:";
                ribbonTextBox8.Text = "Sorszám:";
                ribbonTextBox9.Text = "Leírás:";
                coverpathTb.Text = "Borító Helye:";
                ribbonComboBox9.Text = "Szinezet:";
                subtitleTb.Text = "Felirat Helye:";
                SubtitleBt.Text = "Felirat Tallózása";
                StartPozTb.Text = "Kezdő Pozíció";
                EndPozTb.Text = "Vég Pozíció";
                rotateCb.Text = "Forgatás:";
                FileSizeLB.Text = "Fájlméret:";
                VBitrateLB.Text = "Bitráta:";
                RemainLB.Text = "Hátralévő Idő:";
                PauseBT.Text = "Szünet";
                LogBT.Text = "Napló";
                FileExtLB.Text = "Ha a fájl létezik";
                InfoVidCodecsTb.Text = "Kodek";
                InfoVidBitrateTb.Text = "Bitráta";
                InfoAudBitrTb.Text = "Bitráta";
                InfoAud2BitrTb.Text = "Bitráta";
                InfoVidBitrateTb.Text = "Bitráta";
                InfoLenghtTb.Text = "Hossz";
                InfoSizeTb.Text = "Méret";
                InfoContainerTb.Text = "Konténer";
                InfoVidResTb.Text = "Felbontás";
                InfoVidArTb.Text = "Képarány";
                InfoVidFpsTb.Text = "FPS";
                InfoVidStandTb.Text = "=Szabvány";
                InfoVidColorTb.Text = "Szinezet";
                InfoVidFormatTb.Text = "Formátum";
                InfoAudSampleTb.Text = "Mintavétel";
                InfoAudChanTb.Text = "Csatorna Szám";
                InfoAud2ChanTb.Text = "Csatorna Szám";
                listView1.Columns[0].Text = "Fájlnév";
                listView1.Columns[1].Text = "Méret";
                listView1.Columns[2].Text = "Hossz";
                listView1.Columns[3].Text = "Kimeneti fájlnév";
                listView1.Columns[4].Text = "Kezdő Poz.";
                listView1.Columns[5].Text = "Vég Poz.";
                listView1.Columns[6].Text = "Felirat";
                listView1.Columns[7].Text = "Hangsáv";
                autoshut.Text = "Konvertálás utáni esemény";
                shutdownBT.Text = "Leállítás";
                rebootBt.Text = "Újraindítás";
                logoutBT.Text = "Kijelentkezés";
                hibernateBt.Text = "Hibernálás";
                sleepBt.Text = "Alvó állapot";
                nothingBt.Text = "Ne csináljon semmit";
                ribbonOrbMenuItem5.Text = "Névjegy";
                elapsedTimeLb.Text = "Eltelt Idő:";
                LenghtLB.Text = "Teljes hossz:";
                ActualPosVidLb.Text = "Aktuális Pozíció:";
                LenghtVidLb.Text = "Hossz:";
                CompressionTypeCb.Text = "2 Menetes Tömörítés";
                JoinVideosLb.Text = "Egyesítés:";
                LenghtVidLb.Text = "Hossz:";
            }


        }

        private void ribbonButton99_Click(object sender, EventArgs e)
        {
            //Angol
            ini.IniWriteValue("Settings", "Language", "English");
            try
            {
                label1.Text = english.IniReadValue("Language", "Label1_text");
                label2.Text = english.IniReadValue("Language", "Label2_text");
                label3.Text = english.IniReadValue("Language", "Label3_text");
                PlayerControlMenu.Text = english.IniReadValue("Language", "LenghtLB");
                LenghtLB.Text = english.IniReadValue("Language", "LenghtLB");
                ConvertButton.Text = english.IniReadValue("Language", "Convert_text");
                sourceBrovseBT.Text = english.IniReadValue("Language", "Button2_text");
                OutputBrowseBt.Text = english.IniReadValue("Language", "Button3_text");
                //button4.Text = english.IniReadValue("Language", "Button4_text");
                stopBT.Text = english.IniReadValue("Language", "Button5_text");
                ListMoveUStrip.ToolTipText = english.IniReadValue("Language", "Button6_tooltip");
                ListMoveDStrip.ToolTipText = english.IniReadValue("Language", "Button7_tooltip");
                ListDelStrip.ToolTipText = english.IniReadValue("Language", "Button8_tooltip");
                //checkBox1.Text = english.IniReadValue("Language", "CheckBox1_text");
                //checkBox2.Text = english.IniReadValue("Language", "CheckBox2_text");
                GroupSumLb.Text = english.IniReadValue("Language", "GroupSum_text");
                rotateCb.Text = english.IniReadValue("Language", "Rotate_text");
                label1.Text = english.IniReadValue("Language", "Label1_text");
                FileSizeLB.Text = english.IniReadValue("Language", "FileSizeLB_text");
                VBitrateLB.Text = english.IniReadValue("Language", "VBitrateLB_text");
                RemainLB.Text = english.IniReadValue("Language", "RemainLB_text");
                elapsedTimeLb.Text = english.IniReadValue("Language", "elapsedLB_text");
                PauseBT.Text = english.IniReadValue("Language", "PauseBT_text");
                LogBT.Text = english.IniReadValue("Language", "LogBT_text");
                FileExtLB.Text = english.IniReadValue("Language", "FileExtLB_text");
                ribbonTab1.Text = english.IniReadValue("Language", "RibbonTab1");
                ribbonTab2.Text = english.IniReadValue("Language", "RibbonTab2");
                ribbonTab3.Text = english.IniReadValue("Language", "RibbonTab3");
                ribbonTab4.Text = english.IniReadValue("Language", "RibbonTab4");
                ribbonTab5.Text = english.IniReadValue("Language", "RibbonTab5");
                ribbonTab6.Text = english.IniReadValue("Language", "RibbonTab6");
                ribbonTab7.Text = english.IniReadValue("Language", "RibbonTab7");
                ribbonOrbMenuItem1.Text = english.IniReadValue("Language", "RibbonOrb1");
                ribbonOrbMenuItem2.Text = english.IniReadValue("Language", "RibbonOrb2");
                ribbonOrbMenuItem3.Text = english.IniReadValue("Language", "RibbonOrb3");
                ribbonOrbMenuItem4.Text = english.IniReadValue("Language", "RibbonOrb4");
                helpOrbMenu.Text = english.IniReadValue("Language", "HelpMenu");
                VideoBitrateLabel.Text = english.IniReadValue("Language", "RibbonUpDown1");
                ribbonUpDown4.Text = english.IniReadValue("Language", "RibbonUpDown4");
                ribbonPanel1.Text = english.IniReadValue("Language", "RibbonPanel1");
                ribbonPanel2.Text = english.IniReadValue("Language", "RibbonPanel2");
                ribbonPanel3.Text = english.IniReadValue("Language", "RibbonPanel3");
                ribbonPanel4.Text = english.IniReadValue("Language", "RibbonPanel4");
                ribbonPanel5.Text = english.IniReadValue("Language", "RibbonPanel5");
                ribbonPanel6.Text = english.IniReadValue("Language", "RibbonPanel6");
                ribbonPanel7.Text = english.IniReadValue("Language", "RibbonPanel7");
                ribbonPanel8.Text = english.IniReadValue("Language", "RibbonPanel8");
                ribbonPanel9.Text = english.IniReadValue("Language", "RibbonPanel9");
                ribbonPanel10.Text = english.IniReadValue("Language", "RibbonPanel10");
                ribbonPanel11.Text = english.IniReadValue("Language", "RibbonPanel11");
                ribbonPanel12.Text = english.IniReadValue("Language", "RibbonPanel12");
                asceptwBt.Text = english.IniReadValue("Language", "AspectWide");
                asceptsBt.Text = english.IniReadValue("Language", "AspectStandard");
                asceptcusBt.Text = english.IniReadValue("Language", "AspectOther");
                xyLabel.Text = english.IniReadValue("Language", "XYLabel");
                screen1920x1080Bt.Text = english.IniReadValue("Language", "RibbonButton4");
                onBt.Text = english.IniReadValue("Language", "RibbonButton5");
                offBt.Text = english.IniReadValue("Language", "RibbonButton6");
                screencomputerBt.Text = english.IniReadValue("Language", "RibbonButton7");
                screen320x240Bt.Text = english.IniReadValue("Language", "RibbonButton8");
                screen426x240Bt.Text = english.IniReadValue("Language", "RibbonButton9");
                screendvdBt.Text = english.IniReadValue("Language", "RibbonButton31");
                screen1280x720Bt.Text = english.IniReadValue("Language", "RibbonButton32");
                monoBt.Text = english.IniReadValue("Language", "RibbonButton59");
                stereoBt.Text = english.IniReadValue("Language", "RibbonButton60");
                homethreadBt.Text = english.IniReadValue("Language", "RibbonButton64");
                screen720x576Bt.Text = english.IniReadValue("Language", "RibbonButton65");
                screen1440x816Bt.Text = english.IniReadValue("Language", "RibbonButton86");
                screen640x480Bt.Text = english.IniReadValue("Language", "RibbonButton87");
                screenoriginalBt.Text = english.IniReadValue("Language", "RibbonButton88");
                ribbonButton98.Text = english.IniReadValue("Language", "RibbonButton98");
                ribbonButton99.Text = english.IniReadValue("Language", "RibbonButton99");
                ribbonButton100.Text = english.IniReadValue("Language", "RibbonButton100");
                ribbonComboBox1.Text = english.IniReadValue("Language", "ribbonComboBox1_text");
                ribbonComboBox2.Text = english.IniReadValue("Language", "ribbonComboBox2_text");
                ribbonComboBox3.Text = english.IniReadValue("Language", "ribbonComboBox3_text");
                ribbonComboBox4.Text = english.IniReadValue("Language", "ribbonComboBox4_text");
                ribbonComboBox5.Text = english.IniReadValue("Language", "ribbonComboBox5_text");
                ribbonComboBox6.Text = english.IniReadValue("Language", "ribbonComboBox6_text");
                ribbonComboBox7.Text = english.IniReadValue("Language", "ribbonComboBox7_text");
                ribbonComboBox8.Text = english.IniReadValue("Language", "ribbonComboBox8_text");
                audiodisabledCb.Text = english.IniReadValue("Language", "RibbonCheckBox1_text");
                videodisabledCb.Text = english.IniReadValue("Language", "RibbonCheckBox2_text");
                originalcodecsCb.Text = english.IniReadValue("Language", "RibbonCheckBox3_text");
                audiocopyCb.Text = english.IniReadValue("Language", "AudioCopy_string");
                subtitleTb.Text = english.IniReadValue("Language", "subtitleTb_text");
                SubtitleBt.Text = english.IniReadValue("Language", "SubtitleBt_text");
                StartPozTb.Text = english.IniReadValue("Language", "StartPozTb_text");
                EndPozTb.Text = english.IniReadValue("Language", "EndPozTb_text");
                labelCb.Text = english.IniReadValue("Language", "RibbonCheckBox4_text");
                idv3Cb.Text = english.IniReadValue("Language", "RibbonCheckBox5_text");
                onlyaudioCb.Text = english.IniReadValue("Language", "RibbonCheckBox6_text");
                ribbonTextBox1.Text = english.IniReadValue("Language", "RibbonTextBox1_text");
                ribbonTextBox2.Text = english.IniReadValue("Language", "RibbonTextBox2_text");
                ribbonTextBox3.Text = english.IniReadValue("Language", "RibbonTextBox3_text");
                ribbonTextBox4.Text = english.IniReadValue("Language", "RibbonTextBox4_text");
                ribbonTextBox5.Text = english.IniReadValue("Language", "RibbonTextBox5_text");
                ribbonTextBox6.Text = english.IniReadValue("Language", "RibbonTextBox6_text");
                ribbonTextBox7.Text = english.IniReadValue("Language", "RibbonTextBox7_text");
                ribbonTextBox8.Text = english.IniReadValue("Language", "RibbonTextBox8_text");
                ribbonTextBox9.Text = english.IniReadValue("Language", "RibbonTextBox9_text");
                coverpathTb.Text = english.IniReadValue("Language", "coverpathTb_text");
                ribbonComboBox9.Text = english.IniReadValue("Language", "colorspace_text");
                InfoVidCodecsTb.Text = english.IniReadValue("Language", "InfoCodecs_text");
                InfoAudCodecsTb.Text = english.IniReadValue("Language", "InfoCodecs_text");
                InfoAud2CodecsTb.Text = english.IniReadValue("Language", "InfoCodecs_text");
                InfoVidBitrateTb.Text = english.IniReadValue("Language", "InfoBitrate_text");
                InfoAudBitrTb.Text = english.IniReadValue("Language", "InfoBitrate_text");
                InfoAud2BitrTb.Text = english.IniReadValue("Language", "InfoBitrate_text");
                InfoLenghtTb.Text = english.IniReadValue("Language", "InfoLenght_text");
                InfoSizeTb.Text = english.IniReadValue("Language", "InfoSize_text");
                InfoContainerTb.Text = english.IniReadValue("Language", "InfoContainer_text");
                InfoVidResTb.Text = english.IniReadValue("Language", "InfoVidRes_text");
                InfoVidArTb.Text = english.IniReadValue("Language", "InfoVidAr_text");
                InfoVidFpsTb.Text = english.IniReadValue("Language", "InfoVidFps_text");
                InfoVidStandTb.Text = english.IniReadValue("Language", "InfoVidStand_text");
                InfoVidColorTb.Text = english.IniReadValue("Language", "InfoVidColor_text");
                InfoVidFormatTb.Text = english.IniReadValue("Language", "InfoVidFormat_text");
                InfoAudSampleTb.Text = english.IniReadValue("Language", "InfoAudSample_text");
                InfoAud2SampleTb.Text = english.IniReadValue("Language", "InfoAudSample_text");
                InfoAudChanTb.Text = english.IniReadValue("Language", "InfoAudChannel_text");
                InfoAud2ChanTb.Text = english.IniReadValue("Language", "InfoAudChannel_text");
                listView1.Columns[0].Text = english.IniReadValue("Language", "ListviewName_text");
                listView1.Columns[1].Text = english.IniReadValue("Language", "ListviewSize_text");
                listView1.Columns[2].Text = english.IniReadValue("Language", "ListviewLenght_text");
                listView1.Columns[3].Text = english.IniReadValue("Language", "ListviewOutName_text");
                listView1.Columns[4].Text = english.IniReadValue("Language", "ListviewStartPos_text");
                listView1.Columns[5].Text = english.IniReadValue("Language", "ListviewEndPos_text");
                listView1.Columns[6].Text = english.IniReadValue("Language", "ListviewSub_text");
                listView1.Columns[7].Text = english.IniReadValue("Language", "ListviewAudio_text");
                ActualPosVidLb.Text = english.IniReadValue("Language", "ActualPosVid_text");
                autoshut.Text = english.IniReadValue("Language", "ConvertAfter_text");
                shutdownBT.Text = english.IniReadValue("Language", "Shutdown_text");
                rebootBt.Text = english.IniReadValue("Language", "Reboot_text");
                logoutBT.Text = english.IniReadValue("Language", "Logout_text");
                hibernateBt.Text = english.IniReadValue("Language", "Hibernate_text");
                sleepBt.Text = english.IniReadValue("Language", "Sleep_text");
                nothingBt.Text = english.IniReadValue("Language", "Nothing_text");
                ribbonOrbMenuItem5.Text = english.IniReadValue("Language", "About_text");
                CompressionTypeCb.Text = english.IniReadValue("Language", "CompressionType");
                JoinVideosLb.Text = english.IniReadValue("Language", "Join_text");
                LenghtVidLb.Text = english.IniReadValue("Language", "RibbonTextBox1_text");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            try
            {
                using (StreamReader reader = new StreamReader(english.path))
                {
                    reader.Close();
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("A fájl nem található!\n" + english.path, "Figyelem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ribbonButton98.PerformClick();
            }


        }

        private void ListDelStrip_Click(object sender, EventArgs e)
        {
            for (int i = listView1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listView1.Items.RemoveAt(listView1.SelectedIndices[i]);
                itemsCount();
            }
           
        }

        private void ListMoveUStrip_Click(object sender, EventArgs e)
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

        private void ListMoveDStrip_Click(object sender, EventArgs e)
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



        //picture resize
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private void ribbonButton100_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Képfájlok|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Minden fájl (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                coverpathTb.TextBoxText = openFileDialog1.FileName;
                Bitmap objBitmap = new Bitmap(System.Drawing.Image.FromFile(openFileDialog1.FileName), new Size(56, 56));
                ribbonButton100.Image = objBitmap;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            //listView1.BackgroundImage = null;
            MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int i;
            for (i = 0; i < s.Length; i++)
            {

                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = new FileInfo(Path.GetFullPath(s[i])).Length;
                int order = 0;
                while (len >= 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    len = len / 1024;
                }
                string result = String.Format("{0:0.##} {1}", len, sizes[order]);

                string fileName = Path.GetFullPath(s[i]);
                string fileExt = Path.GetExtension(s[i]).TrimStart('.');
                string outputName = Path.GetFileNameWithoutExtension(s[i]);
                string startEnd = "";
                string subtitlePath = "";
                string audioPath = "";
                MediaInfo MediaInfoLib = new MediaInfo();
                MediaInfoLib.Open(Path.GetFullPath(s[i]));
                string durationinfo = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String3");
                if (durationinfo == "")
                {
                    durationinfo = "N/A";
                }
                string[] row = { fileName, result, durationinfo, outputName, startEnd, startEnd, subtitlePath, audioPath };
                var listViewItem = new ListViewItem(row, _iconListManager.AddFileIcon(s[i]));
                listView1.Items.Add(listViewItem);
                //listView1.Items[i].Checked = true;
                if (fileExt == "mp3" || fileExt == "m4a" || fileExt == "ogg" || fileExt == "flac" || fileExt == "wma" || fileExt == "aac" || fileExt == "ra")
                {
                    onlyaudioCb.Checked = true;
                    if (onlyaudioCb.Checked == true)
                    {
                        disable_video = "";
                        VCodec = "";
                        SizeString = "";
                        VBitrate = "";
                        videoaspect = "";
                        videodisabledCb.Checked = true;
                        //ribbonButton66.Enabled = false;
                        //ribbonComboBox8.DropDownItems.ElementAt(2).Visible = false;
                        ribbonComboBox1.Enabled = false;
                        ribbonComboBox6.Enabled = false;
                        ribbonUpDown1.Enabled = false;
                        for (int j = 0; j < 20; j++)
                        {
                            ribbonComboBox8.DropDownItems.ElementAt(j).Visible = false;
                        }
                        ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(21);
                    }
                    else
                    {
                        videodisabledCb.Checked = false;
                        for (int j = 0; j < 20; j++)
                        {
                            ribbonComboBox8.DropDownItems.ElementAt(j).Visible = true;
                        }
                        ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(0);
                        ribbonComboBox1.Enabled = true;
                        ribbonComboBox6.Enabled = true;
                        ribbonUpDown1.Enabled = true;
                    }
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            itemsCount();
            if (listView1.SelectedItems.Count > 0)
            {
                // CalculatedSizeTb.Text = listView1.SelectedItems[0].SubItems[3].Text;
            }

            if (ribbonTab5.Active == true || ribbonTab4.Active == true)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    //listView1.BackgroundImage = null;
                    MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
                    string item = listView1.SelectedItems[0].Text;
                    MediaInfo MediaInfoLib = new MediaInfo();
                    MediaInfoLib.Open(item);
                    string[] sizes = { "B", "KB", "MB", "GB" };
                    double len = new FileInfo(item).Length;
                    int order = 0;
                    while (len >= 1024 && order + 1 < sizes.Length)
                    {
                        order++;
                        len = len / 1024;
                    }
                    string filesize = String.Format("{0:0.##} {1}", len, sizes[order]);

                    InfoLenghtTb.TextBoxText = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String3");
                    if (InfoLenghtTb.TextBoxText == "")
                    {
                        InfoLenghtTb.TextBoxText = "N/A";
                    }
                    InfoSizeTb.TextBoxText = filesize;
                    InfoContainerTb.TextBoxText = MediaInfoLib.Get(StreamKind.General, 0, "Format");
                    //General Lenght, Position
                    ribbonTextBox1.TextBoxText = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String");
                    //video
                    InfoVidCodecsTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "Codec");
                    InfoVidBitrateTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "BitRate/String");
                    InfoVidResTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "Width") + " X " + MediaInfoLib.Get(StreamKind.Video, 0, "Height");
                    InfoVidArTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
                    InfoVidFpsTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "FrameRate/String");
                    InfoVidStandTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "Standard");
                    InfoVidColorTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "ColorSpace");
                    InfoVidFormatTb.TextBoxText = MediaInfoLib.Get(StreamKind.Video, 0, "Format");
                    //Audio
                    InfoAudCodecsTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 0, "Format");
                    InfoAudBitrTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 0, "BitRate/String");
                    InfoAudSampleTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 0, "SamplingRate/String");
                    InfoAudChanTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 0, "Channel(s)/String");
                    //Audio2
                    InfoAud2CodecsTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 1, "Format");
                    InfoAud2BitrTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 1, "BitRate/String");
                    InfoAud2SampleTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 1, "SamplingRate/String");
                    InfoAud2ChanTb.TextBoxText = MediaInfoLib.Get(StreamKind.Audio, 1, "Channel(s)/String");
                }
            }

           
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            CPUProgressBar1.PerformStep();
            CPUProgressBar1.Value = (int)((decimal)performanceCounter1.NextValue());
            CPULabel.Text = "Cpu: " + CPUProgressBar1.Value.ToString() + "%";
        }

        private void SubtitleBt_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Srt, Ass, Sub Feliratok|*.srt;*.ass,*.sub|Minden fájl|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                subtitleTb.TextBoxText = openFileDialog1.FileName;
            }
        }

        private void ribbonOrbMenuItem5_Click(object sender, EventArgs e)
        {
            settings frmK = new settings();
            frmK.Controls.Add(frmK.aboutControl1);
            frmK.aboutControl1.Visible = true;
            frmK.listView1.Items[4].Selected = true;
            frmK.ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {

            logfrm logsfrm = new logfrm();
            if (File.Exists(temp + "vienna-encoder\\file-new.txt"))
            {
                String str = File.ReadAllText(temp + "vienna-encoder\\file-new.txt");
                logsfrm.InsertText(str);
            }

            logsfrm.ShowDialog();
        }

        private void shutdownBT_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = shutdownBT.SmallImage;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdownsl_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.reboot_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logout_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernate_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleep_icon;
            shutcommand = 1;
        }

        private void rebootBt_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = rebootBt.SmallImage;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdown_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.rebootsl_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logout_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernate_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleep_icon;
            shutcommand = 2;
        }

        private void logoutBT_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = logoutBT.SmallImage;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdown_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.reboot_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logoutsl_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernate_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleep_icon;
            shutcommand = 5;
        }

        private void hibernateBt_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = hibernateBt.SmallImage;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdown_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.reboot_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernatesl_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logout_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleep_icon;
            shutcommand = 4;
        }

        private void sleepBt_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = sleepBt.SmallImage;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdown_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.reboot_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logout_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernate_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleepsl_icon;
            shutcommand = 3;
        }

        private void nothingBt_Click(object sender, EventArgs e)
        {
            conveventIcon.Image = null;
            shutdownBT.SmallImage = ViennaEncoder.Properties.Resources.shutdown_icon;
            rebootBt.SmallImage = ViennaEncoder.Properties.Resources.reboot_icon;
            logoutBT.SmallImage = ViennaEncoder.Properties.Resources.logout_icon;
            hibernateBt.SmallImage = ViennaEncoder.Properties.Resources.hibernate_icon;
            sleepBt.SmallImage = ViennaEncoder.Properties.Resources.sleep_icon;
            shutcommand = 0;
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);

        private static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }


        private void LogBT_Click(object sender, EventArgs e)
        {
            logfrm logsfrm = new logfrm();
            if (File.Exists(temp + "vienna-encoder\\file-new.txt"))
            {
                String str = File.ReadAllText(temp + "vienna-encoder\\file-new.txt");
                logsfrm.InsertText(str);
            }

            logsfrm.ShowDialog();
        }
        private void OpenFiles()
        {
            openFileDialog1.Filter = "Videófájlok|*.avi;*.mkv;*.mp4;*.mpg;*.3g2;*.3gp;*.mov;*.vob;*.flv;*.mpeg;*.mjpeg;*.wmv;*.dav;*.ts|Hangfájlok|*.mp3;*.m4a;*.wma;*.wav;*.ogg;*.ac3;*.mp2|Minden fájl (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //listView1.BackgroundImage = null;
                MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
                // Read the files
                foreach (String file in openFileDialog1.FileNames)
                {
                    try
                    {
                        string[] sizes = { "B", "KB", "MB", "GB" };
                        double len = new FileInfo(file).Length;
                        int order = 0;
                        while (len >= 1024 && order + 1 < sizes.Length)
                        {
                            order++;
                            len = len / 1024;
                        }
                        string result = String.Format("{0:0.##} {1}", len, sizes[order]);

                        string fileName = Path.GetFullPath(file);
                        string fileExt = Path.GetExtension(file).TrimStart('.');
                        string outputName = Path.GetFileNameWithoutExtension(file);
                        string startEnd = "";
                        string subtitlePath = "";
                        string audioPath = "";
                        MediaInfo MediaInfoLib = new MediaInfo();
                        MediaInfoLib.Open(Path.GetFullPath(file));
                        string durationinfo = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String3");
                        if (durationinfo == "")
                        {
                            durationinfo = "N/A";
                        }

                        string[] row = { fileName, result, durationinfo, outputName, startEnd, startEnd, subtitlePath, audioPath };
                        var listViewItem = new ListViewItem(row, _iconListManager.AddFileIcon(file));
                        listView1.Items.Add(listViewItem);


                        if (fileExt == "mp3" || fileExt == "m4a" || fileExt == "ogg" || fileExt == "flac" || fileExt == "wma" || fileExt == "aac" || fileExt == "ra")
                        {
                            onlyaudioCb.Checked = true;
                            if (onlyaudioCb.Checked == true)
                            {
                                disable_video = "";
                                VCodec = "";
                                SizeString = "";
                                VBitrate = "";
                                videoaspect = "";
                                videodisabledCb.Checked = true;
                                //ribbonButton66.Enabled = false;
                                //ribbonComboBox8.DropDownItems.ElementAt(2).Visible = false;
                                ribbonComboBox1.Enabled = false;
                                ribbonComboBox6.Enabled = false;
                                ribbonUpDown1.Enabled = false;
                                for (int j = 0; j < 18; j++)
                                {
                                    ribbonComboBox8.DropDownItems.ElementAt(j).Visible = false;
                                }
                                ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(19);
                            }
                            else
                            {
                                videodisabledCb.Checked = false;
                                for (int j = 0; j < 18; j++)
                                {
                                    ribbonComboBox8.DropDownItems.ElementAt(j).Visible = true;
                                }
                                ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(0);
                                ribbonComboBox1.Enabled = true;
                                ribbonComboBox6.Enabled = true;
                                ribbonUpDown1.Enabled = true;
                            }
                        }

                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Nem nyithatóak meg a fájlok!" + ex.Message);
                    }

                    itemsCount();
                }
            }
        }
        private void sourceBrovseBT_Click(object sender, EventArgs e)
        {
            //OpenFiles();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!folderBrowserDialog1.SelectedPath.ToString().EndsWith(@"\"))
                {
                    TargetTb.Text = folderBrowserDialog1.SelectedPath.ToString() + @"\";
                }
                else
                {
                    TargetTb.Text = folderBrowserDialog1.SelectedPath.ToString();
                }

            }
        }

        private void stopBT_Click(object sender, EventArgs e)
        {
            pass = 0;
            Process[] prs = Process.GetProcesses();


            foreach (Process pr in prs)
            {
                if (pr.ProcessName == "ffmpeg")
                {

                    pr.Kill();
                    statusprocess = 1;
                    if (ini.IniReadValue("Settings", "EmptyListStopButton") == "true")
                    {
                        listView1.Items.Clear();
                    }
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        TaskDialog dlg = new TaskDialog("A konvertálást a felhasználó megszakította!", "Figyelmeztetés", "Konvertálás Kész (Megszakítva!)", TaskDialogButton.OK, TaskDialogIcon.Warning);
                        Results results = dlg.Show(this.Handle);
                    }
                    else
                    {
                        MessageBox.Show("A konvertálást a felhasználó megszakította!", "Figyelem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        StatusLabel.Text = "Kész (Megszakítva!)";
                    }

                }
                if (pr.ProcessName == "freaccmd")
                {
                    pr.Kill();
                    statusprocess = 1;
                }

            }
        }

        private void PauseBT_Click(object sender, EventArgs e)
        {

            Process[] p = Process.GetProcessesByName("ffmpeg");
            foreach (var proc in p)
                pid = proc.Id;
            if (PauseBT.Tag == "pause")
            {
                if (ini.IniReadValue("Settings", "Language") == "English")
                {
                    PauseBT.Text = english.IniReadValue("Language", "ResumeBT_text");
                }
                if (ini.IniReadValue("Settings", "Language") == "Hungarian")
                {
                    PauseBT.Text = hungarian.IniReadValue("Language", "ResumeBT_text");
                }
                //PauseBT.Text = "Folytat";
                PauseBT.Tag = "resume";
                SuspendProcess(pid);
                PauseBT.Image = ViennaEncoder.Properties.Resources.play_icon;
                if (ini.IniReadValue("Settings", "TaskbarControl") == "enable")
                {
                    thumbpauseButton.Icon = ViennaEncoder.Properties.Resources.Play_thumb;
                }

            }
            else if (PauseBT.Tag == "resume")
            {

                if (ini.IniReadValue("Settings", "Language") == "English")
                {
                    PauseBT.Text = english.IniReadValue("Language", "PauseBT_text");
                }
                if (ini.IniReadValue("Settings", "Language") == "Hungarian")
                {
                    PauseBT.Text = hungarian.IniReadValue("Language", "PauseBT_text");
                }
                //PauseBT.Text = "Szünet";
                PauseBT.Tag = "pause";
                ResumeProcess(pid);
                PauseBT.Image = ViennaEncoder.Properties.Resources.Pause_Normal;
                if (ini.IniReadValue("Settings", "TaskbarControl") == "enable")
                {
                    thumbpauseButton.Icon = ViennaEncoder.Properties.Resources.Pause_thumb;
                }
            }
        }


        private void helpOrbMenu_Click(object sender, EventArgs e)
        {
            //Helpform hlpfrm = new Helpform();
            //hlpfrm.Show();
            Process.Start("http://tandemradio.hu/vienna");

        }

        private void asceptcusBt_Click(object sender, EventArgs e)
        {
            if (customarXYTb.TextBoxText == "")
            {
                asceptcusBt.Checked = false;
            }
            else
            {
                videoaspect = "-aspect " + customarXYTb.TextBoxText + " ";
                asceptcusBt.Checked = true;
            }

        }

        private void asceptcusBt_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            if (customarXYTb.TextBoxText == "")
            {
                asceptcusBt.Checked = false;
            }
            else
            {
                videoaspect = "-aspect " + customarXYTb.TextBoxText + " ";
                asceptcusBt.Checked = true;
            }


        }

        private void asceptcusBt_DoubleClick(object sender, EventArgs e)
        {
            videoaspect = "";
            asceptcusBt.Checked = false;
        }

        private void customarXYTb_TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {

            // Check for a naughty character in the KeyDown event.
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^0-9^.^\:^]"))
            {
                // Stop the character from being entered into the control since it is illegal.
                e.Handled = true;
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            }
        }


        private void CusResBt_Click(object sender, EventArgs e)
        {
            CustomResLael.Enabled = true;
            ribbonItemGroup1.Enabled = true;
        }

        private void ribbonComboBox6_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            if (ribbonComboBox6.SelectedItem != screenCustomBt)
            {
                CustomResLael.Enabled = false;
                ribbonItemGroup1.Enabled = false;
                CusResXTb.Text = "";
                CusResYTb.Text = "";
            }
        }
        string rescustomX = "";
        string rescustomY = "";
        private void CusResXTb_TextBoxTextChanged(object sender, EventArgs e)
        {
            rescustomX = CusResXTb.TextBoxText;
            if (rescustomX != "")
            {
                screenCustomBt.Value = "-s " + rescustomX + "x" + rescustomY + " ";
                screenCustomBt.Text = "Egyéni Felbontás (" + rescustomX + "x" + rescustomY + ")";
                ribbonComboBox6.TextBoxText = "Egyéni Felbontás (" + rescustomX + "x" + rescustomY + ")";
                ribbonComboBox6.Value = "-s " + rescustomX + "x" + rescustomY + " ";
            }

        }

        private void CusResYTb_TextBoxTextChanged(object sender, EventArgs e)
        {
            rescustomY = CusResYTb.TextBoxText;
            if (rescustomY != "")
            {
                screenCustomBt.Value = "-s " + rescustomX + "x" + rescustomY + " ";
                screenCustomBt.Text = "Egyéni Felbontás (" + rescustomX + "x" + rescustomY + ")";
                ribbonComboBox6.TextBoxText = "Egyéni Felbontás (" + rescustomX + "x" + rescustomY + ")";
                ribbonComboBox6.Value = "-s " + rescustomX + "x" + rescustomY + " ";
            }


        }

//Konvertálás kilépése!!
        void myProcess_Exited(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (coverpathTb.TextBoxText != "")
                {
                    string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);
                    //Album Borító MP4
                    if (ribbonComboBox8.TextBoxText == "MP4" || ribbonComboBox8.TextBoxText == "M4A")
                    {
                        if (ribbonComboBox8.Enabled == true)
                        {
                            Process AtomicParsleyProcess = new Process();
                            AtomicParsleyProcess.StartInfo.FileName = "cmd.exe";
                            AtomicParsleyProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                            AtomicParsleyProcess.StartInfo.Arguments = "/C Codecs\\AtomicParsley.exe \"" + actualoutputfilename + "\"" + " --artwork " + "\"" + coverpathTb.TextBoxText + "\"" + " --overWrite";

                            AtomicParsleyProcess.StartInfo.CreateNoWindow = true;
                            AtomicParsleyProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                            AtomicParsleyProcess.Start();
                            AtomicParsleyProcess.WaitForExit();
                        }
                    }
                    if (ribbonComboBox8.TextBoxText == "MP3")
                    {
                        Process metamp3Process = new Process();
                        metamp3Process.StartInfo.FileName = "cmd.exe";
                        metamp3Process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                        metamp3Process.StartInfo.Arguments = "/C Codecs\\metamp3.exe" + " --pict " + "\"" + coverpathTb.TextBoxText + "\" " + "\"" + actualoutputfilename + "\"";

                        metamp3Process.StartInfo.CreateNoWindow = true;
                        metamp3Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        metamp3Process.Start();
                        metamp3Process.WaitForExit();
                    }
                }
                if (listView1.Items.Count > 0 && listView1.Items[0].SubItems[6].Text != "")
                {
                    string fullPath = TargetTb.Text + listView1.Items[0].SubItems[3].Text + Extension;
                        string newFullPath = fullPath;
                        string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);

                        int count = 1;
                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                            newFullPath = Path.Combine(TargetTb.Text, tempFileName + ".mkv");
                        }
                        SubtitleMerge(newFullPath, fullPath, listView1.Items[0].SubItems[6].Tag.ToString(), listView1.Items[0].SubItems[6].Text, 0);
                        progressBar1.Maximum = 100;
                }
                if (listView1.Items.Count > 0 && listView1.Items[0].SubItems[7].Text != "")
                {
                    string fullPath = TargetTb.Text + "\\" + listView1.Items[0].SubItems[3].Text + Extension;
                        string newFullPath = fullPath;
                        string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);

                        int count = 1;
                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                            newFullPath = Path.Combine(TargetTb.Text, tempFileName + ".mkv");
                        }
                        SubtitleMerge(newFullPath, fullPath, listView1.Items[0].SubItems[7].Tag.ToString(), listView1.Items[0].SubItems[7].Text, 0);
                        progressBar1.Maximum = 100;
                }
                alltimes += elapsedTime.Elapsed;
                elapsedTime.Reset();
                if (JoinVideosCb.Checked == true)
                {
                    string fullPath = TargetTb.Text + listView1.Items[0].SubItems[3].Text + Extension;
                    //Directory.CreateDirectory(temp + "\\vienna-encoder");
                    TempFolderCreate();
                    StreamWriter file = new StreamWriter(temp + "\\vienna-encoder\\fileList.txt",true);
                    file.WriteLine("file '" + fullPath + "'");
                    file.Close();
                    
                }
                if (listView1.Items.Count > 0)
                    {
                        if (pass == 2)
                        {
                            listView1.Items.RemoveAt(0);
                            pass = 0;
                        }
                        if (CompressionTypeCb.Checked == false)
                        {
                            listView1.Items.RemoveAt(0);
                        }
                        int pr2 = progressbar2value;
                        progressbar2value = pr2 + 100;
                        
                    }

                    if (listView1.Items.Count > 0)
                        {
                            //Button Click
                            statusprocess = 0;
                            //listcountper++;
                            //Directory.CreateDirectory(temp + "\\vienna-encoder");
                            TempFolderCreate();
                            parametersSet();
                            ffmpegStart();
                            
                        }
                
                
                else
                {
                    timer1.Stop();
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Increment(0);
                    //progressBar1.Value = 0;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        if (Environment.OSVersion.Version.Minor >= 1)
                        {
                            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                            prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                        }
                    }

                    //Kész!------------------------------------------
                    //listcount = 0;
                    //listcountper = 0;
                    label4.Text = "100%";
                    RT.StopAndReset();
                    elapsedTime.Stop();
                    progressbar2state = false;
                    progressBar2.Value = progressBar2.Maximum;

                    if (JoinVideosCb.Checked == true && listView1.Items.Count == 0)
                    {
                        progressBar1.Style = ProgressBarStyle.Marquee;
                        StatusLabel.Text = "Videók egyesítése...";
                        VideoJoin();
                    }

                    string waitingMerge = "";

                    if (shutcommand == 0)
                    {
                        if (statusprocess == 0)
                        {
                            if (waitingVideoMergeEnd == true)
                            {
                                waitingMerge = "\nDe a Videó vagy Hangfáj egyesítés folyamatban";
                            }
                            string last = File.ReadAllLines(temp + "vienna-encoder\\out.txt").Last();
                            if (last.Contains("No such file or directory"))
                            {
                                errorcode = "Nem található kimeneti fájl vagy a könyvtár! Ellenőrizd a Kimeneti mappát!";
                            }
                            if (last.Contains("At least one output") || last.Contains("Error number") || last.Contains("Invalid argument"))
                            {
                                errorcode = last;
                            }
                            if (last.Contains("Permission denied"))
                            {
                                errorcode = "Nincs Rendszergazdai jog a mappa hozzáféréséhez \n(Adj meg másik kimeneti mappát)!";
                            }
                            string lines = "";
                            using (System.IO.StreamReader file = new System.IO.StreamReader(temp + "vienna-encoder\\out.txt"))
                            {
                                while ((lines = file.ReadLine()) != null)
                                {
                                    if (lines.Contains("Error setting") || lines.Contains("Unknown") || lines.Contains("Unrecognized"))
                                    {
                                        errorcode = lines;
                                    }
                                }
                            }
                            StatusLabel.Text = "Kész!";
                            progressbar2state = false;

                            EndPozTb.TextBoxText = "";
                            StartPozTb.TextBoxText = "";
                            StartVideos = "";
                            EndVideos = "";

                            if (errorcode == "")
                            {
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    string hours = "";
                                    string minutes = "";
                                    if (alltimes.Hours > 0) 
                                    {
                                        hours = alltimes.Hours + " óra ";
                                    }
                                    if (alltimes.Minutes > 0)
                                    {
                                        minutes = alltimes.Minutes + " perc ";
                                    }
                                    TaskDialog dlg = new TaskDialog("Konvertálás Kész!", "Információ", "A konvertálás befejeződött " + hours + minutes + alltimes.Seconds + " másodperc" + " alatt." + waitingMerge, TaskDialogButton.OK, TaskDialogIcon.SecuritySuccess);
                                    Results results = dlg.Show(this.Handle);
                                }
                                else
                                {
                                    MessageBox.Show("Konvertálás Kész!" + waitingMerge, "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }

                            }
                            else
                            {

                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    TaskDialog dlg = new TaskDialog("Konvertálás hibával Befejeződött!", "Hiba");
                                    dlg.CustomButtons = new CustomButton[] { new CustomButton(10, "Képeryőmentés"), new CustomButton(Result.OK, "OK") };
                                    dlg.CommonIcon = TaskDialogIcon.SecurityError;
                                    dlg.Content = "Valamilyen hiba történt konvertáláskor részletekért kattints a \"További információ\" gombra." + waitingMerge;
                                    //Evt registration
                                    dlg.ExpandedControlText = "További információ";
                                    dlg.ExpandedInformation = errorcode;
                                    dlg.ButtonClick += new EventHandler<ClickEventArgs>(PrintScreenEvent);
                                    Results results = dlg.Show(this.Handle);
                                }
                                else
                                {
                                    MessageBox.Show(errorcode + waitingMerge, "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            alltimes = elapsedTime.Elapsed;
                            progressBar1.Maximum = 100;
                            progressBar1.Value = 0;
                            label4.Text = "0%";
                            if (Environment.OSVersion.Version.Major >= 6)
                            {
                                if (Environment.OSVersion.Version.Minor >= 1)
                                {
                                    var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                                    prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                                    prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                                }
                            }
                            
                        }

                    }
                    coverpathTb.TextBoxText = "";
                    this.ribbonButton100.Image = global::ViennaEncoder.Properties.Resources.cover;
                    if (ini.IniReadValue("Settings", "Debug") == "true")
                    {
                        string documents = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        File.Copy(temp + "\\vienna-encoder\\out.txt", documents + "\\ViennaOut.txt", true);
                        StreamReader sr = new StreamReader(temp + "\\vienna-encoder\\commands.txt");
                        File.AppendAllText(documents + "\\ViennaOut.txt", sr.ReadLine());
                        sr.Close();
                    }


                        if (Directory.Exists(temp + "\\vienna-encoder") && waitingVideoMergeEnd == false)
                        {
                            Directory.Delete(temp + "\\vienna-encoder", true);
                        }
                 


                    if (statusprocess == 0)
                    {
                        switch (shutcommand)
                        {
                            case 1:
                                Process.Start(system + "\\shutdown", "/s /t 0 /f");
                                break;
                            case 2:
                                Process.Start(system + "\\shutdown", "/r /t 0 /f");
                                break;
                            case 3:
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    Process.Start("powercfg", "-hibernate off");
                                    Thread.Sleep(50);
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                                    Thread.Sleep(150);
                                    Process.Start("powercfg", "-hibernate on");
                                }
                                else
                                {
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState Hibernate");
                                }
                                break;
                            case 4:
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    Process.Start("powercfg", "-hibernate on");
                                    Thread.Sleep(50);
                                    Process.Start(system + "\\shutdown", "/h");
                                }
                                else
                                {
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState Sleep");
                                }
                                break;
                            case 5:
                                Process.Start("rundll32.exe", "User32.dll,LockWorkStation");
                                break;
                            default:
                                break;
                        }
                    }


                }
            });
        }
        void PrintScreenEvent(object sender, ClickEventArgs e)
        {
            if (e.ButtonID == 10)
            {
                Rectangle bounds = this.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                    }
                    bitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\FFMpegVienna" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".png", ImageFormat.Png);
                }
            }
        }



        string VideoPositionString;
        string lenghtString;
        TimeSpan positionactual = new TimeSpan();

        private delegate void UpdateTextDelegate();
        private void OnVlcPositionChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs e)
        {
            var position = vmpl.GetMedia().Duration.Ticks * e.NewPosition;
            VideoPositionString = new DateTime((long)position).ToString("T");
            actualPoz.BeginInvoke(new UpdateTextDelegate(UpdateText));

           // _currentTime = Convert.ToInt32(position)/1024/1024/10;
        }
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


        int startvideo = 0;
        int endvideo = 0;

        public class MySR : ToolStripSystemRenderer
        {
            public MySR() { }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                //base.OnRenderToolStripBorder(e);
            }
        }
        private void vidCutS_Click(object sender, EventArgs e)
        {
            ribbon1.ActiveTab = ribbonTab4;
            StartPozTb.TextBoxText = VideoPositionString;
            
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].SubItems[4].Text = StartPozTb.TextBoxText;
                startvideo = this._currentTime;
            }
        }

        private void vidCutE_Click(object sender, EventArgs e)
        {
            ribbon1.ActiveTab = ribbonTab4;
            EndPozTb.TextBoxText = VideoPositionString;
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].SubItems[5].Text = EndPozTb.TextBoxText;
                endvideo = this._currentTime;
            }
        }
        //Összegzés...
        private void osszegzesvideovago()
        {
            TimeSpan startosszeg = new TimeSpan();
            TimeSpan endosszeg = new TimeSpan();
            int startTime;
            int endTime;

            if (listView1.SelectedItems[0].SubItems[4].Text != "" && listView1.SelectedItems[0].SubItems[5].Text != "")
            {
                startosszeg = TimeSpan.Parse(listView1.SelectedItems[0].SubItems[4].Text);
                startTime = startosszeg.Hours * 3600 + startosszeg.Minutes * 60 + startosszeg.Seconds;
                endosszeg = TimeSpan.Parse(listView1.SelectedItems[0].SubItems[5].Text);
                endTime = endosszeg.Hours * 3600 + endosszeg.Minutes * 60 + endosszeg.Seconds;
                int osszes = endTime - startTime;
                listView1.SelectedItems[0].SubItems[5].Tag = osszes;
            }
            if (listView1.SelectedItems[0].SubItems[5].Text == "")
            {
                TimeSpan positionended = new TimeSpan();
                positionended = TimeSpan.Parse(vidLenght.Text);
                endvideo = positionended.Hours * 3600 + positionended.Minutes * 60 + positionended.Seconds;
                int osszes = endvideo - startvideo;
                listView1.SelectedItems[0].SubItems[5].Tag = osszes;
            }
            else if (listView1.SelectedItems[0].SubItems[4].Text == "")
            {
                int osszes = endvideo;
                listView1.SelectedItems[0].SubItems[5].Tag = osszes;
            }
            
        }
        private void vidPlay_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                filePath = listView1.SelectedItems[0].SubItems[0].Text;
                FileInfo file = new FileInfo(filePath);
                MediaInfo MediaInfoLib = new MediaInfo();
                MediaInfoLib.Open(filePath);

                vmpl.SetMedia(file);
                vmpl.Play();

            }
        }

        private void vidPause_Click(object sender, EventArgs e)
        {
                vmpl.Pause();
        }

        private void vidStop_Click(object sender, EventArgs e)
        {
            vmpl.Stop();
            trackBar1.Value = 0;
        }

        private void vidRew_Click(object sender, EventArgs e)
        {
            int currtrackvalue = trackBar1.Value;
            if (currtrackvalue > 2)
            {
                trackBar1.Value = currtrackvalue - 2;
            }
        }

        private void vidFF_Click(object sender, EventArgs e)
        {
            int currtrackvalue = trackBar1.Value;
            if (currtrackvalue < trackBar1.Maximum)
            {
                trackBar1.Value = currtrackvalue + 2;
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem listViewItem in listView1.SelectedItems)
                {
                    listViewItem.Remove();
                    //countLabel.Text = listView1.Items.Count + " elem";
                    itemsCount();
                }
            }
        }
        ListViewItem.ListViewSubItem SelectedLSI;
        private void HideTextEditor()
        {
            bool isWhitespace = TxtEdit.Text.All(char.IsWhiteSpace);
            if (TxtEdit.Text != "" && isWhitespace == false)
            {
                String result = Regex.Replace(TxtEdit.Text, "^[ \t\r\n]+|[ \t\r\n]+$", "");
                TxtEdit.Text = result;
            }
            TxtEdit.Visible = false;
            if (SelectedLSI != null)
                SelectedLSI.Text = TxtEdit.Text;
            SelectedLSI = null;

        }

        private void TxtEdit_Leave(object sender, EventArgs e)
        {
            HideTextEditor();

        }

        private void TxtEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                HideTextEditor();
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
           
            HideTextEditor();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MouseEventArgs mea = (MouseEventArgs)e;
            if (mea != null)
            {
                int mousex = mea.X;
                int x = 0;
                int subitemindex = 0;
                for (int i = 0; i < listView1.Columns.Count; i++)
                {
                    x += listView1.Columns[i].Width;
                    subitemindex = i;
                    if (mousex < x)
                    {
                        break;
                    }
                }
                if (subitemindex == 3)
                {
                    ListViewHitTestInfo i = listView1.HitTest(e.X, e.Y);
                    SelectedLSI = i.SubItem;
                    if (SelectedLSI == null)
                        return;

                    int border = 0;
                    switch (listView1.BorderStyle)
                    {
                        case BorderStyle.FixedSingle:
                            border = 1;
                            break;
                        case BorderStyle.Fixed3D:
                            border = 2;
                            break;
                    }

                    int CellWidth = SelectedLSI.Bounds.Width;
                    int CellHeight = SelectedLSI.Bounds.Height;
                    int CellLeft = border + listView1.Left + i.SubItem.Bounds.Left;
                    int CellTop = listView1.Top + i.SubItem.Bounds.Top;
                    // First Column
                    if (i.SubItem == i.Item.SubItems[0])
                        CellWidth = listView1.Columns[0].Width;

                    TxtEdit.Location = new Point(CellLeft, CellTop);
                    TxtEdit.Size = new Size(CellWidth, CellHeight);
                    TxtEdit.Visible = true;
                    TxtEdit.BringToFront();
                    TxtEdit.Text = i.SubItem.Text;
                    TxtEdit.Select();
                    TxtEdit.SelectAll();

                }

            }
        }

        private void SubtitleApplyBt_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (subtitleTb.TextBoxText != "")
                {
                    listView1.SelectedItems[0].SubItems[6].Text = subtitleTb.TextBoxText;
                    listView1.SelectedItems[0].SubItems[6].Tag = "--language " + SubtitleTrackID.Value.ToString() + ":" + SubtitleLang.Value.ToString() + " --track-name " + SubtitleTrackID.Value.ToString() + ":" + SubtitleTrackName.TextBoxText + otherCommandMergeSub.TextBoxText;
                    subtitleTb.TextBoxText = "";
                    suboraudiomessenge = "Felirat(ok)";
                }
            }
        }
        private void SubtitleMerge(string output, string input, string command, string subpath, int progressevent)
        {
            try
            {
                TempFolderCreate();
                Process SubMergeProcess = new Process();
                SubMergeProcess.StartInfo.FileName = "cmd.exe";
                SubMergeProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                SubMergeProcess.StartInfo.Arguments = "/c Codecs\\mkvmerge.exe -o \"" + output + "\"" + " \"" + input + "\" " + command + " \"" + subpath + "\"" + " 1>" + "\"" + temp + "\\vienna-encoder\\submerge.txt" + "\""; ;
                SubMergeProcess.StartInfo.UseShellExecute = false;
                SubMergeProcess.StartInfo.CreateNoWindow = true;
                SubMergeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (progressevent == 1)
                {
                    SubMergeProcess.EnableRaisingEvents = true;
                    SubMergeProcess.Exited += new EventHandler(SubMergeProcess_Exited);
                    timer3.Enabled = true;
                    timer3.Start();
                }
                SubMergeProcess.Start();
                if (progressevent == 0)
                {
                    SubMergeProcess.EnableRaisingEvents = true;
                    SubMergeProcess.WaitForExit();
                }
                //SubMergeProcess.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        private void VideoJoin()
        {
            saveFileDialog1.FileName = "";
            // set filters - this can be done in properties as well
            saveFileDialog1.Filter = ribbonComboBox8.TextBoxText + "|*" + ribbonComboBox8.SelectedValue + "|Minden fájl (*.*)|*.*";
            string filename = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = saveFileDialog1.FileName;
            }

            //Konvertáló program indítása Egyesítés---------------------------------------------------------------
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            myProcess.StartInfo.Arguments = "/C Codecs\\FFMpeg\\ffmpeg.exe " + "-f concat -i \"" + temp + "vienna-encoder\\fileList.txt" + "\" " + "-c copy \"" + filename + "\"";
            if (ini.IniReadValue("Settings", "HideCMD") == "true")
            {
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            myProcess.EnableRaisingEvents = true;
            myProcess.Exited += new EventHandler(VideoJoin_Exited);
            myProcess.Start();
            waitingVideoMergeEnd = true;
        }

         void VideoJoin_Exited(object sender, EventArgs e)
        {
           
            this.Invoke((MethodInvoker)delegate
            {
                waitingVideoMergeEnd = false;
                if (Directory.Exists(temp + "\\vienna-encoder") && waitingVideoMergeEnd == false)
                {
                    Directory.Delete(temp + "\\vienna-encoder", true);
                }
                progressBar1.Style = ProgressBarStyle.Blocks;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    TaskDialog dlg = new TaskDialog("Egyesítés Kész!", "Információ", "A Videó/Audió egyesítés befejeződött! ", TaskDialogButton.OK, TaskDialogIcon.SecuritySuccess);
                    Results results = dlg.Show(this.Handle);
                }
                else
                {
                    MessageBox.Show("Egyesítés Kész!!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (listView1.Items.Count >0)
                {
                    listView1.Clear();
                    progressBar1.Value = 0;
                }
            });
        }

        void SubMergeProcess_Exited(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (listView1.Items.Count > 0)
                {
                    listView1.Items.RemoveAt(0);
                }
                if (listView1.Items.Count > 0)
                {
                    SubtitleMergeButton.PerformClick();
                }
                else
                {
                    timer3.Stop();
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Increment(100);
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        if (Environment.OSVersion.Version.Minor >= 1)
                        {
                            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                            prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                        }
                    }

                    //Kész!------------------------------------------
                    //listcount = 0;
                    //listcountper = 0;
                    label4.Text = "100%";
                    if (shutcommand == 0)
                    {
                        if (statusprocess == 0)
                        {
                            if (Environment.OSVersion.Version.Major >= 6)
                            {
                                TaskDialog dlg = new TaskDialog(suboraudiomessenge + " fésülése Kész!", "Információ", "A " + suboraudiomessenge + " sikeresen beletetted a videó(k)ba.", TaskDialogButton.OK, TaskDialogIcon.SecuritySuccess);
                                Results results = dlg.Show(this.Handle);
                            }
                            else
                            {
                                MessageBox.Show(suboraudiomessenge + " fésülése Kész!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                    }
                    //Directory.Delete(temp + "\\vienna-encoder", true);
                    if (statusprocess == 0)
                    {
                        switch (shutcommand)
                        {
                            case 1:
                                Process.Start(system + "\\shutdown", "/s /t 0 /f");
                                break;
                            case 2:
                                Process.Start(system + "\\shutdown", "/r /t 0 /f");
                                break;
                            case 3:
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    Process.Start("powercfg", "-hibernate off");
                                    Thread.Sleep(50);
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                                    Thread.Sleep(150);
                                    Process.Start("powercfg", "-hibernate on");
                                }
                                else
                                {
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState Hibernate");
                                }
                                break;
                            case 4:
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    Process.Start("powercfg", "-hibernate on");
                                    Thread.Sleep(50);
                                    Process.Start(system + "\\shutdown", "/h");
                                }
                                else
                                {
                                    Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState Sleep");
                                }
                                break;
                            case 5:
                                Process.Start("rundll32.exe", "User32.dll,LockWorkStation");
                                break;
                            default:
                                break;
                        }
                    }


                }
            });
        }

        private void SubtitleMergeButton_Click(object sender, EventArgs e)
        {
            statusprocess = 0;
            //listcountper++;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[0].Selected = true;
                listView1.Select();
            }
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].SubItems[6].Text != "")
                {
                    string fullPath = TargetTb.Text + listView1.SelectedItems[0].SubItems[3].Text + ".mkv";
                    string newFullPath = fullPath;
                    string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);
                    string inputfilename = listView1.SelectedItems[0].SubItems[0].Text;
                    if (FileExtCB.SelectedIndex == 2)
                    {
                        int count = 1;
                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                            newFullPath = Path.Combine(TargetTb.Text, tempFileName + Extension);
                        }
                    }
                    if (FileExtCB.SelectedIndex == 3)
                    {
                        string dates = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                        newFullPath = Path.Combine(TargetTb.Text, dates + Extension);

                    }
                    SubtitleMerge(newFullPath, inputfilename, listView1.SelectedItems[0].SubItems[6].Tag.ToString(), listView1.SelectedItems[0].SubItems[6].Text, 1);
                    progressBar1.Maximum = 100;
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            File.Copy(temp + @"\vienna-encoder\submerge.txt", temp + @"\vienna-encoder\submerge2.txt", true);

            String last = "";
            if (new FileInfo(temp + @"\vienna-encoder\submerge2.txt").Length > 0)
            {
                last = System.IO.File.ReadAllLines(temp + @"\vienna-encoder\submerge2.txt").Last();
                convertInfoLabel.Text = last;
            }

            if (last.Contains("Progress:"))
            {
                if (progressBar1.Style != ProgressBarStyle.Marquee)
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        if (Environment.OSVersion.Version.Minor >= 1)
                        {
                            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                            prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                        }
                    }
                    string justNumbers = new String(last.Where(Char.IsDigit).ToArray());

                    int percent = int.Parse(justNumbers);

                    label4.Text = justNumbers + "%";
                    progressBar1.Value = percent;

                }

            }

        }


        private void EndPozTb_TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].SubItems[5].Text = EndPozTb.TextBoxText;
            }
        }

        private void StartPozTb_TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].SubItems[4].Text = StartPozTb.TextBoxText;
            }
        }

        private void StartPozTb_TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            // Check for a naughty character in the KeyDown event.
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^0-9^.^\:^]"))
            {
                // Stop the character from being entered into the control since it is illegal.
                e.Handled = true;
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            }
        }

        private void EndPozTb_TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            // Check for a naughty character in the KeyDown event.
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^0-9^.^\:^]"))
            {
                // Stop the character from being entered into the control since it is illegal.
                e.Handled = true;
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            }
        }

        private void FileAddContextMenu_Click(object sender, EventArgs e)
        {
            ribbonOrbMenuItem6.PerformClick();
        }

        private void FileRemoveContextMenu_Click(object sender, EventArgs e)
        {
            for (int i = listView1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listView1.Items.RemoveAt(listView1.SelectedIndices[i]);
            }
        }

        private void TargetFolderContextMenu_Click(object sender, EventArgs e)
        {
            if (TargetTb.Text != "")
            {
                Process.Start("explorer.exe", TargetTb.Text);
            }

        }

        private void audioMergeBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Támogatott Hangfájlok|*.mp3;*.aac;*.ac3;*.m4a;*.ogg|Minden fájl|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                audioMergePath.TextBoxText = openFileDialog1.FileName;
            }
        }

        private void audioMergeStart_Click(object sender, EventArgs e)
        {
            statusprocess = 0;
            //listcountper++;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[0].Selected = true;
                listView1.Select();
            }
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].SubItems[7].Text != "")
                {
                    string fullPath = TargetTb.Text + listView1.SelectedItems[0].SubItems[3].Text + ".mkv";
                    string newFullPath = fullPath;
                    string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);
                    string inputfilename = listView1.SelectedItems[0].SubItems[0].Text;
                    if (FileExtCB.SelectedIndex == 2)
                    {
                        int count = 1;
                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                            newFullPath = Path.Combine(TargetTb.Text, tempFileName + Extension);
                        }
                    }
                    if (FileExtCB.SelectedIndex == 3)
                    {
                        string dates = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                        newFullPath = Path.Combine(TargetTb.Text, dates + Extension);

                    }
                    SubtitleMerge(newFullPath, inputfilename, listView1.SelectedItems[0].SubItems[7].Tag.ToString(), listView1.SelectedItems[0].SubItems[7].Text, 1);
                    progressBar1.Maximum = 100;
                }
            }
        }

        private void audioMergeApply_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (audioMergePath.TextBoxText != "")
                {
                    listView1.SelectedItems[0].SubItems[7].Text = audioMergePath.TextBoxText;
                    listView1.SelectedItems[0].SubItems[7].Tag = "--language " + audioMergeTrackID.Value.ToString() + ":" + audioMergeLang.Value.ToString() + " --track-name " + audioMergeTrackID.Value.ToString() + ":" + audioMergeTrackName.TextBoxText + otherCommandMergeAudio.TextBoxText;
                    audioMergePath.TextBoxText = "";
                    suboraudiomessenge = "Audiósáv(ok)";
                }
            }
        }

//Konvertálás!!
        public void ffmpegStart()
        {
            try
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[0].Selected = true;
                    listView1.Select();
                }
                string fileName = Path.GetFileName(listView1.SelectedItems.ToString());
                string fileFull = listView1.Items[0].Text;
                string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);
                //Ha van vágás
                osszegzesvideovago();
                //Információ gyűjtése
                if (listView1.Items[0].SubItems[2].Text != "N/A" )
                {
                    //Tized másodperc elhagyása
                    string s2 = Regex.Replace(listView1.Items[0].SubItems[2].Text, ".\\d{3}$", "");
                    LenghtTB.Text = s2;
                    var values = Regex.Split(s2, @"[\s,:]+");
                    if (s2 != "N/A")
                    {
                        int ora = int.Parse(values[0]);
                        int perc = int.Parse(values[1]);
                        int mp = int.Parse(values[2]);
                        int osszeg = ora * 3600 + 60 * perc + mp;
                        if (listView1.SelectedItems[0].SubItems[5].Text == "")
                        {
                            progressBar1.Maximum = osszeg;
                            RT.TargetValue = osszeg;
                        }
                        else
                        {
                            progressBar1.Maximum = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                            RT.TargetValue = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                        }

                    }
                    //ismeretlen hártalévő idő
                    else
                    {
                        progressBar1.Style = ProgressBarStyle.Marquee;
                        progressBar1.MarqueeAnimationSpeed = 50;
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            if (Environment.OSVersion.Version.Minor >= 1)
                            {
                                var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                                prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Indeterminate);
                            }

                        }
                        label4.Text = "N/A";
                    }

                }
                else
                {
                    Process ffprobeProcess = new Process();
                    ffprobeProcess.StartInfo.FileName = "cmd.exe";
                    ffprobeProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                    ffprobeProcess.StartInfo.Arguments = "/C Codecs\\FFMpeg\\ffprobe.exe -show_streams -i \"" + fileFull + "\"" + " 2>" + "\"" + temp + "\\vienna-encoder\\info.txt" + "\"";

                    ffprobeProcess.StartInfo.CreateNoWindow = true;
                    ffprobeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    ffprobeProcess.Start();
                    ffprobeProcess.WaitForExit();


                    foreach (string duration in System.IO.File.ReadAllLines(temp + @"\vienna-encoder\info.txt"))
                    {
                        if (duration.Contains("Duration:"))
                        {
                            string pat = @"\: ([^,]*)";
                            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                            Match m = r.Match(duration);
                            //Az idő feldarabolása :)
                            string duration2 = m.ToString();
                            char[] arr = new char[] { ':', ' ' };
                            duration2 = duration2.TrimStart(arr);
                            //Tized másodperc elhagyása
                            string s2 = Regex.Replace(duration2, ".\\d{2}$", "");
                            //textBox3.Text = s2;
                            LenghtTB.Text = s2;
                            var values = Regex.Split(s2, @"[\s,:]+");
                            if (s2 != "N/A")
                            {
                                int ora = int.Parse(values[0]);
                                int perc = int.Parse(values[1]);
                                int mp = int.Parse(values[2]);
                                int osszeg = ora * 3600 + 60 * perc + mp;
                                if (listView1.SelectedItems[0].SubItems[5].Text == "")
                                {
                                    progressBar1.Maximum = osszeg;
                                    RT.TargetValue = osszeg;
                                }
                                else
                                {
                                    progressBar1.Maximum = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                                    RT.TargetValue = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                                }

                            }
                            //ismeretlen hártalévő idő
                            else
                            {
                                progressBar1.Style = ProgressBarStyle.Marquee;
                                progressBar1.MarqueeAnimationSpeed = 50;
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    if (Environment.OSVersion.Version.Minor >= 1)
                                    {
                                        var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                                        prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Indeterminate);
                                    }

                                }

                                label4.Text = "N/A";
                            }

                        }
                    }
                }


                //start end poz
                if (listView1.SelectedItems[0].SubItems[4].Text != "")
                {
                    StartVideos = "-ss " + listView1.SelectedItems[0].SubItems[4].Text + ".000 ";
                }
                if (listView1.SelectedItems[0].SubItems[5].Text != "")
                {
                    EndVideos = "-t " + listView1.SelectedItems[0].SubItems[5].Tag + " ";
                }
                Process[] mprs = Process.GetProcesses();
                foreach (Process mpr in mprs)
                {
                    vmpl.Stop();
                }
                string fullPath = TargetTb.Text + listView1.SelectedItems[0].SubItems[3].Text + Extension;
                string newFullPath = fullPath;
                if (FileExtCB.SelectedIndex == 2)
                {
                    int count = 1;
                    while (File.Exists(newFullPath))
                    {
                        string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                        newFullPath = Path.Combine(TargetTb.Text, tempFileName + Extension);
                    }
                }
                if (FileExtCB.SelectedIndex == 3)
                {
                    string dates = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                    newFullPath = Path.Combine(TargetTb.Text, dates + Extension);

                }
                string macska = "\"";
                if (pass == 1)
	            {
		           newFullPath = "";
                   macska = "";
	            }
                
                //textBox2.Visible = true;
                //textBox2.Text = StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + "-x264-params " + tuning + EndVideos + copyvideocodecs + VCodec + copyaudiocodecs + SizeString + fps + colorspace + VBitrate + videoaspect + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + openclx264 + openclx265 + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + "\"" + newFullPath + "\"";
                File.WriteAllText(temp + "\\vienna-encoder\\commands.txt", StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + tuning + Flagsx264 + Threads + x264Profile + x264ProfileLevel + EndVideos + VCodec + copyaudiocodecs + SizeString + vidminrate + vidmaxrate + vidRotate + fps + colorspace + VBitrate + videoaspect + pass1 + pass2 + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + atempo + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + openclx264 + openclx265 + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + macska + newFullPath + macska);
                //Konvertáló program indítása---------------------------------------------------------------
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = "cmd.exe";
                myProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                myProcess.StartInfo.Arguments = "/C Codecs\\FFMpeg\\ffmpeg.exe " + StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + tuning + Flagsx264 + Threads + x264Profile + x264ProfileLevel + EndVideos + VCodec + copyaudiocodecs + SizeString + vidminrate + vidmaxrate + vidRotate + fps + colorspace + VBitrate + videoaspect + pass1 + pass2 + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + atempo + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + openclx264 + openclx265 + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + macska + newFullPath + macska + " 2>" + "\"" + temp + "\\vienna-encoder\\out.txt" + "\"";
                if (ini.IniReadValue("Settings", "HideCMD") == "true")
                {
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                myProcess.EnableRaisingEvents = true;
                myProcess.Exited += new EventHandler(myProcess_Exited);
                myProcess.Start();
                progressBar1.Visible = true;
                progressBar1.Show();
                timer1.Enabled = true;
                RT.Start();
                elapsedTime.Start();
                liststatus = "Konvertálás: ";
                //StatusLabel.Text = liststatus + listcount.ToString() + "\\" + listcountper.ToString();
                StatusLabel.Text = "FFmpeg";
                actualoutputfilename = newFullPath;
                toolStripStatusLabel1.Text = newFullPath;
            }
            catch
            {
                //StreamWriter sw = new StreamWriter(@"D:\Users\Frozen\Desktop\tze.txt");
                //sw.WriteLine(StartVideos + " ___ " + EndVideos);
                

                if (Environment.OSVersion.Version.Major >= 6)
                {
                    TaskDialog dlg = new TaskDialog("Nincs megadva forrásfájl!", "Figyelem", "A lista üres vagy valamilyen hiba miatt kivétel történt!", TaskDialogButton.OK, TaskDialogIcon.SecurityWarning);
                    Results results = dlg.Show(this.Handle);
                }
                else
                {
                    MessageBox.Show("A lista üres vagy valamilyen hiba miatt kivétel történt!", "Nincs megadva forrásfájl!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
//ffmpeg konvertálás vége
//libav konvertálás
        public void libavStart()
        {
            try
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[0].Selected = true;
                    listView1.Select();
                }
                string fileName = Path.GetFileName(listView1.SelectedItems.ToString());
                string fileFull = listView1.Items[0].Text;
                string simpleFileName = Path.GetFileNameWithoutExtension(listView1.Items[0].Text);
                //Ha van vágás
                osszegzesvideovago();
                //Információ gyűjtése
                if (listView1.Items[0].SubItems[2].Text != "N/A")
                {
                    //Tized másodperc elhagyása
                    string s2 = Regex.Replace(listView1.Items[0].SubItems[2].Text, ".\\d{3}$", "");
                    LenghtTB.Text = s2;
                    var values = Regex.Split(s2, @"[\s,:]+");
                    if (s2 != "N/A")
                    {
                        int ora = int.Parse(values[0]);
                        int perc = int.Parse(values[1]);
                        int mp = int.Parse(values[2]);
                        int osszeg = ora * 3600 + 60 * perc + mp;
                        if (listView1.SelectedItems[0].SubItems[5].Text == "")
                        {
                            progressBar1.Maximum = osszeg;
                            RT.TargetValue = osszeg;
                        }
                        else
                        {
                            progressBar1.Maximum = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                            RT.TargetValue = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                        }

                    }
                    //ismeretlen hártalévő idő
                    else
                    {
                        progressBar1.Style = ProgressBarStyle.Marquee;
                        progressBar1.MarqueeAnimationSpeed = 50;
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            if (Environment.OSVersion.Version.Minor >= 1)
                            {
                                var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                                prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Indeterminate);
                            }

                        }
                        label4.Text = "N/A";
                    }

                }
                else
                {
                    Process ffprobeProcess = new Process();
                    ffprobeProcess.StartInfo.FileName = "cmd.exe";
                    ffprobeProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                    ffprobeProcess.StartInfo.Arguments = "/C Codecs\\FFMpeg\\ffprobe.exe -show_streams -i \"" + fileFull + "\"" + " 2>" + "\"" + temp + "\\vienna-encoder\\info.txt" + "\"";

                    ffprobeProcess.StartInfo.CreateNoWindow = true;
                    ffprobeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    ffprobeProcess.Start();
                    ffprobeProcess.WaitForExit();


                    foreach (string duration in System.IO.File.ReadAllLines(temp + @"\vienna-encoder\info.txt"))
                    {
                        if (duration.Contains("Duration:"))
                        {
                            string pat = @"\: ([^,]*)";
                            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                            Match m = r.Match(duration);
                            //Az idő feldarabolása :)
                            string duration2 = m.ToString();
                            char[] arr = new char[] { ':', ' ' };
                            duration2 = duration2.TrimStart(arr);
                            //Tized másodperc elhagyása
                            string s2 = Regex.Replace(duration2, ".\\d{2}$", "");
                            //textBox3.Text = s2;
                            LenghtTB.Text = s2;
                            var values = Regex.Split(s2, @"[\s,:]+");
                            if (s2 != "N/A")
                            {
                                int ora = int.Parse(values[0]);
                                int perc = int.Parse(values[1]);
                                int mp = int.Parse(values[2]);
                                int osszeg = ora * 3600 + 60 * perc + mp;
                                if (listView1.SelectedItems[0].SubItems[5].Text == "")
                                {
                                    progressBar1.Maximum = osszeg;
                                    RT.TargetValue = osszeg;
                                }
                                else
                                {
                                    progressBar1.Maximum = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                                    RT.TargetValue = int.Parse(listView1.SelectedItems[0].SubItems[5].Tag.ToString());
                                }

                            }
                            //ismeretlen hártalévő idő
                            else
                            {
                                progressBar1.Style = ProgressBarStyle.Marquee;
                                progressBar1.MarqueeAnimationSpeed = 50;
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    if (Environment.OSVersion.Version.Minor >= 1)
                                    {
                                        var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                                        prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Indeterminate);
                                    }

                                }

                                label4.Text = "N/A";
                            }

                        }
                    }
                }


                //start end poz
                if (listView1.SelectedItems[0].SubItems[4].Text != "")
                {
                    StartVideos = "-ss " + listView1.SelectedItems[0].SubItems[4].Text + ".000 ";
                }
                if (listView1.SelectedItems[0].SubItems[5].Text != "")
                {
                    EndVideos = "-t " + listView1.SelectedItems[0].SubItems[5].Tag + " ";
                }
                Process[] mprs = Process.GetProcesses();
                foreach (Process mpr in mprs)
                {
                    vmpl.Stop();
                }
                string fullPath = TargetTb.Text + listView1.SelectedItems[0].SubItems[3].Text + Extension;
                string newFullPath = fullPath;
                if (FileExtCB.SelectedIndex == 2)
                {
                    int count = 1;
                    while (File.Exists(newFullPath))
                    {
                        string tempFileName = string.Format("{0}({1})", simpleFileName, count++);
                        newFullPath = Path.Combine(TargetTb.Text, tempFileName + Extension);
                    }
                }
                if (FileExtCB.SelectedIndex == 3)
                {
                    string dates = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                    newFullPath = Path.Combine(TargetTb.Text, dates + Extension);

                }
                string macska = "\"";
                if (pass == 1)
                {
                    newFullPath = "";
                    macska = "";
                }

                //textBox2.Visible = true;
                //textBox2.Text = StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + "-x264-params " + tuning + EndVideos + copyvideocodecs + VCodec + copyaudiocodecs + SizeString + fps + colorspace + VBitrate + videoaspect + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + openclx264 + openclx265 + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + "\"" + newFullPath + "\"";
                File.WriteAllText(temp + "\\vienna-encoder\\commands.txt", StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + tuning + Flagsx264 + Threads + EndVideos + VCodec + copyaudiocodecs + SizeString + vidminrate + vidmaxrate + vidRotate + fps + colorspace + VBitrate + videoaspect + pass1 + pass2 + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + macska + newFullPath + macska);
                //Konvertáló program indítása---------------------------------------------------------------
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = "cmd.exe";
                myProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                myProcess.StartInfo.Arguments = "/C Codecs\\Libav\\avconv.exe " + StartVideos + "-i \"" + fileFull + "\" " + cover + owerwrite + hwacc + tuning + Flagsx264 + Threads + EndVideos + VCodec + copyaudiocodecs + SizeString + vidminrate + vidmaxrate + vidRotate + fps + colorspace + VBitrate + videoaspect + pass1 + pass2 + disable_audio + ACodec + ABitrate + Asample + AChannels + Avol + disable_video + disable_sub + composer + title + author + comment + album + idv3 + subtitle + x264crf + presets + stereoscopicInput + stereoscopic + CommandFFMpeg + macska + newFullPath + macska + " 2>" + "\"" + temp + "\\vienna-encoder\\out.txt" + "\"";
                if (ini.IniReadValue("Settings", "HideCMD") == "true")
                {
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                myProcess.EnableRaisingEvents = true;
                myProcess.Exited += new EventHandler(myProcess_Exited);
                myProcess.Start();
                progressBar1.Visible = true;
                progressBar1.Show();
                timer1.Enabled = true;
                RT.Start();
                elapsedTime.Start();
                liststatus = "Konvertálás: ";
                //StatusLabel.Text = liststatus + listcount.ToString() + "\\" + listcountper.ToString();
                StatusLabel.Text = "Libav";
                actualoutputfilename = newFullPath;
                toolStripStatusLabel1.Text = newFullPath;
            }
            catch
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    TaskDialog dlg = new TaskDialog("Nincs megadva forrásfájl!", "Figyelem", "A lista üres vagy valamilyen hiba miatt kivétel történt!", TaskDialogButton.OK, TaskDialogIcon.SecurityWarning);
                    Results results = dlg.Show(this.Handle);
                }
                else
                {
                    MessageBox.Show("A lista üres vagy valamilyen hiba miatt kivétel történt!", "Nincs megadva forrásfájl!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
//LibAv Vége

//Paraméterek!!!!
        public void parametersSet()
        {
            Extension = ribbonComboBox8.SelectedValue.ToString();
            VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
            ACodec = ribbonComboBox2.SelectedValue.ToString();
            if (ribbonComboBox6.SelectedValue.ToString() != "")
            {
                SizeString = "-s " + ribbonComboBox6.SelectedValue.ToString();
            }
            else
            {
                SizeString = ribbonComboBox6.SelectedValue.ToString();
            }
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";

            if (ribbonComboBox3.SelectedValue.ToString() == "0")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 245k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "1")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 225k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "2")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 190k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "3")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 175k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "4")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 165k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "5")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 130k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "6")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 115k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "7")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 100k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "8")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 85k ";
            }
            else
            {
                ABitrate = "-ab " + ribbonComboBox3.SelectedValue.ToString() + " ";
            }
            Asample = "-ar " + ribbonComboBox5.SelectedValue.ToString() + " ";
            AChannels = "-ac " + ribbonComboBox7.SelectedValue.ToString() + " ";
            if (ribbonUpDown4.TextBoxText != "256" && Avol != "-vol 256 ")
            {
                Avol = "-vol " + ribbonUpDown4.TextBoxText.ToString() + " ";
            }
            else
            {
                Avol = "";
            }
            if (ribbonUpDown2.TextBoxText != "1" && atempo != "-filter:a \"atempo=1\" ")
            {
                atempo = "-filter:a \"atempo=" + ribbonUpDown2.TextBoxText.ToString().Replace(',','.') + "\" ";
            }
            else
            {
                atempo = "";
            }
            
            if (ribbonComboBox9.TextBoxText != "Nincs")
            {
                colorspace = "-pix_fmt " + ribbonComboBox9.TextBoxText.ToString() + " ";

            }

            if (ribbonComboBox10.TextBoxText != "Eredeti")
            {
                fps = "-r " + ribbonComboBox10.SelectedValue.ToString() + " ";
            }
            else
            {
                fps = "";
            }

            //video disabled
            if (videodisabledCb.Checked == true)
            {
                disable_video = "-vn ";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
            }
            //IDv3
            if (idv3Cb.Checked == true)
            {
                idv3 = "-id3v2_version 3 -write_id3v1 1 ";
            }
            if (idv3Cb.Checked == false)
            {
                idv3 = "";
            }
            //Album borító
            if (coverpathTb.TextBoxText != "")
            {
                if (ribbonComboBox8.TextBoxText == "MP3")
                {
                    // cover = "-i " + coverpathTb.TextBoxText + " -map 0:0 -map 1:0 ";
                }
            }
            else
            {
                cover = "";
            }
            //Csak audió 
            if (onlyaudioCb.Checked == true)
            {
                disable_video = "";
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                videodisabledCb.Checked = true;
            }
            //audio disabled
            if (audiodisabledCb.Checked == false)
            {
                disable_audio = "";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
            }
            //Videó kodek másolás
            if (originalcodecsCb.Checked == true)
            {
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "-c:v copy ";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                //copyvideocodecs = "-c:v copy ";
            }
            //Audió kodek másolás
            if (audiocopyCb.Checked == true)
            {
                ACodec = "";
                ABitrate = "";
                Asample = "";
                copyaudiocodecs = "-c:a copy ";
            }

            if (ini.IniReadValue("Settings", "HardwareAcc") == "true")
            {
                hwacc = "-strict experimental ";
            }
            if (ini.IniReadValue("Settings", "HardwareAcc") == "false")
            {
                hwacc = "";
            }
            if (ini.IniReadValue("Settings", "Openclx264") == "true")
            {
                openclx264 = "-x264opts opencl ";
            }
            if (ini.IniReadValue("Settings", "Openclx264") == "false")
            {
                openclx264 = "";
            }
            if (ini.IniReadValue("Settings", "Openclx265") == "true")
            {
                openclx265 = "-x265opts opencl ";
            }
            if (ini.IniReadValue("Settings", "Openclx265") == "false")
            {
                openclx265 = "";
            }
            if (FileExtCB.SelectedIndex == 0)
            {
                owerwrite = "-y ";
            }
            if (FileExtCB.SelectedIndex == 1)
            {
                owerwrite = "-n ";
            }
            if (ini.IniReadValue("Settings", "Flagsx264") == "+ildct+ilme")
            {
                Flagsx264 = "-flags +ildct+ilme ";
            }
            if (ini.IniReadValue("Settings", "Threads") != "none")
            {
                Threads = "-threads " + ini.IniReadValue("Settings", "Threads") + " ";
            }
            if (ini.IniReadValue("Settings", "x264Profile") != "none")
            {
                x264Profile = "-profile:v " + ini.IniReadValue("Settings", "x264Profile") + " ";
            }
            if (ini.IniReadValue("Settings", "x264ProfileLevel") != "none")
            {
                x264ProfileLevel = "-level " + ini.IniReadValue("Settings", "x264ProfileLevel") + " ";
            }
            //audio disabled
            if (audiodisabledCb.Checked == true)
            {
                disable_audio = "-an ";
                VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
                ACodec = "";
                ABitrate = "";
                Asample = "";
                AChannels = "";
                Avol = "";
            }

            disable_sub = ribbonComboBox4.SelectedValue.ToString();

            if (originalcodecsCb.Checked == false)
            {
                copyvideocodecs = "";
            }
            //címkék
            if (ribbonTextBox2.TextBoxText != "")
            {
                title = "-metadata title=" + "\"" + ribbonTextBox2.TextBoxText + "\" ";
            }
            if (ribbonTextBox3.TextBoxText != "")
            {
                author = "-metadata author=" + "\"" + ribbonTextBox3.TextBoxText + "\" ";
            }
            if (ribbonTextBox4.TextBoxText != "")
            {
                comment = "-metadata comment=" + "\"" + ribbonTextBox4.TextBoxText + "\" ";
            }
            if (ribbonTextBox5.TextBoxText != "")
            {
                album = "-metadata album=" + "\"" + ribbonTextBox5.TextBoxText + "\" ";
            }
            if (ribbonTextBox6.TextBoxText != "")
            {
                composer = "-metadata composer=" + "\"" + ribbonTextBox6.TextBoxText + "\" ";
            }
            if (ribbonTextBox7.TextBoxText != "")
            {
                year = "-metadata year=" + "\"" + ribbonTextBox7.TextBoxText + "\" ";
            }
            if (ribbonTextBox8.TextBoxText != "")
            {
                track = "-metadata track=" + "\"" + ribbonTextBox8.TextBoxText + "\" ";
            }
            if (ribbonTextBox9.TextBoxText != "")
            {
                description = "-metadata description=" + "\"" + ribbonTextBox9.TextBoxText + "\" ";
            }
            CommandFFMpeg = otherCommandFFMpeg.TextBoxText;
            if (ini.IniReadValue("Settings", "x264crf") != "Nincs" && ini.IniReadValue("Settings", "x264crf") != "")
            {
                x264crf = "-crf " + ini.IniReadValue("Settings", "x264crf") + " ";
            }
            if (ini.IniReadValue("Settings", "x264crf") == "Nincs")
            {
                x264crf = "";
            }
            if (ini.IniReadValue("Settings", "Tuning") != "none")
            {
                tuning = "-tune " + ini.IniReadValue("Settings", "Tuning") + " ";
            }
            if (ini.IniReadValue("Settings", "Tuning") == "none")
            {
                tuning = "";
            }
            if (ini.IniReadValue("Settings", "x264Presets") != "none")
            {
                presets = "-preset " + ini.IniReadValue("Settings", "x264Presets") + " ";
            }
            if (ini.IniReadValue("Settings", "x264Presets") == "none" && ini.IniReadValue("Settings", "x264Presets") == "")
            {
                presets = "";
            }
            if (pass == 0 || pass == 1 && CompressionTypeCb.Checked == true)
            {
                pass += 1;
            }
            if (pass > 2)
            {
                pass = 0;
            }
            if (CompressionTypeCb.Checked == true && pass == 1)
            {
                pass1 = "-pass 1 -passlogfile \"" + temp + "\\pass1.log\" -an -f " + ribbonComboBox8.TextBoxText + " -y NUL ";
                pass2 = "";
                ACodec = "";
                ABitrate = "";
                Asample = "";
                AChannels = "";
                Avol = "";
            }
            if (CompressionTypeCb.Checked == true && pass == 2)
            {
                pass1 = "";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                ABitrate = ribbonComboBox3.SelectedValue.ToString();
                Asample = ribbonComboBox5.SelectedValue.ToString();
                AChannels = ribbonComboBox7.SelectedValue.ToString();
                Avol = "-vol " + ribbonUpDown4.TextBoxText.ToString() + " ";
                pass2 = "-pass 2 -passlogfile \"" + temp + "\\pass1.log\" ";
            }
            if (CompressionTypeCb.Checked == false)
            {
                pass1 = "";
                pass2 = "";
                pass = 0;
            }

            if (rotateCb.Value == "1" || rotateCb.Value == "2")
            {
                vidRotate = "-vf \"transpose=" + rotateCb.Value + "\" ";
            }
            if (rotateCb.Value == "0" )
            {
                vidRotate = "-vf \"transpose=2,transpose=2\" ";
            }
            if (rotateCb.Value == "")
            {
                vidRotate = "";
            }
            if (VbitrateMinTb.Enabled == true)
            {
                vidminrate = "-minrate " + VbitrateMinTb.TextBoxText + "k ";
                vidmaxrate = "-maxrate " + VbitrateMaxTb.TextBoxText + "k ";
                VBitrate = "";
            }
            if (VbitrateMinTb.Enabled == false)
            {
                vidminrate = "";
                vidmaxrate = "";
            }
                
            TreeviewInfo();
        }
//FFMpeg paraméter vége

        public void libavParameters()
        {
            Extension = ribbonComboBox8.SelectedValue.ToString();
            VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
            ACodec = ribbonComboBox2.SelectedValue.ToString();
            if (ribbonComboBox6.SelectedValue.ToString() != "")
            {
                SizeString = "-s " + ribbonComboBox6.SelectedValue.ToString();
            }
            else
            {
                SizeString = ribbonComboBox6.SelectedValue.ToString();
            }
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";

            if (ribbonComboBox3.SelectedValue.ToString() == "0")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 245k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "1")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 225k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "2")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 190k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "3")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 175k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "4")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 165k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "5")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 130k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "6")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 115k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "7")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 100k ";
            }
            else if (ribbonComboBox3.SelectedValue.ToString() == "8")
            {
                ABitrate = "-q:a " + ribbonComboBox3.SelectedValue.ToString() + " -ab 85k ";
            }
            else
            {
                ABitrate = "-ab " + ribbonComboBox3.SelectedValue.ToString() + " ";
            }
            Asample = "-ar " + ribbonComboBox5.SelectedValue.ToString() + " ";
            AChannels = "-ac " + ribbonComboBox7.SelectedValue.ToString() + " ";
            if (ribbonUpDown4.TextBoxText != "256")
            {
                Avol = "-vol " + ribbonUpDown4.TextBoxText.ToString() + " ";
            }
            else
            {
                Avol = "";
            }
            if (ribbonComboBox9.TextBoxText != "Nincs")
            {
                colorspace = "-pix_fmt " + ribbonComboBox9.TextBoxText.ToString() + " ";

            }

            if (ribbonComboBox10.TextBoxText != "Eredeti")
            {
                fps = "-r " + ribbonComboBox10.SelectedValue.ToString() + " ";
            }
            else
            {
                fps = "";
            }

            //video disabled
            if (videodisabledCb.Checked == true)
            {
                disable_video = "-vn ";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
            }
            //IDv3
            if (idv3Cb.Checked == true)
            {
                idv3 = "-id3v2_version 3 -write_id3v1 1 ";
            }
            if (idv3Cb.Checked == false)
            {
                idv3 = "";
            }
            //Album borító
            if (coverpathTb.TextBoxText != "")
            {
                if (ribbonComboBox8.TextBoxText == "MP3")
                {
                    // cover = "-i " + coverpathTb.TextBoxText + " -map 0:0 -map 1:0 ";
                }
            }
            else
            {
                cover = "";
            }
            //Csak audió 
            if (onlyaudioCb.Checked == true)
            {
                disable_video = "";
                VCodec = "";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                videodisabledCb.Checked = true;
            }
            //audio disabled
            if (audiodisabledCb.Checked == false)
            {
                disable_audio = "";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
            }
            //Videó kodek másolás
            if (originalcodecsCb.Checked == true)
            {
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                VCodec = "-c:v copy ";
                SizeString = "";
                VBitrate = "";
                videoaspect = "";
                //copyvideocodecs = "-c:v copy ";
            }
            //Audió kodek másolás
            if (audiocopyCb.Checked == true)
            {
                ACodec = "";
                ABitrate = "";
                Asample = "";
                copyaudiocodecs = "-c:a copy ";
            }

            if (ini.IniReadValue("Settings", "HardwareAcc") == "true")
            {
                //hwacc = "-strict experimental ";
            }
            if (ini.IniReadValue("Settings", "HardwareAcc") == "false")
            {
                //hwacc = "";
            }
            if (ini.IniReadValue("Settings", "Openclx264") == "true")
            {
                //openclx264 = "-x264opts opencl ";
            }
            if (ini.IniReadValue("Settings", "Openclx264") == "false")
            {
                //openclx264 = "";
            }
            if (ini.IniReadValue("Settings", "Openclx265") == "true")
            {
                //openclx265 = "-x265opts opencl ";
            }
            if (ini.IniReadValue("Settings", "Openclx265") == "false")
            {
               // openclx265 = "";
            }
            if (FileExtCB.SelectedIndex == 0)
            {
                owerwrite = "-y ";
            }
            if (FileExtCB.SelectedIndex == 1)
            {
                owerwrite = "-n ";
            }
            if (ini.IniReadValue("Settings", "Flagsx264") == "+ildct+ilme")
            {
                //Flagsx264 = "-flags +ildct+ilme ";
            }
            if (ini.IniReadValue("Settings", "Threads") != "none")
            {
                Threads = "-threads " + ini.IniReadValue("Settings", "Threads") + " ";
            }
            //audio disabled
            if (audiodisabledCb.Checked == true)
            {
                disable_audio = "-an ";
                VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
                ACodec = "";
                ABitrate = "";
                Asample = "";
                AChannels = "";
                Avol = "";
            }

            disable_sub = ribbonComboBox4.SelectedValue.ToString();

            if (originalcodecsCb.Checked == false)
            {
                copyvideocodecs = "";
            }
            //címkék
            if (ribbonTextBox2.TextBoxText != "")
            {
                title = "-metadata title=" + "\"" + ribbonTextBox2.TextBoxText + "\" ";
            }
            if (ribbonTextBox3.TextBoxText != "")
            {
                author = "-metadata author=" + "\"" + ribbonTextBox3.TextBoxText + "\" ";
            }
            if (ribbonTextBox4.TextBoxText != "")
            {
                comment = "-metadata comment=" + "\"" + ribbonTextBox4.TextBoxText + "\" ";
            }
            if (ribbonTextBox5.TextBoxText != "")
            {
                album = "-metadata album=" + "\"" + ribbonTextBox5.TextBoxText + "\" ";
            }
            if (ribbonTextBox6.TextBoxText != "")
            {
                composer = "-metadata composer=" + "\"" + ribbonTextBox6.TextBoxText + "\" ";
            }
            if (ribbonTextBox7.TextBoxText != "")
            {
                year = "-metadata year=" + "\"" + ribbonTextBox7.TextBoxText + "\" ";
            }
            if (ribbonTextBox8.TextBoxText != "")
            {
                track = "-metadata track=" + "\"" + ribbonTextBox8.TextBoxText + "\" ";
            }
            if (ribbonTextBox9.TextBoxText != "")
            {
                description = "-metadata description=" + "\"" + ribbonTextBox9.TextBoxText + "\" ";
            }
            CommandFFMpeg = otherCommandFFMpeg.TextBoxText;
            if (ini.IniReadValue("Settings", "x264crf") != "Nincs" && ini.IniReadValue("Settings", "x264crf") != "")
            {
                x264crf = "-crf " + ini.IniReadValue("Settings", "x264crf") + " ";
            }
            if (ini.IniReadValue("Settings", "x264crf") == "Nincs")
            {
                x264crf = "";
            }
            if (ini.IniReadValue("Settings", "Tuning") != "none")
            {
                tuning = "-tune " + ini.IniReadValue("Settings", "Tuning") + " ";
            }
            if (ini.IniReadValue("Settings", "Tuning") == "none")
            {
                tuning = "";
            }
            if (ini.IniReadValue("Settings", "x264Presets") != "none")
            {
                presets = "-preset " + ini.IniReadValue("Settings", "x264Presets") + " ";
            }
            if (ini.IniReadValue("Settings", "x264Presets") == "none")
            {
                presets = "";
            }
            if (pass == 0 || pass == 1 && CompressionTypeCb.Checked == true)
            {
                pass += 1;
            }
            if (pass > 2)
            {
                pass = 0;
            }
            if (CompressionTypeCb.Checked == true && pass == 1)
            {
                pass1 = "-pass 1 -passlogfile \"" + temp + "\\pass1.log\" -an -f " + ribbonComboBox8.TextBoxText + " -y NUL ";
                pass2 = "";
                ACodec = "";
                ABitrate = "";
                Asample = "";
                AChannels = "";
                Avol = "";
            }
            if (CompressionTypeCb.Checked == true && pass == 2)
            {
                pass1 = "";
                ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
                ABitrate = ribbonComboBox3.SelectedValue.ToString();
                Asample = ribbonComboBox5.SelectedValue.ToString();
                AChannels = ribbonComboBox7.SelectedValue.ToString();
                Avol = "-vol " + ribbonUpDown4.TextBoxText.ToString() + " ";
                pass2 = "-pass 2 -passlogfile \"" + temp + "\\pass1.log\" ";
            }
            if (CompressionTypeCb.Checked == false)
            {
                pass1 = "";
                pass2 = "";
                pass = 0;
            }

            if (rotateCb.Value == "1" || rotateCb.Value == "2")
            {
                vidRotate = "-vf \"transpose=" + rotateCb.Value + "\" ";
            }
            if (rotateCb.Value == "0" )
            {
                vidRotate = "-vf \"transpose=2,transpose=2\" ";
            }
            if (rotateCb.Value == "")
            {
                vidRotate = "";
            }
            if (VbitrateMinTb.Enabled == true)
            {
                vidminrate = "-minrate " + VbitrateMinTb.TextBoxText + "k ";
                vidmaxrate = "-maxrate " + VbitrateMaxTb.TextBoxText + "k ";
                VBitrate = "";
            }
            if (VbitrateMinTb.Enabled == false)
            {
                vidminrate = "";
                vidmaxrate = "";
            }
                
            TreeviewInfo();
        }
//Paraméterek vége!!!



 //3D Videó ///
        private void LeftRightHorizontal_Click(object sender, EventArgs e)
        {
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            stereoscopicInput = LeftRightHorizontal.Value;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void TopBottomVertical_Click(object sender, EventArgs e)
        {
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            stereoscopicInput = TopBottomVertical.Value;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void RightLeftHorizontal_Click(object sender, EventArgs e)
        {
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            stereoscopicInput = RightLeftHorizontal.Value;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Disable3DMode_Click(object sender, EventArgs e)
        {
            enabled3DMode.Visible = false;
            toolStripSeparator4.Visible = false;
            TopBottomVertical.Checked = false;
            LeftRightHorizontal.Checked = false;
            stereoscopic = "";
            disabledStereoscopic1();
            stereoscopicInput = "";
            commands3dCMD.Text = "";
        }


        private void StereoscopicPicFormat_MouseDown(object sender, MouseEventArgs e)
        {
            if (File.Exists("Images\\Green magenta color - small.jpg") == true)
            {
                Green_magenta_colorBt.ToolTipImage = Image.FromFile("Images\\Green magenta color - small.jpg");
            }
            if (File.Exists("Images\\Green magenta dubois - small.jpg") == true)
            {
                Green_magenta_duboisBt.ToolTipImage = Image.FromFile("Images\\Green magenta dubois - small.jpg");
            }
            if (File.Exists("Images\\Green magenta gray monochrome -small.jpg") == true)
            {
                Green_magenta_graymonochromeBt.ToolTipImage = Image.FromFile("Images\\Green magenta gray monochrome -small.jpg");
            }
            if (File.Exists("Images\\Green magenta half color - small.jpg") == true)
            {
                Green_magenta_halfcolorBt.ToolTipImage = Image.FromFile("Images\\Green magenta half color - small.jpg");
            }

            if (File.Exists("Images\\Red blue gray - small.jpg") == true)
            {
                Red_blue_grayBt.ToolTipImage = Image.FromFile("Images\\Red blue gray - small.jpg");
            }
            if (File.Exists("Images\\Red cyan color - small.jpg") == true)
            {
                Red_cyan_colorBt.ToolTipImage = Image.FromFile("Images\\Red cyan color - small.jpg");
            }
            if (File.Exists("Images\\Red cyan dubois - small.jpg") == true)
            {
                Red_cyan_duboisBt.ToolTipImage = Image.FromFile("Images\\Red cyan dubois - small.jpg");
            }
            if (File.Exists("Images\\Red cyan half color - small.jpg") == true)
            {
                Red_cyan_half_colorBt.ToolTipImage = Image.FromFile("Images\\Red cyan half color - small.jpg");
            }
            if (File.Exists("Images\\Red green gray - small.jpg") == true)
            {
                Red_cian_grayBt.ToolTipImage = Image.FromFile("Images\\Red green gray - small.jpg");
            }
            if (File.Exists("Images\\Yellow blue color - small.jpg") == true)
            {
                Yellow_blue_colorBt.ToolTipImage = Image.FromFile("Images\\Yellow blue color - small.jpg");
            }
            if (File.Exists("Images\\Yellow blue dubois - small.jpg") == true)
            {
                Yellow_blue_duboisBt.ToolTipImage = Image.FromFile("Images\\Yellow blue dubois - small.jpg");
            }
            if (File.Exists("Images\\Yellow blue gray monochrome - small.jpg") == true)
            {
                Yellow_blue_gray_monochromeBt.ToolTipImage = Image.FromFile("Images\\Yellow blue gray monochrome - small.jpg");
            }
            if (File.Exists("Images\\Yellow blue half color - small.jpg") == true)
            {
                Yellow_blue_half_colorBt.ToolTipImage = Image.FromFile("Images\\Yellow blue half color - small.jpg");
            }

        }
        private void disabledStereoscopic1()
        {
            Green_magenta_duboisBt.Checked = false;
            Green_magenta_colorBt.Checked = false;
            Green_magenta_graymonochromeBt.Checked = false;
            Green_magenta_halfcolorBt.Checked = false;
            Red_blue_grayBt.Checked = false;
            Red_cyan_colorBt.Checked = false;
            Red_cyan_duboisBt.Checked = false;
            Red_cyan_half_colorBt.Checked = false;
            Red_cian_grayBt.Checked = false;
            Yellow_blue_colorBt.Checked = false;
            Yellow_blue_duboisBt.Checked = false;
            Yellow_blue_gray_monochromeBt.Checked = false;
            Yellow_blue_half_colorBt.Checked = false;
            commands3dCMD.Text = "";

        }
        private void Green_magenta_colorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Green_magenta_colorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Green_magenta_colorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Green_magenta_duboisBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Green_magenta_duboisBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Green_magenta_duboisBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Green_magenta_graymonochromeBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Green_magenta_graymonochromeBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Green_magenta_graymonochromeBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Green_magenta_halfcolorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Green_magenta_halfcolorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Green_magenta_halfcolorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Red_blue_grayBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Red_blue_grayBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Red_blue_grayBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Red_cyan_colorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Red_cyan_colorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Red_cyan_colorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Red_cyan_duboisBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Red_cyan_duboisBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Red_cyan_duboisBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Red_cyan_half_colorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Red_cyan_half_colorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Red_cyan_half_colorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Red_cian_grayBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Red_cian_grayBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Red_cian_grayBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Yellow_blue_colorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Yellow_blue_colorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Yellow_blue_colorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Yellow_blue_duboisBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Yellow_blue_duboisBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Yellow_blue_duboisBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Yellow_blue_gray_monochromeBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Yellow_blue_gray_monochromeBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Yellow_blue_gray_monochromeBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void Yellow_blue_half_colorBt_Click(object sender, EventArgs e)
        {
            stereoscopic = Yellow_blue_half_colorBt.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            Yellow_blue_half_colorBt.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void sbslCb_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            stereoscopic = sbslCb.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            sbslCb.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }
        private void sbs2lCb_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            stereoscopic = sbs2lCb.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            sbs2lCb.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void ablCb_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            stereoscopic = ablCb.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            ablCb.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }

        private void ab2lCb_CheckBoxCheckChanged(object sender, EventArgs e)
        {
            stereoscopic = ab2lCb.Value + " ";
            enabled3DMode.Visible = true;
            toolStripSeparator4.Visible = true;
            disabledStereoscopic1();
            ab2lCb.Checked = true;
            commands3dCMD.Text = stereoscopicInput + stereoscopic;
        }
       

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(ini.IniReadValue("Settings", "MainBackground")) == true && ini.IniReadValue("Settings", "MainBackground") != "none")
            {
                this.BackgroundImage = Image.FromFile(ini.IniReadValue("Settings", "MainBackground"));
            }
            if (ini.IniReadValue("Settings", "MainBackground") != "none")
            {
                MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
                parametersSet();
            }
            
            TreeviewInfo();
            if (listcheckbox == 1)
            {
                listView1.CheckBoxes = true;
            }
            if (listcheckbox == 0)
            {
                listView1.CheckBoxes = false;
            }
            if (ini.IniReadValue("Settings", "UpdateInterval") != "0")
            {
                DateTime date1 = DateTime.Parse(ini.IniReadValue("Settings", "LastUpdateCheck"));
                if (ini.IniReadValue("Settings", "UpdateInterval") == "1")
                {
                    DateTime date2 = date1.AddMonths(1);
                    if (date2 < DateTime.Now)
                    {
                        UpdateCheck();
                    }
                   
                }
                else if (ini.IniReadValue("Settings", "UpdateInterval") == "2")
                {
                    DateTime date2 = date1.AddDays(7);
                    if (date2 < DateTime.Now)
                    {
                        UpdateCheck();
                    }
                }
                else if (ini.IniReadValue("Settings", "UpdateInterval") == "3")
                {
                    UpdateCheck();
                }
            }


           

        }
        string downloadx86 = "";
        string downloadx64 = "";
        private void UpdateCheck()
        {
            string downloadUrl = "";
            string publicDate = "";
            Version newVersion = null;
            XmlTextReader reader = null;
            string xmlURL = "";
            if (ini.IniReadValue("Settings", "UpdateURL") != "")
            {
                xmlURL = ini.IniReadValue("Settings", "UpdateURL");
            }
            else
            {
                xmlURL = "http://tandemradio.hu/download/viennaversion.xml";
            }
           
            try
            {
                reader = new XmlTextReader(xmlURL);
                reader.MoveToContent();
                string elementName = "";

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "ffmpegvienna"))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            // for text nodes...  
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                // we check what the name of the node was  
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        downloadUrl = reader.Value;
                                        break;
                                    case "modify":
                                        publicDate = reader.Value;
                                        break;
                                    case "downloadx86":
                                        downloadx86 = reader.Value;
                                        break;
                                    case "downloadx64":
                                        downloadx64 = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    //Nincs Internet!SecurityError;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        TaskDialog dlg = new TaskDialog("Frissítési Hiba!", "Alkalmazás frissítése", "Nem lehet csatlakozni a tandemradio.hu weboldalához!\nLehet hogy nincs internetkapcsolat vagy a frissítés-szolgáltatás nem aktív.", TaskDialogButton.OK, TaskDialogIcon.SecurityError);
                        Results results = dlg.Show(this.Handle);
                    }
                    else
                    {
                        MessageBox.Show("Frissítési Hiba! Nem lehet csatlakozni a tandemradio.hu weboldalához!\nLehet hogy nincs internetkapcsolat vagy a frissítés-szolgáltatás nem aktív.", "Alkalmazás frissítése", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            // get the running version  
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            // compare the versions  
            if (curVersion.CompareTo(newVersion) < 0)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    //Elérhető a frissítés
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        TaskDialog dlg = new TaskDialog("Frissítés elérhető!", "Alkalmazás frissítése");
                        dlg.CustomButtons = new CustomButton[] { new CustomButton(20, "Letöltés"), new CustomButton(Result.OK, "OK") };
                            dlg.CommonIcon = TaskDialogIcon.SecurityWarning;
                            dlg.Content = "A frissítésről további részletekért kattints a \"További információ\" gombra.";
                            //Evt registration
                            dlg.ExpandedControlText = "További információ";
                            dlg.ExpandedInformation = "Elérhető verzió: " + newVersion.ToString();
                            dlg.ButtonClick += new EventHandler<ClickEventArgs>(DownloadEvent);
                            Results results = dlg.Show(this.Handle);
                        
                    }
                    else
                    {
                        MessageBox.Show("Frissítés elérhető! " + publicDate, "Alkalmazás frissítése", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                   
                });
            }
          

        }
        public void startDownload()
        {
            settings frmK = new settings();
            frmK.Controls.Add(frmK.updateControl1);
            frmK.updateControl1.Visible = true;
            frmK.listView1.Items[2].Selected = true;
            frmK.updateControl1.button1_Click(null, EventArgs.Empty);
            frmK.updateControl1.BackColor = Color.White;
            frmK.ShowDialog();
            
        }

        void DownloadEvent(object sender, ClickEventArgs e)
        {
            if (e.ButtonID == 20)
            {
                startDownload();
            }
        }

       

        private void MakeTransparent(Control ctrl, int x, int y)
        {
            Bitmap bMap = new Bitmap(this.BackgroundImage);
            Color[,] pixelArray = new Color[ctrl.Width, ctrl.Height];

            for (int i = 0; i < ctrl.Width; i++)
            {
                for (int j = 0; j < ctrl.Height; j++)
                {
                    pixelArray[i, j] = bMap.GetPixel(x + i, y + j);
                }
            }

            Bitmap bmp = new Bitmap(ctrl.Width, ctrl.Height);

            for (int i = 0; i < ctrl.Width; i++)
            {
                for (int j = 0; j < ctrl.Height; j++)
                {
                    bmp.SetPixel(i, j, pixelArray[i, j]);
                }
            }

            ctrl.BackgroundImage = bmp;
            ctrl.Location = new Point(x, y);
        }
        private int sortColumn = -1;
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView1.Sorting == SortOrder.Ascending)
                    listView1.Sorting = SortOrder.Descending;
                else
                    listView1.Sorting = SortOrder.Ascending;
            }
            listView1.Sort();
            SetWindowTheme(listView1.Handle, "explorer", null); //Explorer style
            SendMessage(listView1.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
        }


        private void OutputBrowseBt_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!folderBrowserDialog1.SelectedPath.ToString().EndsWith(@"\"))
                {
                    TargetTb.Text = folderBrowserDialog1.SelectedPath.ToString() + @"\";
                }
                else
                {
                    TargetTb.Text = folderBrowserDialog1.SelectedPath.ToString();
                }
               
            }
        }

        private void ConvertButton_ButtonClick(object sender, EventArgs e)
        {
            if (progressbar2state == false)
            {
                progressBar2.Maximum = listView1.Items.Count * 100;
                progressbar2value = 0;
            }

            progressbar2state = true;
            statusprocess = 0;
            //listcountper++;
            TempFolderCreate();
            if (ini.IniReadValue("Settings", "Coder") == "FFMpeg")
            {
                parametersSet();
                ffmpegStart();
            }
            if (ini.IniReadValue("Settings", "Coder") == "Libav")
            {
                libavParameters();
                libavStart();
            }
            
            for (int i = 0; i < 12; i++)
            {
                if (listView1.Items.Count > i)
                {
                    int v = i + 1;
                    ini.IniWriteValue("History", "Item" + v.ToString(), listView1.Items[i].SubItems[0].Text);
                }
            }
            
        }

        private void checkAllcb_CheckedChanged(object sender, EventArgs e)
        {
            if (checkAllcb.Checked == true)
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                     {
                        listView1.Items[i].Checked = true;
                     }
            }
            else
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].Checked = false;
                }
            }
        }

        private void ffmpegToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ini.IniWriteValue("Settings", "Coder", "FFMpeg");
            libavToolStripMenuItem.Checked = false;
            ffmpegToolStripMenuItem.Checked = true;
        }

        private void mencoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ini.IniWriteValue("Settings", "Coder", "Libav");
            libavToolStripMenuItem.Checked = true;
            ffmpegToolStripMenuItem.Checked = false;
        }

        private void VideoBitrateIcon_Click(object sender, EventArgs e)
        {
            if (VideoBitrateIcon.Checked == true)
            {
                ribbonUpDown1.Enabled = false;
                ribbonLabel2.Enabled = true;
                ribbonLabel3.Enabled = true;
                VbitrateMinTb.Enabled = true;
                VbitrateMaxTb.Enabled = true;
            }
            if (VideoBitrateIcon.Checked == false)
            {
                ribbonUpDown1.Enabled = true;
                ribbonLabel2.Enabled = false;
                ribbonLabel3.Enabled = false;
                VbitrateMinTb.Enabled = false;
                VbitrateMaxTb.Enabled = false;
            }
           
        }

        private void ribbonComboBox8_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            TreeviewInfo();
        }

        public void AudioCDRip()
        {
            string cddb = "";
            string trackname = "";
            string[] elements = new string[listView1.Items.Count];
            if (AudiocddbCb.Checked == true)
            {
                cddb = "-cddb ";
            }
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                elements[i] = ",";
                elements[i] = listView1.Items[i].SubItems[1].Text;
            }
            
            if (AudioAllTrackCb.Checked == false)
            {
                trackname = String.Join(",", elements);
            }
            else
            {
                trackname = "all";
            }
            try {
            //audió Konvertáló program indítása---------------------------------------------------------------
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = "cmd.exe";
                myProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                myProcess.StartInfo.Arguments = "/C Codecs\\freac\\freaccmd.exe " + "-cd " + AudioCDSoureBt.SelectedValue + " " + "-e " + AudioCdEncoderCb.Value + " " + "-track " + trackname + " " + cddb + "-d \"" + TargetTb.Text + "\"" + " 2>" + "\"" + temp + "\\vienna-encoder\\audioCDout.txt" + "\"";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.EnableRaisingEvents = true;
                myProcess.Exited += new EventHandler(AudioConvert_Exited);
                myProcess.Start();
                progressBar1.Visible = true;
                progressBar1.Show();
                elapsedTime.Start();
                timer4.Start();
                StatusLabel.Text = "Bemásolás: ";
                //tipsLabel.Text = "/C Codecs\\freac\\freaccmd.exe " + "-cd " + AudioCDSoureBt.SelectedValue + " " + "-e " + AudioCdEncoderCb.Value + " " + "-track " + trackname + " " + cddb + "-d \"" + textBox1.Text + "\""; //+ " 1>" + "\"" + temp + "\\vienna-encoder\\audioCDout.txt" + "\"";
                //liststatus = "Bemásolás: ";
                StatusLabel.Text = "freac";
                progressBar1.Style = ProgressBarStyle.Marquee;
            }
            catch
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    TaskDialog dlg = new TaskDialog("Nincs megadva forrásfájl!", "Figyelem", "A lista üres vagy valamilyen hiba miatt kivétel történt!", TaskDialogButton.OK, TaskDialogIcon.SecurityWarning);
                    Results results = dlg.Show(this.Handle);
                }
                else
                {
                    MessageBox.Show("A lista üres vagy valamilyen hiba miatt kivétel történt!", "Nincs megadva forrásfájl!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //Konvertálás kilépése!!
        void AudioConvert_Exited(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                elapsedTime.Stop();
                timer4.Stop();
                progressBar1.Style = ProgressBarStyle.Blocks;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    if (Environment.OSVersion.Version.Minor >= 1)
                    {
                        var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
                        prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                        prog.SetProgressValue(progressBar1.Value, progressBar1.Maximum);
                    }
                }

                //Kész!------------------------------------------
                //listcount = 0;
                //listcountper = 0;
                label4.Text = "100%";
                StatusLabel.Text = "Kész!";
                if (shutcommand == 0)
                {
                    if (statusprocess == 0)
                    {
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            TaskDialog dlg = new TaskDialog("Audio CD bemásolása Kész!", "Információ", elapsedTimeTb.Text + " Idő alatt a CD sikeresen átmásolódott!", TaskDialogButton.OK, TaskDialogIcon.SecuritySuccess);
                            Results results = dlg.Show(this.Handle);
                        }
                        else
                        {
                            MessageBox.Show("Audio CD bemásolása Kész!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    if (statusprocess == 1)
                    {
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            TaskDialog dlg = new TaskDialog("Audio CD bemásolása Megszakítva!", "Információ", elapsedTimeTb.Text + " Idő elteltével megszakították a bemásolást!", TaskDialogButton.OK, TaskDialogIcon.SecurityWarning);
                            Results results = dlg.Show(this.Handle);
                        }
                        else
                        {
                            MessageBox.Show("Audio CD Megszakítva!", "Információ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                }
            });
        }
                

        private void AudioCDSoureBt_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            listView1.Items.Clear();
            
            // Get an interface that provides access to the CD playlist.
            
            AxWMPLib.AxWindowsMediaPlayer wmPlayer;
            wmPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            wmPlayer.CreateControl();
            WMPLib.IWMPPlaylist playlist = wmPlayer.cdromCollection.Item(int.Parse(AudioCDSoureBt.SelectedValue)).Playlist;

            // Iterate through the CD playlist.
            for (int i = 0; i < playlist.count; i++)
            {
                double track = i + 1;

                String[] row = { playlist.name + " - " + playlist.get_Item(i).name, track.ToString(), playlist.get_Item(i).durationString };
                var listViewItem = new ListViewItem(row);

                listView1.Items.Add(listViewItem);
            }
        }

       

        private void ribbonTab7_ActiveChanged(object sender, EventArgs e)
        {
            if (ribbonTab7.Active == true)
            {
                ConvertButton.Enabled = false;
                shadowPanel1.Enabled = false;
                panel1.Enabled = false;
            }
            else
            {
                ConvertButton.Enabled = true;
                shadowPanel1.Enabled = true;
                panel1.Enabled = true;
            }
        }

        private void AudioCdConvert_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                statusprocess = 0;
                TempFolderCreate();
                AudioCDRip();
            }
        }


        private void AudioCdEncoderCb_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            AudioCdEncoderCb.Value = AudioCdEncoderCb.SelectedValue;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = elapsedTime.Elapsed;
            elapsedTimeTb.Text = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }
        public string filePropString = "None";
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

            if (listView1.SelectedItems.Count > 0 && listView1.SelectedItems.Count == 1)
            {
                FilePropertiesMenu.Visible = true;
                videoCutMenuItem.Visible = true;
                filePropString = listView1.SelectedItems[0].Text;
            }
            else
            {
                filePropString = "None";
                videoCutMenuItem.Visible = false;
                FilePropertiesMenu.Visible = false;
            }
        }

        private void FilePropertiesMenu_Click(object sender, EventArgs e)
        {
            FilePropertiesForm frmprop = new FilePropertiesForm();
            frmprop.ReturnedText = filePropString;
            frmprop.Show();
        }

        private void JoinStartBt_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count != 0)
                {
                    TempFolderCreate();
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        string fullPath = listView1.Items[i].SubItems[0].Text;
                        StreamWriter file = new StreamWriter(temp + "\\vienna-encoder\\fileList.txt", true);
                        file.WriteLine("file '" + fullPath + "'");
                        file.Close();
                    }
                    saveFileDialog1.FileName = "";
                    saveFileDialog1.Filter = ribbonComboBox8.TextBoxText + "|*" + ribbonComboBox8.SelectedValue + "|Minden fájl (*.*)|*.*";
                    string fileName = "";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveFileDialog1.FileName;
                    }
                    //Információ gyűjtése
                    Process JoinProcess = new Process();
                    JoinProcess.StartInfo.FileName = "cmd.exe";
                    JoinProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                    JoinProcess.StartInfo.Arguments = "/C Codecs\\FFMpeg\\ffmpeg.exe " + "-f concat -i \"" + temp + "\\vienna-encoder\\fileList.txt" + "\" " + "-c copy \"" + fileName + "\"";

                    JoinProcess.StartInfo.CreateNoWindow = true;
                    JoinProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    JoinProcess.EnableRaisingEvents = true;
                    JoinProcess.Exited += new EventHandler(VideoJoin_Exited);

                    JoinProcess.Start();
                    progressBar1.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        TaskDialog dlg = new TaskDialog("Nincs megadva forrásfájlok!", "A lista üres!", "Tallózd ki vagy húzd a listára videó vagy hangfájlokat az egyesítéshez.", TaskDialogButton.OK, TaskDialogIcon.SecurityWarning);
                        Results results = dlg.Show(this.Handle);
                    }
                    else
                    {
                        MessageBox.Show("Nincs megadva forrásfájlok!", "A lista üres!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                File.Delete(temp + "\\vienna-encoder\\fileList.txt");
                MessageBox.Show("A lista üres vagy valamilyen hiba miatt kivétel történt!", "Nincs megadva forrásfájl!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void rotateCb_DropDownItemClicked(object sender, RibbonItemEventArgs e)
        {
            if (rotateCb.SelectedItem.Value != "")
            {
                rotateCb.Value = rotateCb.SelectedItem.Value;
            }
        }
        XDocument xdoc = new XDocument();
        private String[] arr;
        private Image[] imageArr;
        public Font myFont;
        private void presetsloading()
        {
            const int ERROR_FILE_NOT_FOUND = 2;

            string filePath = Environment.CurrentDirectory + "\\presets.xml";

            xdoc = XDocument.Load(filePath);
            var codecsList = xdoc.Root.Descendants("PresetList").Elements("PresetName").Select(x => x.Value).ToList();
            var codecsImageList = xdoc.Root.Descendants("PresetList").Elements("PresetIcon").Select(x => x.Value).ToList();
            int countlist = codecsList.Count;
            arr = new String[countlist];
            imageArr = new Image[countlist];
            
            myFont = new System.Drawing.Font("Segoe UI", 9);

            Image[] iconList = new Image[ini.GetEntryNames("PresetIcons").Count()];
            for (int i = 0; i < ini.GetEntryNames("PresetIcons").Count(); i++)
            {
                string actualIcon = ini.IniReadValue("PresetIcons", "Preset" + i.ToString());
                iconList[i] = new Bitmap((Image)Properties.Resources.ResourceManager.GetObject(actualIcon));
            }

                string cds;
                string images;
                for (int i = 0; i < countlist; i++)
                {
                    cds = codecsList[i].ToString();
                    images = codecsImageList[i].ToString();
                    arr[i] = cds;
                    int a = int.Parse(images);
                    imageArr[i] = iconList[a];
                }
                //comboBox1.Items.Add(codecs);
            comboBox1.DataSource = arr;
            //comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (!cb.Focused)
            {
                return;
            }
            string s = this.comboBox1.SelectedItem as string;

            if (s != null)
            {
                var list = (from xNode in xdoc.Root.Descendants("PresetList") where xNode.Element("PresetName").Value == s select xNode);

                string VCodecsName = "";
                string ACodecsName = "";
                string ContainerName = "";
                string ResolutionName = "";
                string VBitrateName = "";
                string ABitrateName = "";
                string ColorspaceName = "";
                string FPSName = "";
                string AscpetRatioName = "";
                string ASampleName = "";
                string AChannelName = "";
                string SubtitleName = "";
                string VideoDisableName = "";
                string CopyVCodecsName = "";
                string CopyACodecsName = "";
                string OnlyAudioName = "";
                string DisableAudioName = "";
                string IDv3Name = "";
                string CustomResolutionName = "";
                string RotateName = "";

                string attrVcodecs = "";
                string attrAcodecs = "";
                string attrContainer = "";
                string attrResolution = "";
                string attrABitrate = "";
                string attrASample = "";
                string attrAChannel = "";
                string attrSubtitle = "";
                string attrColorspace = "";
                string attrFPS = "";

                foreach (XElement ele in list)
                {
                    VCodecsName = ele.Element("Vcodecs").Value;
                    attrVcodecs = ele.Element("Vcodecs").FirstAttribute.Value;
                    if (ele.Element("Acodecs") != null)
                    {
                        ACodecsName = ele.Element("Acodecs").Value;
                        attrAcodecs = ele.Element("Acodecs").FirstAttribute.Value;
                    }
                    if (ele.Element("Container") != null)
                    {
                        ContainerName = ele.Element("Container").Value;
                        attrContainer = ele.Element("Container").FirstAttribute.Value;
                    }
                    if (ele.Element("Resolution") != null)
                    {
                        ResolutionName = ele.Element("Resolution").Value;
                        attrResolution = ele.Element("Resolution").FirstAttribute.Value;
                    }
                    if (ele.Element("VBitrate") != null)
                    {
                        VBitrateName = ele.Element("VBitrate").Value;
                    }
                    if (ele.Element("ABitrate") != null)
                    {
                        ABitrateName = ele.Element("ABitrate").Value;
                        attrABitrate = ele.Element("ABitrate").FirstAttribute.Value;
                    }
                    if (ele.Element("Colorspace") != null)
                    {
                        ColorspaceName = ele.Element("Colorspace").Value;
                        attrColorspace = ele.Element("Colorspace").FirstAttribute.Value;
                    }
                    if (ele.Element("FPS") != null)
                    {
                        FPSName = ele.Element("FPS").Value;
                        attrFPS = ele.Element("FPS").FirstAttribute.Value;
                    }
                    if (ele.Element("ASample") != null)
                    {
                        ASampleName = ele.Element("ASample").Value;
                        attrASample = ele.Element("ASample").FirstAttribute.Value;
                    }
                    if (ele.Element("AChannel") != null)
                    {
                        AChannelName = ele.Element("AChannel").Value;
                        attrAChannel = ele.Element("AChannel").FirstAttribute.Value;
                    }
                    AscpetRatioName = ele.Element("AscpetRatio").Value;
                    if (ele.Element("Subtitle") != null)
                    {
                        SubtitleName = ele.Element("Subtitle").Value;
                        attrSubtitle = ele.Element("Subtitle").FirstAttribute.Value;
                    }
                    VideoDisableName = ele.Element("VideoDisable").Value;
                    CopyVCodecsName = ele.Element("CopyVCodecs").Value;
                    CopyACodecsName = ele.Element("CopyACodecs").Value;
                    OnlyAudioName = ele.Element("OnlyAudio").Value;
                    DisableAudioName = ele.Element("DisableAudio").Value;
                    IDv3Name = ele.Element("IDv3").Value;
                    CustomResolutionName = ele.Element("CustomResolution").Value;
                    RotateName = ele.Element("Rotate").Value;

                    if (VideoDisableName == "true")
                    {
                        videodisabledCb.Checked = true;
                    }
                    if (VideoDisableName == "false")
                    {
                        videodisabledCb.Checked = false;
                    }
                    //Original Video Codecs
                    if (CopyVCodecsName == "true")
                    {
                        originalcodecsCb.Checked = true;
                    }
                    if (CopyVCodecsName == "false")
                    {
                        originalcodecsCb.Checked = false;
                    }
                    //Original Audio Codecs
                    if (CopyACodecsName == "true")
                    {
                        audiocopyCb.Checked = true;
                    }
                    if (CopyACodecsName == "false")
                    {
                        audiocopyCb.Checked = false;
                    }
                    //Only Audio
                    if (OnlyAudioName == "true")
                    {
                        onlyaudioCb.Checked = true;
                        OnlyAudioCheck(null, EventArgs.Empty);
                    }
                    if (OnlyAudioName == "false")
                    {
                        onlyaudioCb.Checked = false;
                    }
                    //Audio Disable
                    if (DisableAudioName == "true")
                    {
                        audiodisabledCb.Checked = true;
                    }
                    if (DisableAudioName == "false")
                    {
                        audiodisabledCb.Checked = false;
                    }
                    //IDv3
                    if (IDv3Name == "true")
                    {
                        idv3Cb.Checked = true;
                    }
                    if (IDv3Name == "false")
                    {
                        idv3Cb.Checked = false;
                    }
                    //Rotate
                    if (RotateName == "0")
                    {
                        rotateCb.SelectedItem = rotate0;
                        rotateCb.Value = rotate0.Value;
                    }
                    if (RotateName == "1")
                    {
                        rotateCb.SelectedItem = rotate1;
                        rotateCb.Value = rotate1.Value;
                    }
                    if (RotateName == "2")
                    {
                        rotateCb.SelectedItem = rotate2;
                        rotateCb.Value = rotate2.Value;
                    }
                    if (RotateName == "")
                    {
                        rotateCb.SelectedItem = noneBt;
                        rotateCb.Value = noneBt.Value;
                    }
                    //videó kodek
                    RibbonButton rbvcodecs = new System.Windows.Forms.RibbonButton();
                    rbvcodecs.Text = VCodecsName;
                    rbvcodecs.Value = attrVcodecs;
                    ribbonComboBox1.SelectedItem = rbvcodecs;

                    //audio kodek
                    RibbonButton rbacodecs = new System.Windows.Forms.RibbonButton();
                    rbacodecs.Text = ACodecsName;
                    rbacodecs.Value = attrAcodecs;
                    ribbonComboBox2.SelectedItem = rbacodecs;

                    //konténer
                    RibbonButton rbcontainer = new System.Windows.Forms.RibbonButton();
                    rbcontainer.Text = ContainerName;
                    rbcontainer.Value = attrContainer;
                    ribbonComboBox8.SelectedItem = rbcontainer;
                    Extension = attrContainer;

                    //felbontás
                    RibbonButton rbResolution = new System.Windows.Forms.RibbonButton();
                    rbResolution.Text = ResolutionName;
                    rbResolution.Value = attrResolution;
                    ribbonComboBox6.SelectedItem = rbResolution;

                    //video bitráta
                    ribbonUpDown1.TextBoxText = VBitrateName;

                    //audió bitráta
                    RibbonButton rbABitrate = new System.Windows.Forms.RibbonButton();
                    rbABitrate.Text = ABitrateName;
                    rbABitrate.Value = attrABitrate;
                    ribbonComboBox3.SelectedItem = rbABitrate;

                    //audió mintavétel
                    RibbonButton rbASample = new System.Windows.Forms.RibbonButton();
                    rbASample.Text = ASampleName;
                    rbASample.Value = attrASample;
                    ribbonComboBox5.SelectedItem = rbASample;

                    //audió Csatorna
                    RibbonButton rbAChannel = new System.Windows.Forms.RibbonButton();
                    rbAChannel.Text = AChannelName;
                    rbAChannel.Value = attrAChannel;
                    ribbonComboBox7.SelectedItem = rbAChannel;
                    //Felirat
                    if (SubtitleName != null)
                    {
                        RibbonButton rbSubtitle = new System.Windows.Forms.RibbonButton();
                        rbSubtitle.Text = SubtitleName;
                        rbSubtitle.Value = attrSubtitle;
                        ribbonComboBox4.SelectedItem = rbSubtitle;
                    }
                    //színtér
                    if (ColorspaceName != "disabled")
                    {
                        RibbonButton rbColorspace = new System.Windows.Forms.RibbonButton();
                        rbColorspace.Text = ColorspaceName;
                        rbColorspace.Value = attrColorspace;
                        ribbonComboBox9.SelectedItem = rbColorspace;
                    }
                    if (ColorspaceName == "disabled")
                    {
                        ribbonComboBox9.Value = "";
                        ribbonComboBox9.TextBoxText = "Nincs";
                    }
                    //Képarány
                    videoaspect = AscpetRatioName;
                    if (videoaspect == "-aspect 16:9 ")
                    {
                        asceptwBt.Checked = true;
                        asceptsBt.Checked = false;
                    }
                    if (videoaspect == "-aspect 4:3 ")
                    {
                        asceptwBt.Checked = false;
                        asceptsBt.Checked = true;
                    }
                    if (videoaspect != "-aspect 4:3 " && videoaspect != "-aspect 16:9 " && videoaspect != "")
                    {
                        asceptwBt.Checked = false;
                        asceptsBt.Checked = false;
                        asceptcusBt.Checked = true;
                        customarXYTb.TextBoxText = videoaspect.Remove(videoaspect.Length - 1).Replace("-aspect ", "");
                    }
                    if (videoaspect == "")
                    {
                        asceptwBt.Checked = false;
                        asceptsBt.Checked = false;
                        asceptcusBt.Checked = false;
                    }
                    //CustomResolution
                    if (CustomResolutionName == "true")
                    {
                        CustomResLael.Enabled = true;
                        ribbonItemGroup1.Enabled = true;
                        string xy = attrResolution.Remove(attrResolution.Length - 1).Replace("-s ", "");
                        CusResXTb.TextBoxText = xy.Substring(0, xy.IndexOf('x'));
                        CusResYTb.TextBoxText = xy.Substring(xy.IndexOf('x') + 1);
                    }
                    if (CustomResolutionName == "false")
                    {
                        CustomResLael.Enabled = false;
                        ribbonItemGroup1.Enabled = false;
                    }

                }
            }
            TreeviewInfo();
        }

        private void ribbonOrbMenuItem6_Click(object sender, EventArgs e)
        {
            OpenFiles();
        }

        private void jumpfolderBT_Click(object sender, EventArgs e)
        {
            if (TargetTb.Text != "")
            {
                Process.Start("explorer.exe", TargetTb.Text);
            }

        }

        private void vidfullscreen_Click(object sender, EventArgs e)
        {
            VideoPlayerForm vplfrm = new VideoPlayerForm(this);
             if (listView1.SelectedItems.Count > 0)
             {
                 vplfrm.videofile = listView1.SelectedItems[0].SubItems[0].Text;
             }
           
            vplfrm.Show();
        }

        private void ribbon1_OrbClicked(object sender, EventArgs e)
        {
           
        }

        private void rb_Click(object sender, EventArgs e)
        {
            RibbonButton foo = sender as RibbonButton;
            //listView1.BackgroundImage = null;
            MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
            // Read the files
                try
                {
                    string[] sizes = { "B", "KB", "MB", "GB" };
                    double len = new FileInfo(foo.Value).Length;
                    int order = 0;
                    while (len >= 1024 && order + 1 < sizes.Length)
                    {
                        order++;
                        len = len / 1024;
                    }
                    string result = String.Format("{0:0.##} {1}", len, sizes[order]);

                    string fileName = Path.GetFullPath(foo.Value);
                    string fileExt = Path.GetExtension(foo.Value).TrimStart('.');
                    string outputName = Path.GetFileNameWithoutExtension(foo.Value);
                    string startEnd = "";
                    string subtitlePath = "";
                    string audioPath = "";
                    MediaInfo MediaInfoLib = new MediaInfo();
                    MediaInfoLib.Open(Path.GetFullPath(foo.Value));
                    string durationinfo = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String3");
                    if (durationinfo == "")
                    {
                        durationinfo = "N/A";
                    }

                    string[] row = { fileName, result, durationinfo, outputName, startEnd, startEnd, subtitlePath, audioPath };
                    var listViewItem = new ListViewItem(row, _iconListManager.AddFileIcon(foo.Value));
                    listView1.Items.Add(listViewItem);


                    if (fileExt == "mp3" || fileExt == "m4a" || fileExt == "ogg" || fileExt == "flac" || fileExt == "wma" || fileExt == "aac" || fileExt == "ra")
                    {
                        onlyaudioCb.Checked = true;
                        if (onlyaudioCb.Checked == true)
                        {
                            disable_video = "";
                            VCodec = "";
                            SizeString = "";
                            VBitrate = "";
                            videoaspect = "";
                            videodisabledCb.Checked = true;
                            //ribbonButton66.Enabled = false;
                            //ribbonComboBox8.DropDownItems.ElementAt(2).Visible = false;
                            ribbonComboBox1.Enabled = false;
                            ribbonComboBox6.Enabled = false;
                            ribbonUpDown1.Enabled = false;
                            for (int j = 0; j < 18; j++)
                            {
                                ribbonComboBox8.DropDownItems.ElementAt(j).Visible = false;
                            }
                            ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(19);
                        }
                        else
                        {
                            videodisabledCb.Checked = false;
                            for (int j = 0; j < 18; j++)
                            {
                                ribbonComboBox8.DropDownItems.ElementAt(j).Visible = true;
                            }
                            ribbonComboBox8.SelectedItem = ribbonComboBox8.DropDownItems.ElementAt(0);
                            ribbonComboBox1.Enabled = true;
                            ribbonComboBox6.Enabled = true;
                            ribbonUpDown1.Enabled = true;
                        }
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show("Nem nyithatóak meg a fájlok!" + ex.Message);
                }
                itemsCount();
            

        }
        private void rem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 12; i++)
            {
                int v = i + 1;
                ini.IniWriteValue("History", "Item"+v.ToString(), "");
                if (ribbon1.OrbDropDown.RecentItems.Count > v)
                {
                    ribbon1.OrbDropDown.RecentItems[v].Visible = false;
                }
                
            }

        }
        
        public void cutVideos(int cutcount, string[] videofile, string[] start, string[] end, string[] ossz)
        {
            
            for (int i = 0; i < cutcount; i++)
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = new FileInfo(Path.GetFullPath(listView1.SelectedItems[0].SubItems[0].Text)).Length;
                int order = 0;
                while (len >= 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    len = len / 1024;
                }
                string result = String.Format("{0:0.##} {1}", len, sizes[order]);
                //int elems = i + 1;
                string fileName = Path.GetFullPath(listView1.SelectedItems[0].SubItems[0].Text);
                string fileExt = Path.GetExtension(listView1.SelectedItems[0].SubItems[0].Text).TrimStart('.');
                //string outputName = videofile[elems];
                //string[] startEnd = start[i];
                string subtitlePath = "";
                string audioPath = "";
                MediaInfo MediaInfoLib = new MediaInfo();
                MediaInfoLib.Open(Path.GetFullPath(listView1.SelectedItems[0].SubItems[0].Text));
                string durationinfo = MediaInfoLib.Get(StreamKind.General, 0, "Duration/String3");
                if (durationinfo == "")
                {
                    durationinfo = "N/A";
                }
                string[] row = { fileName, result, durationinfo, videofile[i], start[i], end[i], subtitlePath, audioPath };
                var listViewItem = new ListViewItem(row, _iconListManager.AddFileIcon(listView1.SelectedItems[0].SubItems[0].Text));
                //listViewItem.SubItems[5].Tag = ossz;
                listView1.Items.Add(listViewItem);
                //osszegzesvideovago();
                //listView1.SelectedItems[0].SubItems[5].Tag = ossz[i];
            }
            //tipsLabel.Text = videofile[0];
        }

        private void itemsCount()
        {
            TimeSpan osszeg = new TimeSpan();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[2].Text != "N/A" || listView1.Items[i].SubItems[2].Text != "Unable to load MediaInfo library")
                {
                    osszeg += TimeSpan.Parse(listView1.Items[i].SubItems[2].Text);
                }
                
            }
            countLabel.Text = listView1.Items.Count.ToString() + " elem, " + string.Format("{0:00}:{1:00}:{2:00}",(int)osszeg.TotalHours,osszeg.Minutes,osszeg.Seconds);

        }

        private void videoCutMenuItem_Click(object sender, EventArgs e)
        {
            vidfullscreen.PerformClick();
        }

        private void ribbonUpDown2_DownButtonClicked(object sender, MouseEventArgs e)
        {
            double d = double.Parse(ribbonUpDown2.TextBoxText);
            d = d - 0.01;
            ribbonUpDown2.TextBoxText = d.ToString();

        }

        private void ribbonUpDown2_UpButtonClicked(object sender, MouseEventArgs e)
        {
            double d = double.Parse(ribbonUpDown2.TextBoxText);
            d = d + 0.01;
            ribbonUpDown2.TextBoxText = d.ToString();
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //Lets highlight the currently selected item like any well behaved combo box should
            e.Graphics.FillRectangle(Brushes.DodgerBlue, e.Bounds);
            e.Graphics.DrawString(arr[e.Index], myFont, Brushes.WhiteSmoke, new Point(imageArr[e.Index].Width + 6, e.Bounds.Y + 12));
            e.Graphics.DrawImage(imageArr[e.Index], new Point(e.Bounds.X, e.Bounds.Y));

            //is the mouse hovering over a combobox item??			
            if ((e.State & DrawItemState.Focus) == 0)
            {
                //this code keeps the last item drawn from having a Bisque background. 
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                e.Graphics.DrawString(arr[e.Index], myFont, Brushes.Black, new Point(imageArr[e.Index].Width + 6, e.Bounds.Y + 12));
                e.Graphics.DrawImage(imageArr[e.Index], new Point(e.Bounds.X, e.Bounds.Y));
            }
        }

        private void ribbonComboBox1_TextBoxTextChanged(object sender, EventArgs e)
        {
            VCodec = "-c:v " + ribbonComboBox1.SelectedValue.ToString();
        }

        private void ribbonComboBox2_TextBoxTextChanged(object sender, EventArgs e)
        {
            ACodec = "-acodec " + ribbonComboBox2.SelectedValue.ToString();
        }

        private void ribbonUpDown1_TextBoxTextChanged(object sender, EventArgs e)
        {
            VBitrate = "-b:v " + ribbonUpDown1.TextBoxText.ToString() + "k ";
        }

        private void ribbonComboBox6_TextBoxTextChanged(object sender, EventArgs e)
        {
            if (ribbonComboBox6.SelectedValue.ToString() != "")
            {
                SizeString = "-s " + ribbonComboBox6.SelectedValue.ToString();
                TreeviewInfo();
            }
            else
            {
                SizeString = ribbonComboBox6.SelectedValue.ToString();
                TreeviewInfo();
            }
        }

        private void ribbonOrbMenuItem7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "Xml Fájl|*.xml|Minden fájl (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s = this.comboBox1.SelectedItem as string;
                int Icon = 0;

                if (s != null)
                {
                    var list = (from xNode in xdoc.Root.Descendants("PresetList") where xNode.Element("PresetName").Value == s select xNode);

                    

                    foreach (XElement ele in list)
                    {
                        if (ele.Element("PresetName").Value == s)
                        {
                            Icon = int.Parse(ele.Element("PresetIcon").Value);
                        }
                    }
                }
                ExportPresets(saveFileDialog1.FileName, s, Icon);
            }

        }
        private void ExportPresets(string fileName, string NamePreset, int Iconnum)
        {
            //Gyökér elemen belüli elemek létrehozása
            XDocument doc = new XDocument();
            XElement root = new XElement("PresetList");
            root.Add(new XElement("PresetName", NamePreset));
            root.Add(new XElement("PresetIcon", Iconnum));
            root.Add(new XElement("Vcodecs", new XAttribute("Value", ribbonComboBox1.SelectedValue), ribbonComboBox1.TextBoxText));
            root.Add(new XElement("Acodecs", new XAttribute("Value", ribbonComboBox2.SelectedValue), ribbonComboBox2.TextBoxText));
            root.Add(new XElement("Container", new XAttribute("Value", ribbonComboBox8.SelectedValue), ribbonComboBox8.TextBoxText));
            root.Add(new XElement("Resolution", new XAttribute("Value", ribbonComboBox6.SelectedValue), ribbonComboBox6.TextBoxText));
            root.Add(new XElement("VBitrate", ribbonUpDown1.TextBoxText));
            root.Add(new XElement("ABitrate", new XAttribute("Value", ribbonComboBox3.SelectedValue), ribbonComboBox3.TextBoxText));
            if (ribbonComboBox9.SelectedValue != null)
            {
                root.Add(new XElement("Colorspace", new XAttribute("Value", ribbonComboBox9.SelectedValue), ribbonComboBox9.TextBoxText));
            }
            else
            {
                root.Add(new XElement("Colorspace", new XAttribute("Value", "disabled"), "disabled"));
            }
            if (ribbonComboBox10.SelectedValue != null)
            {
                root.Add(new XElement("FPS", new XAttribute("Value", ribbonComboBox10.SelectedValue), ribbonComboBox10.TextBoxText));
            }
            else
            {
                root.Add(new XElement("FPS", new XAttribute("Value", "disabled"), "disabled"));
            }
            root.Add(new XElement("AscpetRatio", videoaspect));
            root.Add(new XElement("ASample", new XAttribute("Value", ribbonComboBox5.SelectedValue), ribbonComboBox5.TextBoxText));
            root.Add(new XElement("AChannel", new XAttribute("Value", ribbonComboBox7.SelectedValue), ribbonComboBox7.TextBoxText));
            if (ribbonComboBox4.SelectedValue != null)
            {
                root.Add(new XElement("Subtitle", new XAttribute("Value", ribbonComboBox4.SelectedValue), ribbonComboBox4.TextBoxText));
            }
            if (ribbonComboBox4.SelectedValue == null)
            {
                root.Add(new XElement("Subtitle", new XAttribute("Value", ""), ribbonComboBox4.TextBoxText));
            }

            if (videodisabledCb.Checked == true)
            {
                root.Add(new XElement("VideoDisable", "true"));
            }
            else
            {
                root.Add(new XElement("VideoDisable", "false"));
            }
            //Original Video Codecs
            if (originalcodecsCb.Checked == true)
            {
                root.Add(new XElement("CopyVCodecs", "true"));
            }
            else
            {
                root.Add(new XElement("CopyVCodecs", "false"));
            }
            //Original Audio Codecs
            if (audiocopyCb.Checked == true)
            {
                root.Add(new XElement("CopyACodecs", "true"));
            }
            else
            {
                root.Add(new XElement("CopyACodecs", "false"));
            }
            //Only Audio
            if (onlyaudioCb.Checked == true)
            {
                root.Add(new XElement("OnlyAudio", "true"));
            }
            else
            {
                root.Add(new XElement("OnlyAudio", "false"));
            }
            //Audio Disable
            if (audiodisabledCb.Checked == true)
            {
                root.Add(new XElement("DisableAudio", "true"));
            }
            else
            {
                root.Add(new XElement("DisableAudio", "false"));
            }
            //IDv3
            if (idv3Cb.Checked == true)
            {
                root.Add(new XElement("IDv3", "true"));
            }
            else
            {
                root.Add(new XElement("IDv3", "false"));
            }
            //Custom Resolution
            if (CustomResLael.Enabled == true)
            {
                root.Add(new XElement("CustomResolution", "true"));
            }
            else if (CustomResLael.Enabled == false)
            {
                root.Add(new XElement("CustomResolution", "false"));
            }
            //Rotate
            if (rotateCb.Value != "")
            {
                root.Add(new XElement("Rotate", rotateCb.Value));
            }
            //Itt a gyökér nevét kell megadni
            new XElement("Root", new XElement("Child", "content"));

            doc.Add(root);
            //doc.Element("Presets").Add(root);
            doc.Save(fileName);
        }

        private void ribbonOrbMenuItem8_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML fájlok|*.xml|Minden fájl (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                XElement xFileRoot = XElement.Load("presets.xml");
                XElement xFileChild = XElement.Load(openFileDialog1.FileName);
                xFileRoot.Add(xFileChild);
                xFileRoot.Save("presets.xml");
            }
            presetsloading();
        }




    }
}


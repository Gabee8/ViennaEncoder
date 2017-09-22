using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ini;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace ViennaEncoder
{
    public partial class UpdateControl : UserControl
    {
        IniFile ini = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        public UpdateControl()
        {
            InitializeComponent();
            pictureBox1.Image = ViennaEncoder.Properties.Resources.setup1;
            label4.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (File.Exists("MediaInfo.dll") == true)
            {
                FileVersionInfo MediaInfoVersion = FileVersionInfo.GetVersionInfo("MediaInfo.dll");
                label13.Text = MediaInfoVersion.FileVersion.ToString();
            }
            if (File.Exists("Codecs\\mkvmerge.exe") == true)
            {
                FileVersionInfo MkvmergeVersion = FileVersionInfo.GetVersionInfo("Codecs\\mkvmerge.exe");
                label15.Text = MkvmergeVersion.FileVersion.ToString();
            }
            if (File.Exists("Codecs\\Vlc\\libvlc.dll") == true)
            {
                FileVersionInfo libvlcVersion = FileVersionInfo.GetVersionInfo("Codecs\\Vlc\\libvlc.dll");
                label11.Text = libvlcVersion.FileVersion.ToString();
            }
            label8.Text = ini.IniReadValue("Settings", "LastUpdateCheck");
            label10.Text = "";
            label6.Text = "";
            if (ini.IniReadValue("Settings", "UpdateInterval") == "")
            {
                ini.IniWriteValue("Settings", "UpdateInterval", "0");
                comboBox1.SelectedIndex = 0;
            }
            else
            {
                comboBox1.SelectedIndex = int.Parse(ini.IniReadValue("Settings", "UpdateInterval"));
            }
            if (ini.IniReadValue("Settings", "UpdateURL") != "")
            {
                xmlURL = ini.IniReadValue("Settings", "UpdateURL");
            }
            else
            {
                xmlURL = "http://tandemradio.hu/download/viennaversion.xml";
            }
            /*if (ini.IniReadValue("Settings", "LastUpdateCheck") == "")
            {
            }
            else
            {
                string datenow = DateTime.Now.ToString("yyyy.MM.dd hh:mm");
                if (ini.IniReadValue("Settings", "LastUpdateCheck") == datenow)
                {

                    //label8.Text = DateTime.Now.ToString("yyyy.MM.dd hh:mm");
                }
            }*/
            
            
        }
        public void SetUpdateEvent()
        {
            if (ini.IniReadValue("Settings", "UpdateInterval") != "0")
            {
                DateTime date1 = DateTime.Parse(ini.IniReadValue("Settings", "LastUpdateCheck"));
                if (ini.IniReadValue("Settings", "UpdateInterval") == "1")
                {
                    DateTime date2 = date1.AddMonths(1);
                    if (date2 < DateTime.Now)
                    {
                        button1.PerformClick();
                    }
                    else
                    {
                        UpdateNoCheck();
                    }

                }
                else if (ini.IniReadValue("Settings", "UpdateInterval") == "2")
                {
                    DateTime date2 = date1.AddDays(7);
                    if (date2 < DateTime.Now)
                    {
                        button1.PerformClick();
                    }
                    else
                    {
                        UpdateNoCheck();
                    }
                }
                else if (ini.IniReadValue("Settings", "UpdateInterval") == "3")
                {
                    button1.PerformClick();
                }
            }
            else
            {
                UpdateNoCheck();
            }
        }
        Color top =  Color.White;
        Color bottom = Color.White;
        Version newVersion = null;
        string downloadUrl = "";
        string publicDate = "";
        string downloadx86 = "";
        string downloadx64 = "";
        XmlTextReader reader = null;
        string xmlURL = "";

        //string xmlURL = "http://tandemradio.hu/download/viennaversion.xml";
        bool success = true;

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ffmpeg.zeranoe.com/builds");
        }

        public void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            label2.Text = "Frissítés keresése...";
        }

        private void label10_Click(object sender, EventArgs e)
        {
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (curVersion.CompareTo(newVersion) < 0)
            {
                System.Diagnostics.Process.Start(downloadUrl);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mediaarea.net/hu/MediaInfo/Download/Windows");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ini.IniWriteValue("Settings", "UpdateInterval", comboBox1.SelectedIndex.ToString());
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
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
                UpdateError();
                success = false;
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
                    label6.Text = newVersion.ToString() + "  Módosítva: " + publicDate;
                    UpdateAviable();
                });
            }
            else
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (success == true)
                    {
                        UpdateNone();
                        label6.Text = newVersion.ToString() + "  Módosítva: " + publicDate;
                    }
                   
                });
            }
            BeginInvoke((MethodInvoker)delegate
            {
            label8.Text = DateTime.Now.ToLongDateString();
            ini.IniWriteValue("Settings", "LastUpdateCheck", label8.Text);
            });

        }
        public void UpdateNone()
        {
            pictureBox1.Image = ViennaEncoder.Properties.Resources.update_none;
            label2.Text = "A program naprakész";
            button2.Visible = false;
            button3.Visible = false;
            top = Color.FromArgb(22, 118, 20);
            bottom = Color.FromArgb(65, 178, 61);
            sidecolorpanel.Refresh();
            label10.Text = "Nem érhető el új verzió :( A legfrissebb kiadást használod!\nKérlek keress később frissítéseket.";
            label10.Cursor = System.Windows.Forms.Cursors.Default;
            label5.Visible = true;
        }
        public void UpdateAviable()
        {
            pictureBox1.Image = ViennaEncoder.Properties.Resources.update_aviable;
            top = Color.FromArgb(242, 177, 0);
            bottom = Color.FromArgb(254, 205, 72);
            sidecolorpanel.Refresh();
            button2.Visible = true;
            button3.Visible = true;
            label10.Text = "Letölthető egy frissebb verzió a tandemradio.hu/vienna weboldaláról.\nA letöltéshez kattints a Letöltés gombra vagy keresd fel a \ntandemradio.hu/vienna weboldalt!";
            label2.Text = "Új verzió elérhető!";
            label10.Cursor = System.Windows.Forms.Cursors.Hand;
            label5.Visible = true;
        }
        public void UpdateError()
        {
            pictureBox1.Image = ViennaEncoder.Properties.Resources.update_error;
            top = Color.FromArgb(172, 1, 0);
            bottom = Color.FromArgb(221, 1, 0);
            sidecolorpanel.Refresh();
            label10.Text = "Nem lehet csatlakozni a tandemradio.hu weboldalához!\nLehet hogy nincs internetkapcsolat vagy a frissítés-szolgáltatás nem aktív.";
            label2.Text = "Hiba a csatlakozáskor!";
            label6.Text = "Nem elérhető";
            label5.Visible = true;
        }
        public void UpdateNoCheck()
        {
            top = Color.FromArgb(0, 88, 158);
            bottom = Color.FromArgb(0, 130, 224);
            sidecolorpanel.Refresh();
            DateTime date1 = DateTime.Parse(ini.IniReadValue("Settings", "LastUpdateCheck"));
            if (ini.IniReadValue("Settings", "UpdateInterval") == "1")
            {
                DateTime date2 = date1.AddMonths(1);
                if (date2 > DateTime.Now)
                {
                    label10.Text = "Frissítés egy hónapon belül volt keresve. \nAz automatikus frissítés ütemezve van havi rendszerességgel de ha szeretnéd \nleellenőrizni hogy van e újabb verziójú Vienna Encoder akkor a \nFrissítés ellenőrzése gombbal megteheted!";
                }
            }
            else if (ini.IniReadValue("Settings", "UpdateInterval") == "2")
            {
                DateTime date2 = date1.AddDays(7);
                if (date2 > DateTime.Now)
                {
                    label10.Text = "Frissítés egy héten belül volt keresve. \nAz automatikus frissítés ütemezve van heti rendszerességgel de ha szeretnéd \nleellenőrizni hogy van e újabb verziójú Vienna Encoder akkor a \nFrissítés ellenőrzése gombbal megteheted!";
                }
            }
            else if (ini.IniReadValue("Settings", "UpdateInterval") == "3")
            {
                label10.Text = "Frissítés indításkor volt keresve. \nAz automatikus frissítés ütemezve van de ha szeretnéd leellenőrizni hogy van e újabb \nverziójú Vienna Encoder akkor a Frissítés ellenőrzése gombbal megteheted!";
            }
            else if (ini.IniReadValue("Settings", "UpdateInterval") == "0")
            {
                label10.Text = "Frissítés nem volt a közelmúltban keresve vagy nincs ütemezve \naz automatikus frissítés! Ha szeretnéd leellenőrizni hogy van e újabb \nverziójú Vienna Encoder akkor a Frissítés ellenőrzése gombbal megteheted!";
            }
            
            label2.Text = "Nincs ellenőrizve frissítés";
            label5.Visible = false;
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = 100;
        }

        private void sidecolorpanel_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientMode direction = LinearGradientMode.Vertical;
            LinearGradientBrush brush = new LinearGradientBrush(sidecolorpanel.ClientRectangle, top, bottom, direction);
            e.Graphics.FillRectangle(brush, sidecolorpanel.ClientRectangle);
        }

        public void startDownload()
        {
            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(downloadx86), saveFileDialog1.FileName);
            });
            thread.Start();
        }
        public void startDownload64()
        {
            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(downloadx64), saveFileDialog1.FileName);
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                label2.Text = "Letöltés... " + e.BytesReceived /1024 + "KB a " + e.TotalBytesToReceive/1024 + "KB-ból";
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                label2.Text = "Letöltés Kész!";
                button2.Enabled = true;
                button3.Enabled = true;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
                saveFileDialog1.ShowDialog();
                button2.Enabled = false;
                button3.Enabled = false;
                startDownload();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            button2.Enabled = false;
            button3.Enabled = false;
            startDownload64();

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.bunkus.org/videotools/mkvtoolnix/downloads.html#windows");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.videolan.org/vlc/download-windows.html");
        }
        
    }
}

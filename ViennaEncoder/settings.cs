using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Ini;
using System.Reflection;
using System.Windows.Forms;
using WindowsFormsAero;
using WindowsFormsAero.TaskDialog;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ViennaEncoder
{
    public partial class settings :Form// WindowsFormsAero.Dwm.Helpers.GlassForm 
    {
        //Konfigurációfájl helye
        IniFile ini = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        const int ERROR_FILE_NOT_FOUND = 2;

        protected override void OnLoad(EventArgs e)
        {
            
                base.OnLoad(e);
                if (ini.IniReadValue("Settings", "Aero") == "enable")
                {
                 WindowsFormsAero.Dwm.DwmManager.EnableGlassFrame(this, new WindowsFormsAero.Dwm.Margins(0, 0, 44, 0));
                }
                else
                {
                    if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
                    {
                        themedLabel1.BackColor = Color.White;
                        pictureBox1.BackColor = Color.White;
                    }
                }
        }

        //mozgatható ablak
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        //mozgatható ablak eddig
        //Imagelistview kijelölés
        public const int LVM_FIRST = 0x1000; //Value from http://www.winehq.org/pipermail/wine-devel/2002-October/009527.html
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        public const int LVS_EX_DOUBLEBUFFER = 0x00010000;
        
        //Imports the UXTheme DLL
        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd, String textSubAppName, String textSubIdList);
        int selectedRowIndex = 0;
        const int BM_SETIMAGE = 0x00F7;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        public settings()
        {
            InitializeComponent();

            SetWindowTheme(listView1.Handle, "explorer", null); //Explorer style
            SendMessage(listView1.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

            if (listView1.Items.Count > 0 && (String.Empty != listView1.Items[selectedRowIndex].ToString()))
            {
                listView1.Items[selectedRowIndex].Selected = true;
                listView1.Select();
            }
            
            
 
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void settings_Load(object sender, EventArgs e)
        {
          
            if (listView1.Items[0].Selected == true)
            {
                mainControl1.Visible = true;
            }
            
            if (ini.IniReadValue("Settings", "Aero") == "enable")
            {
                themedLabel1.Visible = true;
                pictureBox1.Visible = false;
            }
            else if (ini.IniReadValue("Settings", "Aero") == "disable")
            {
                this.BackColor = System.Drawing.Color.White;
                this.Refresh();
                themedLabel1.Visible = false;
                pictureBox1.Visible = true;
            }
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                // enable glass rendering
                pictureBox1.Visible = false;
                themedLabel1.Visible = true;
            }
            else
            {
                this.BackColor = SystemColors.ActiveCaption;
                pictureBox1.Visible = true;
                themedLabel1.Visible = false;
                // fallback rendering
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedRowIndex = listView1.SelectedItems[0].Index;
            }
            else
            {
                listView1.Items[selectedRowIndex].Selected = true;
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.FocusedItem.Index == 0)
            {
                mainControl1.Visible = true;
                mainControl1.BackColor = System.Drawing.Color.White;
                configControl1.Visible = false;
                versionControl1.Visible = false;
                aboutControl1.Visible = false;
                updateControl1.Visible = false;
            }
            if (listView1.FocusedItem.Index == 1)
            {
                this.Controls.Add(this.configControl1);
                configControl1.BackColor = System.Drawing.Color.White;
                mainControl1.Visible = false;
                configControl1.Visible = true;
                versionControl1.Visible = false;
                aboutControl1.Visible = false;
                updateControl1.Visible = false;
            }
            if (listView1.FocusedItem.Index == 2)
            {
                this.Controls.Add(this.updateControl1);
                updateControl1.BackColor = System.Drawing.Color.White;
                mainControl1.Visible = false;
                configControl1.Visible = false;
                versionControl1.Visible = false;
                aboutControl1.Visible = false;
                updateControl1.Visible = true;
                updateControl1.SetUpdateEvent();
            }
            if (listView1.FocusedItem.Index == 3)
            {
                this.Controls.Add(this.versionControl1);
                versionControl1.BackColor = System.Drawing.Color.White;
                mainControl1.Visible = false;
                configControl1.Visible = false;
                versionControl1.Visible = true;
                aboutControl1.Visible = false;
                updateControl1.Visible = false;
            }
            if (listView1.FocusedItem.Index == 4)
            {
                this.Controls.Add(this.aboutControl1);
                aboutControl1.BackColor = System.Drawing.Color.White;
                mainControl1.Visible = false;
                configControl1.Visible = false;
                versionControl1.Visible = false;
                aboutControl1.Visible = true;
                updateControl1.Visible = false;
            }
        }

        private void settings_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void CheckEnter(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)13)
            {
                if (listView1.FocusedItem.Index == 0)
                {
                    mainControl1.Visible = true;
                    mainControl1.BackColor = System.Drawing.Color.White;
                    configControl1.Visible = false;
                    versionControl1.Visible = false;
                    aboutControl1.Visible = false;
                    updateControl1.Visible = false;
                }
                if (listView1.FocusedItem.Index == 1)
                {
                    this.Controls.Add(this.configControl1);
                    configControl1.BackColor = System.Drawing.Color.White;
                    mainControl1.Visible = false;
                    configControl1.Visible = true;
                    versionControl1.Visible = false;
                    aboutControl1.Visible = false;
                    updateControl1.Visible = false;
                }
                if (listView1.FocusedItem.Index == 2)
                {
                    this.Controls.Add(this.updateControl1);
                    updateControl1.BackColor = System.Drawing.Color.White;
                    mainControl1.Visible = false;
                    configControl1.Visible = false;
                    versionControl1.Visible = false;
                    aboutControl1.Visible = false;
                    updateControl1.Visible = true;
                }
                if (listView1.FocusedItem.Index == 3)
                {
                    this.Controls.Add(this.versionControl1);
                    versionControl1.BackColor = System.Drawing.Color.White;
                    mainControl1.Visible = false;
                    configControl1.Visible = false;
                    versionControl1.Visible = true;
                    aboutControl1.Visible = false;
                    updateControl1.Visible = false;
                }
                if (listView1.FocusedItem.Index == 4)
                {
                    this.Controls.Add(this.aboutControl1);
                    aboutControl1.BackColor = System.Drawing.Color.White;
                    mainControl1.Visible = false;
                    configControl1.Visible = false;
                    versionControl1.Visible = false;
                    aboutControl1.Visible = true;
                    updateControl1.Visible = false;
                }
            }
        }





    }
}

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
using System.Runtime.InteropServices;

namespace ViennaEncoder
{

    public partial class MainControl : UserControl
    {
        //Konfigurációfájl helye
        IniFile inis = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        const int ERROR_FILE_NOT_FOUND = 2;
        public MainControl()
        {
            InitializeComponent();
            if (Environment.OSVersion.Version.Major < 6)
            {
                checkBox2.Enabled = false;
                checkBox5.Enabled = false;
                inis.IniWriteValue("Settings", "TaskbarControl", "disable");
                inis.IniWriteValue("Settings", "Aero", "disable");
                if (Environment.OSVersion.Version.Minor < 1)
                {
                    inis.IniWriteValue("Settings", "TaskbarControl", "disable");
                    checkBox5.Enabled = false;
                }
            }
            comboBox1.Text = inis.IniReadValue("Settings", "Color");
            tuneCb.Text = inis.IniReadValue("Settings", "Tuning");
            if (inis.IniReadValue("Settings", "LastPreset") == "true")
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Aero") == "enable")
            {
                checkBox2.Checked = true;
            }
            else if (inis.IniReadValue("Settings", "Aero") == "disable")
            {
                checkBox2.Checked = false;
            }
            if (inis.IniReadValue("Settings", "HardwareAcc") == "true")
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }
            if (inis.IniReadValue("Settings", "HideCMD") == "true")
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }
            if (inis.IniReadValue("Settings", "TaskbarControl") == "enable")
            {
                checkBox5.Checked = true;
            }
            else if (inis.IniReadValue("Settings", "TaskbarControl") == "disable")
            {
                checkBox5.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Debug") == "true")
            {
                checkBox6.Checked = true;
            }
            else
            {
                checkBox6.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Openclx264") == "true")
            {
                openclCb.Checked = true;
            }
            else
            {
                openclCb.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Openclx265") == "true")
            {
                checkBox7.Checked = true;
            }
            else
            {
                checkBox7.Checked = false;
            }
            if (inis.IniReadValue("Settings", "ListCheckbox") == "true")
            {
                checkBox8.Checked = true;
            }
            else
            {
                checkBox8.Checked = false;
            }
            if (inis.IniReadValue("Settings", "EnableSleep") == "true")
            {
                checkBox9.Checked = true;
            }
            else
            {
                checkBox9.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Flagsx264") == "+ildct+ilme")
            {
                checkBox10.Checked = true;
            }
            else
            {
                checkBox10.Checked = false;
            }
            if (inis.IniReadValue("Settings", "Recent") == "true")
            {
                checkBox11.Checked = true;
            }
            else
            {
                checkBox11.Checked = false;
            }
            if (inis.IniReadValue("Settings", "EmptyListStopButton") == "true")
            {
                checkBox12.Checked = true;
            }
            else
            {
                checkBox12.Checked = false;
            }
            threadsCb.Text = inis.IniReadValue("Settings", "Threads");
            x264ProfileCb.Text = inis.IniReadValue("Settings", "x264Profile");
            x264ProfileLevelCb.Text = inis.IniReadValue("Settings", "x264ProfileLevel");
            label7.Text = inis.IniReadValue("Settings", "x264crf");
            if (inis.IniReadValue("Settings", "x264crf") != "Nincs" && inis.IniReadValue("Settings", "x264crf") != "")
            {
                trackBar1.Value = int.Parse(inis.IniReadValue("Settings", "x264crf"));
            }
            h264PresetsCb.Text = inis.IniReadValue("Settings", "x264Presets");
            
            int bgCount = 0;
            for (int i = 0; i < 5; i++)
            {
                bgCount++;
                if (inis.IniReadValue("Backgrounds", "bg"+bgCount.ToString()) != "")
                {
                    comboBox2.Items.Add(inis.IniReadValue("Backgrounds", "bg" + bgCount.ToString()));
                }
            }
            comboBox2.Text = inis.IniReadValue("Settings", "MainBackground");
        }


        

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                inis.IniWriteValue("Settings", "LastPreset", "true");

            }
            else
            {
                inis.IniWriteValue("Settings", "LastPreset", "false");

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            inis.IniWriteValue("Settings", "Color", comboBox1.Text);
            comboBox1.Enabled = false;

            if (comboBox1.Text == "Fekete")
                Theme.ThemeColor = RibbonTheme.Black;
            else if (comboBox1.Text == "Zöld")
                Theme.ThemeColor = RibbonTheme.Green;
            else if (comboBox1.Text == "Lila")
                Theme.ThemeColor = RibbonTheme.Purple;
            else if (comboBox1.Text == "JellyBelly")
                Theme.ThemeColor = RibbonTheme.JellyBelly;
            else if (comboBox1.Text == "Halloween")
                Theme.ThemeColor = RibbonTheme.Halloween;
            else
                Theme.ThemeColor = RibbonTheme.Normal;
            //Form1.ActiveForm.Refresh();
            //this.Refresh();
            comboBox1.Enabled = true;
            comboBox1.Focus();
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked== true)
            {
                inis.IniWriteValue("Settings", "Aero", "enable");
            }
            else
            {
                inis.IniWriteValue("Settings", "Aero", "disable");
            }
            
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                inis.IniWriteValue("Settings", "HardwareAcc", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "HardwareAcc", "false");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                inis.IniWriteValue("Settings", "HideCMD", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "HideCMD", "false");
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                inis.IniWriteValue("Settings", "TaskbarControl", "enable");
            }
            else
            {
                inis.IniWriteValue("Settings", "TaskbarControl", "disable");
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
            {
                inis.IniWriteValue("Settings", "Debug", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "Debug", "false");
            }
        }

        private void openclCb_CheckedChanged(object sender, EventArgs e)
        {
            if (openclCb.Checked == true)
            {
                inis.IniWriteValue("Settings", "Openclx264", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "Openclx264", "false");
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                inis.IniWriteValue("Settings", "Openclx265", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "Openclx265", "false");
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                inis.IniWriteValue("Settings", "ListCheckbox", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "ListCheckbox", "false");
            }
        }

        private void tuneCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            inis.IniWriteValue("Settings", "Tuning", tuneCb.SelectedItem.ToString());
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked == true)
            {
                inis.IniWriteValue("Settings", "EnableSleep", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "EnableSleep", "false");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            inis.IniWriteValue("Settings", "MainBackground", comboBox2.Text);
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked == true)
            {
                inis.IniWriteValue("Settings", "Flagsx264", "+ildct+ilme");
            }
            else
            {
                inis.IniWriteValue("Settings", "Flagsx264", "none");
            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked == true)
            {
                inis.IniWriteValue("Settings", "Recent", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "Recent", "false");
            }
        }

        private void threadsCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            inis.IniWriteValue("Settings", "Threads", threadsCb.SelectedItem.ToString());
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked == true)
            {
                inis.IniWriteValue("Settings", "EmptyListStopButton", "true");
            }
            else
            {
                inis.IniWriteValue("Settings", "EmptyListStopButton", "false");
            }
        }

        private void x264ProfileCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (x264ProfileCb.SelectedItem.ToString() != "none")
            {
                inis.IniWriteValue("Settings", "x264Profile", x264ProfileCb.SelectedItem.ToString());
            }
            else
            {
                inis.IniWriteValue("Settings", "x264Profile", "none");
                x264ProfileLevelCb.Text = "none";
            }
        }

        private void x264ProfileLevelCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (x264ProfileLevelCb.SelectedItem.ToString() != "none" && x264ProfileCb.SelectedItem.ToString() != "none")
            {
                inis.IniWriteValue("Settings", "x264ProfileLevel", x264ProfileLevelCb.SelectedItem.ToString());
            }
            if (x264ProfileLevelCb.SelectedItem.ToString() == "none")
            {
                inis.IniWriteValue("Settings", "x264ProfileLevel", "none");
            }
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value == 17)
            {
                label7.Text = "Nincs";
            }
            else
            {
                label7.Text = trackBar1.Value.ToString();
            }
           
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
                inis.IniWriteValue("Settings", "x264crf", label7.Text);
        }

        private void h264PresetsCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (h264PresetsCb.SelectedItem.ToString() != "none" && h264PresetsCb.SelectedItem.ToString() != "none")
            {
                inis.IniWriteValue("Settings", "x264Presets", h264PresetsCb.SelectedItem.ToString());
            }
            if (h264PresetsCb.SelectedItem.ToString() == "none")
            {
                inis.IniWriteValue("Settings", "x264Presets", "none");
            }
        }
        
    }
}

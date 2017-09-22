using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using Ini;
using System.Windows.Forms;

namespace ViennaEncoder
{
    public partial class logfrm : Form
    {
        public logfrm()
        {
            InitializeComponent();
        }
        string temp = Path.GetTempPath();
        string windir = Environment.GetEnvironmentVariable("WINDIR");
        string programfiles = Environment.GetEnvironmentVariable("PROGRAMFILES");
        string allusers = Environment.GetEnvironmentVariable("USERPROFILE");
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        IniFile ini = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
            timer1.Stop();
            timer1.Enabled = false;
        }
        public void InsertText(string text)
        {
            richTextBox1.Text = text;
        }

        private void logfrm_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
            if (ini.IniReadValue("Settings", "LogScroll") == "true")
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            if (File.Exists(temp + "\\vienna-encoder\\commands.txt"))
            {
                StreamReader sr = new StreamReader(temp + "\\vienna-encoder\\commands.txt");
                textBox1.Text = sr.ReadLine();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (File.Exists(temp + "\\vienna-encoder\\file-new.txt"))
            {
                label2.Text = temp + "\\vienna-encoder\\file-new.txt";
                String str = File.ReadAllText(temp + "\\vienna-encoder\\file-new.txt");
                richTextBox1.Text = str;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (ini.IniReadValue("Settings", "LogScroll") == "true")
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length; //Set the current caret position at the end
                richTextBox1.ScrollToCaret(); //Now scroll it automatically
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                ini.IniWriteValue("Settings", "LogScroll", "true");
            }
            else
            {
                ini.IniWriteValue("Settings", "LogScroll", "false");
            }
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Start();
            }
        }

    }
}

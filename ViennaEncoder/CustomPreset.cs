using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ini;
using System.Runtime.InteropServices;
using System.IO;

namespace ViennaEncoder
{
    public partial class CustomPreset : Form
    {
        private readonly Form1 _form1;
        //Konfigurációfájl helye
        IniFile ini1 = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
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

        public CustomPreset(Form1 form1)
        {
             _form1 = form1;
             InitializeComponent();
             ImageList Imagelist = new ImageList();
             listView1.LargeImageList = Imagelist;
             listView1.SmallImageList = Imagelist;
             Imagelist.ImageSize = new System.Drawing.Size(32, 32);
             Image[] iconList = new Image[ini1.GetEntryNames("PresetIcons").Count()];
             for (int i = 0; i < ini1.GetEntryNames("PresetIcons").Count(); i++)
             {
                 string actualIcon = ini1.IniReadValue("PresetIcons", "Preset" + i.ToString());
                 iconList[i] = new Bitmap((Image)Properties.Resources.ResourceManager.GetObject(actualIcon));
                 Imagelist.Images.Add(iconList[i]);
                 listView1.Items.Add(new ListViewItem { ImageIndex = i, Text = ini1.IniReadValue("PresetIcons", "Preset" + i.ToString()) });
             }
             SetWindowTheme(listView1.Handle, "explorer", null); //Explorer style
             SendMessage(listView1.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);

             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                _form1.custompresetSave(listView1.Items.IndexOf(listView1.SelectedItems[0]).ToString(), textBox1.Text);
            }
            else
            {
                _form1.custompresetSave("0", textBox1.Text);
            }
            
            Close();
        }
    }
}

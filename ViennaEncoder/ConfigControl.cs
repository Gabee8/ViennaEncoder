using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ini;
using System.IO;
using System.Xml.Linq;

namespace ViennaEncoder
{
    public partial class ConfigControl : UserControl
    {
        IniFile ini = new IniFile(Environment.CurrentDirectory + "\\Settings.ini");
        const int ERROR_FILE_NOT_FOUND = 2;
        XDocument xdoc = new XDocument();
        XDocument xdoc2 = new XDocument();
        XDocument xdoc3 = new XDocument();
        string filePath3 = Environment.CurrentDirectory + "\\presets.xml";

        string filePath = Environment.CurrentDirectory + "\\vcodecs.xml";
        string filePath2 = Environment.CurrentDirectory + "\\acodecs.xml";
        bool newacodecs = false;
        bool newvcodecs = false;
        bool vcancel = false;

        public ConfigControl()
        {
            InitializeComponent();
            xdoc = XDocument.Load(filePath);
            xdoc2 = XDocument.Load(filePath2);
            xdoc3 = XDocument.Load(filePath3);
            var codecsList = xdoc.Root.Descendants("CodecsList").Elements("CodecsName").Select(x => x.Value).ToList();

            foreach (var codecs in codecsList)
            {
                comboBox1.Items.Add(codecs);
                comboBox1.SelectedIndex = 0;
            }
            var codecsList2 = xdoc2.Root.Descendants("CodecsList").Elements("CodecsName").Select(x => x.Value).ToList();

            foreach (var codecs2 in codecsList2)
            {
                comboBox2.Items.Add(codecs2);
                comboBox2.SelectedIndex = 0;
            }

            presetsloading();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (newvcodecs == true)
                {
                    XDocument doc = XDocument.Load(Environment.CurrentDirectory + "\\vcodecs.xml");
                    XElement root = new XElement("CodecsList");
                    root.Add(new XElement("CodecsName", textBox1.Text));
                    root.Add(new XElement("CodecsParam", textBox2.Text));
                    doc.Element("Codecs").Add(root);
                    doc.Save(Environment.CurrentDirectory + "\\vcodecs.xml");
                    newvcodecs = false;
                }
                else
                {
                    string s = this.comboBox1.SelectedItem as string;

                    if (s != null)
                    {
                        var list = (from xNode in xdoc.Root.Descendants("CodecsList") where xNode.Element("CodecsName").Value == s select xNode);
                        foreach (XElement ele in list)
                        {
                            ele.Element("CodecsName").Value = textBox1.Text;
                            ele.Element("CodecsParam").Value = textBox2.Text;
                            ele.Element("CodecsExt").Value = textBox3.Text;
                            ele.Element("CodecsExt").FirstAttribute.Value = textBox6.Text;
                            xdoc.Save(Environment.CurrentDirectory + "\\vcodecs.xml");
                        }

                    }
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string s = this.comboBox1.SelectedItem as string;

            if (s != null)
            {
                var list = (from xNode in xdoc.Root.Descendants("CodecsList") where xNode.Element("CodecsName").Value == s select xNode);

                string CodecsParam;
                string CodecsExt;
                foreach (XElement ele in list)
                {
                    CodecsParam = ele.Element("CodecsParam").Value;
                    if (ele.Element("CodecsExt") != null)
                    {
                        CodecsExt = ele.Element("CodecsExt").Value;
                        textBox3.Text = CodecsExt;
                        textBox6.Text = ele.Element("CodecsExt").FirstAttribute.Value;
                    }
                    textBox1.Text = comboBox1.SelectedItem.ToString();
                    textBox2.Text = CodecsParam;
                    
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string s = this.comboBox2.SelectedItem as string;

            if (s != null)
            {
                var list = (from xNode in xdoc2.Root.Descendants("CodecsList") where xNode.Element("CodecsName").Value == s select xNode);

                string CodecsParam;
                foreach (XElement ele in list)
                {
                    CodecsParam = ele.Element("CodecsParam").Value;
                    textBox5.Text = comboBox2.SelectedItem.ToString();
                    textBox4.Text = CodecsParam;

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                if (newacodecs == true)
                {
                    XDocument doc = XDocument.Load(Environment.CurrentDirectory + "\\acodecs.xml");
                    XElement root = new XElement("CodecsList");
                    root.Add(new XElement("CodecsName", textBox5.Text));
                    root.Add(new XElement("CodecsParam", textBox4.Text));
                    doc.Element("Codecs").Add(root);
                    doc.Save(Environment.CurrentDirectory + "\\acodecs.xml");
                    newacodecs = false;
                }
                else
                {

                    string s = this.comboBox2.SelectedItem as string;

                    if (s != null)
                    {
                        var list = (from xNode in xdoc2.Root.Descendants("CodecsList") where xNode.Element("CodecsName").Value == s select xNode);
                        foreach (XElement ele in list)
                        {
                            ele.Element("CodecsName").Value = textBox5.Text;
                            ele.Element("CodecsParam").Value = textBox4.Text;
                            xdoc2.Save(Environment.CurrentDirectory + "\\acodecs.xml");
                        }

                    }
                }
            }
        }


        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                textBox5.BackColor = Color.White;
                pictureBox3.Visible = false;
            }
            else
            {
                textBox5.BackColor = Color.LightSalmon;
                pictureBox3.Visible = true;

            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                textBox4.BackColor = Color.White;
                pictureBox4.Visible = false;
            }
            else
            {
                textBox4.BackColor = Color.LightSalmon;
                pictureBox4.Visible = true;
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                textBox1.BackColor = Color.White;
                pictureBox1.Visible = false;
            }
            else
            {
                textBox1.BackColor = Color.LightSalmon;
                pictureBox1.Visible = true;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                textBox2.BackColor = Color.White;
                pictureBox2.Visible = false;
            }
            else
            {
                textBox2.BackColor = Color.LightSalmon;
                pictureBox2.Visible = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                newvcodecs = true;
                textBox1.Text = "";
                textBox1.BackColor = Color.LightSalmon;
                textBox2.Text = "";
                textBox2.BackColor = Color.LightSalmon;
                comboBox1.Text = "";
                checkBox1.Text = "Mégsem";
            }
            else
            {
                checkBox1.Text = "Új Kodek";
                comboBox1.Items.Clear();
                var codecsList = xdoc.Root.Descendants("CodecsList").Elements("CodecsName").Select(x => x.Value).ToList();

                foreach (var codecs in codecsList)
                {
                    comboBox1.Items.Add(codecs);
                    comboBox1.SelectedIndex = 0;
                }
            }
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                newacodecs = true;
                textBox5.Text = "";
                textBox5.BackColor = Color.LightSalmon;
                textBox4.Text = "";
                textBox4.BackColor = Color.LightSalmon;
                comboBox2.Text = "";
                checkBox2.Text = "Mégsem";
            }
            else
            {
                checkBox2.Text = "Új Kodek";
                comboBox2.Items.Clear();
                var codecsList2 = xdoc2.Root.Descendants("CodecsList").Elements("CodecsName").Select(x => x.Value).ToList();

                foreach (var codecs2 in codecsList2)
                {
                    comboBox2.Items.Add(codecs2);
                    comboBox2.SelectedIndex = 0;
                }
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox6.Text != "")
            {
                textBox6.BackColor = Color.White;
                pictureBox5.Visible = false;
            }
            else
            {
                textBox6.BackColor = Color.LightSalmon;
                pictureBox5.Visible = true;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.CharacterCasing = CharacterCasing.Upper;
            if (textBox6.Text != "")
            {
                textBox6.BackColor = Color.White;
                pictureBox5.Visible = false;
                textBox6.CharacterCasing = CharacterCasing.Lower;
                textBox6.Text = "." + textBox3.Text;
            }
            else
            {
                textBox6.BackColor = Color.LightSalmon;
                pictureBox5.Visible = true;
            }
        }

        

        private void presetsloading()
        {
            foreach (XElement element in xdoc3.Descendants("PresetName"))
            {
                if (element.FirstAttribute.Value == "Custom")
                {
                    comboBox3.Items.Add(element.Value);
                    comboBox3.SelectedIndex = 0;
                    button3.Enabled = true;
                }
            }

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (!cb.Focused)
            {
                return;
            }
            string s = this.comboBox3.SelectedItem as string;

            if (s != null)
            {
                var list = (from xNode in xdoc3.Root.Descendants("PresetList") where xNode.Element("PresetName").Value == s select xNode);

                foreach (XElement ele in xdoc3.Descendants("PresetName"))
                {
                    if (ele.Element("PresetName").FirstAttribute.Value == "Original")
                    {
                        button3.Enabled = false;
                    }
                    else if (ele.Element("PresetName").FirstAttribute.Value == "Custom")
                    {
                        button3.Enabled = true;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            xdoc3.Descendants("PresetList").Where(node => (string)node.Element("PresetName") == comboBox3.Text).Remove();
            xdoc3.Save(Environment.CurrentDirectory + "\\presets.xml");
            comboBox3.Items.Clear();
            presetsloading();
            button3.Enabled = false;
        }

    }
}

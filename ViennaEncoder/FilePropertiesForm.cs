using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MediaInfoLib;

namespace ViennaEncoder
{
    public partial class FilePropertiesForm : Form
    {
        public string ReturnedText = "";

        public FilePropertiesForm()
        {
            InitializeComponent();
        }
        private void AddColouredText(string strTextToAdd)
        {
            //Use the RichTextBox to create the initial RTF code
            richTextBox1.Clear();
            richTextBox1.AppendText(strTextToAdd);
            string strRTF = richTextBox1.Rtf;
            richTextBox1.Clear();

            /* 
             * ADD COLOUR TABLE TO THE HEADER FIRST 
             * */

            // Search for colour table info, if it exists (which it shouldn't)
            // remove it and replace with our one
            int iCTableStart = strRTF.IndexOf("colortbl;");

            if (iCTableStart != -1) //then colortbl exists
            {
                //find end of colortbl tab by searching
                //forward from the colortbl tab itself
                int iCTableEnd = strRTF.IndexOf('}', iCTableStart);
                strRTF = strRTF.Remove(iCTableStart, iCTableEnd - iCTableStart);

                //now insert new colour table at index of old colortbl tag
                strRTF = strRTF.Insert(iCTableStart,
                    // CHANGE THIS STRING TO ALTER COLOUR TABLE
                    "colortbl ;\\red255\\green0\\blue0;\\red0\\green128\\blue0;\\red0\\green0\\blue255;}");
            }

            //colour table doesn't exist yet, so let's make one
            else
            {
                // find index of start of header
                int iRTFLoc = strRTF.IndexOf("\\rtf");
                // get index of where we'll insert the colour table
                // try finding opening bracket of first property of header first                
                int iInsertLoc = strRTF.IndexOf('{', iRTFLoc);

                // if there is no property, we'll insert colour table
                // just before the end bracket of the header
                if (iInsertLoc == -1) iInsertLoc = strRTF.IndexOf('}', iRTFLoc) - 1;

                // insert the colour table at our chosen location                
                strRTF = strRTF.Insert(iInsertLoc,
                    // CHANGE THIS STRING TO ALTER COLOUR TABLE
                    "{\\colortbl ;\\red128\\green0\\blue0;\\red0\\green128\\blue0;\\red0\\green0\\blue255;}");
            }

            /*
             * NOW PARSE THROUGH RTF DATA, ADDING RTF COLOUR TAGS WHERE WE WANT THEM
             * In our colour table we defined:
             * cf1 = red  
             * cf2 = green
             * cf3 = blue             
             * */

            for (int i = 0; i < strRTF.Length; i++)
            {
                if (strRTF[i] == '(')
                {
                    //add RTF tags after symbol 
                    //Check for comments tags 
                    if (strRTF[i + 1] == '!')
                        strRTF = strRTF.Insert(i + 4, "\\cf2 ");
                    else
                        strRTF = strRTF.Insert(i + 1, "\\cf1 ");
                    //add RTF before symbol
                    strRTF = strRTF.Insert(i, "\\cf3 ");

                    //skip forward past the characters we've just added
                    //to avoid getting trapped in the loop
                    i += 6;
                }
                else if (strRTF[i] == ')')
                {
                    //add RTF tags after character
                    strRTF = strRTF.Insert(i + 1, "\\cf0 ");
                    //Check for comments tags
                    if (strRTF[i - 1] == '-')
                    {
                        strRTF = strRTF.Insert(i - 2, "\\cf3 ");
                        //skip forward past the 6 characters we've just added
                        i += 8;
                    }
                    else
                    {
                        strRTF = strRTF.Insert(i, "\\cf3 ");
                        //skip forward past the 6 characters we've just added
                        i += 6;
                    }
                }
            }
            richTextBox1.Rtf = strRTF;
        }
        string outputName = "";
        String ToDisplay;
        private void FilePropertiesForm_Load(object sender, EventArgs e)
        {
            MediaInfo MediaInfoLib = new MediaInfo();
            MediaInfoLib.Open(ReturnedText);

            richTextBox1.SelectionTabs = new int[] { 300, 200 };

            int audiocounter = 0;
            int subtitlecounter = 0;
            if (MediaInfoLib.Get(StreamKind.General, 0, "AudioCount") != "")
            {
                audiocounter = int.Parse(MediaInfoLib.Get(StreamKind.General, 0, "AudioCount"));
            }

            if (MediaInfoLib.Get(StreamKind.General, 0, "TextCount") != "" )
            {
                subtitlecounter = int.Parse(MediaInfoLib.Get(StreamKind.General, 0, "TextCount"));
            }

            ToDisplay += "(Általános)\r\n";
            ToDisplay += MediaInfoLib.Get(StreamKind.General, 0, "Inform");

            ToDisplay += "\r\n\r\n(Video)\r\n";
            ToDisplay += MediaInfoLib.Get(StreamKind.Video, 0, "Inform");

            for (int i = 0; i < audiocounter; i++)
            {
                ToDisplay += "\r\n\r\n(Audio)#" + MediaInfoLib.Get(StreamKind.Audio, i, "ID") + "\r\n";
                ToDisplay += MediaInfoLib.Get(StreamKind.Audio, i, "Inform");
            }
            for (int i = 0; i < subtitlecounter; i++)
            {
                ToDisplay += "\r\n\r\n(Felirat)\r\n";
                ToDisplay += MediaInfoLib.Get(StreamKind.Text, i, "Inform");
            }
            
            this.AddColouredText(ToDisplay);
            outputName = Path.GetFileNameWithoutExtension(ReturnedText);
            ReturnedText = "";
            MediaInfoLib.Close();
        }

        private void InfoSaveMenu_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = outputName;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))sw.WriteLine(richTextBox1.Text);
            }
        }

    }
}

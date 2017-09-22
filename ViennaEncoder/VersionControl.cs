using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ViennaEncoder
{
    public partial class VersionControl : UserControl
    {
        public VersionControl()
        {
            InitializeComponent();
            try
            {
                richTextBox2.LoadFile("VersionHistory.rtf", RichTextBoxStreamType.RichText);
            }
            catch (Exception)
            {
            }
        }
    }
}

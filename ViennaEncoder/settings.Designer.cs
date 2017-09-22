namespace ViennaEncoder
{
    partial class settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Általános", 5);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Konfiguráció", 1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Frissítés", 2);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Verzió Történet", 3);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Névjegy", 4);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(settings));
            this.listView1 = new System.Windows.Forms.ListView();
            this.Icons_List = new System.Windows.Forms.ImageList(this.components);
            this.horizontalPanel1 = new WindowsFormsAero.HorizontalPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.themedLabel1 = new WindowsFormsAero.ThemeText.ThemedLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.mainControl1 = new ViennaEncoder.MainControl();
            this.aboutControl1 = new ViennaEncoder.AboutControl();
            this.versionControl1 = new ViennaEncoder.VersionControl();
            this.updateControl1 = new ViennaEncoder.UpdateControl();
            this.configControl1 = new ViennaEncoder.ConfigControl();
            this.horizontalPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5});
            this.listView1.LargeImageList = this.Icons_List;
            this.listView1.Location = new System.Drawing.Point(0, 44);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(132, 352);
            this.listView1.SmallImageList = this.Icons_List;
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            this.listView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CheckEnter);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // Icons_List
            // 
            this.Icons_List.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Icons_List.ImageStream")));
            this.Icons_List.TransparentColor = System.Drawing.Color.Transparent;
            this.Icons_List.Images.SetKeyName(0, "generalMenu_icon.png");
            this.Icons_List.Images.SetKeyName(1, "config.ico");
            this.Icons_List.Images.SetKeyName(2, "update.ico");
            this.Icons_List.Images.SetKeyName(3, "hisory.ico");
            this.Icons_List.Images.SetKeyName(4, "about_icon.ico");
            this.Icons_List.Images.SetKeyName(5, "application-sidebar-list.ico");
            // 
            // horizontalPanel1
            // 
            this.horizontalPanel1.BackColor = System.Drawing.Color.Transparent;
            this.horizontalPanel1.Controls.Add(this.button1);
            this.horizontalPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.horizontalPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.horizontalPanel1.Location = new System.Drawing.Point(0, 395);
            this.horizontalPanel1.Name = "horizontalPanel1";
            this.horizontalPanel1.Size = new System.Drawing.Size(620, 34);
            this.horizontalPanel1.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(535, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // themedLabel1
            // 
            this.themedLabel1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.themedLabel1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.themedLabel1.Location = new System.Drawing.Point(45, 7);
            this.themedLabel1.Name = "themedLabel1";
            this.themedLabel1.Size = new System.Drawing.Size(211, 32);
            this.themedLabel1.TabIndex = 8;
            this.themedLabel1.Text = "Vienna Encoder Beállítások";
            this.themedLabel1.TextAlignVertical = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
            this.themedLabel1.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::ViennaEncoder.Properties.Resources.xp_settings_header;
            this.pictureBox1.Location = new System.Drawing.Point(45, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(210, 32);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.WaitOnLoad = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image = global::ViennaEncoder.Properties.Resources.settingslogo_icon;
            this.pictureBox2.Location = new System.Drawing.Point(15, 11);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(24, 24);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // mainControl1
            // 
            this.mainControl1.BackColor = System.Drawing.Color.White;
            this.mainControl1.Location = new System.Drawing.Point(127, 44);
            this.mainControl1.Name = "mainControl1";
            this.mainControl1.Size = new System.Drawing.Size(493, 352);
            this.mainControl1.TabIndex = 7;
            this.mainControl1.Visible = false;
            // 
            // aboutControl1
            // 
            this.aboutControl1.BackColor = System.Drawing.Color.White;
            this.aboutControl1.Location = new System.Drawing.Point(127, 44);
            this.aboutControl1.Name = "aboutControl1";
            this.aboutControl1.Size = new System.Drawing.Size(493, 352);
            this.aboutControl1.TabIndex = 10;
            this.aboutControl1.Visible = false;
            // 
            // versionControl1
            // 
            this.versionControl1.BackColor = System.Drawing.Color.White;
            this.versionControl1.Location = new System.Drawing.Point(127, 44);
            this.versionControl1.Name = "versionControl1";
            this.versionControl1.Size = new System.Drawing.Size(493, 352);
            this.versionControl1.TabIndex = 9;
            this.versionControl1.Visible = false;
            // 
            // updateControl1
            // 
            this.updateControl1.Location = new System.Drawing.Point(127, 44);
            this.updateControl1.Name = "updateControl1";
            this.updateControl1.Size = new System.Drawing.Size(493, 352);
            this.updateControl1.TabIndex = 12;
            // 
            // configControl1
            // 
            this.configControl1.Location = new System.Drawing.Point(127, 44);
            this.configControl1.Name = "configControl1";
            this.configControl1.Size = new System.Drawing.Size(493, 352);
            this.configControl1.TabIndex = 8;
            this.configControl1.Visible = false;
            // 
            // settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(620, 429);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.themedLabel1);
            this.Controls.Add(this.horizontalPanel1);
            this.Controls.Add(this.mainControl1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "settings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Beállítások és Névjegy";
            this.Load += new System.EventHandler(this.settings_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.settings_MouseDown);
            this.horizontalPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList Icons_List;
        private ViennaEncoder.MainControl mainControl1;
        private ViennaEncoder.VersionControl versionControl1;
        public ViennaEncoder.AboutControl aboutControl1;
        public System.Windows.Forms.ListView listView1;
        public ViennaEncoder.UpdateControl updateControl1;
        private WindowsFormsAero.HorizontalPanel horizontalPanel1;
        public System.Windows.Forms.Button button1;
        private ViennaEncoder.ConfigControl configControl1;
        private WindowsFormsAero.ThemeText.ThemedLabel themedLabel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
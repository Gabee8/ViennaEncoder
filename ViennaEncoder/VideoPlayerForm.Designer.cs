namespace ViennaEncoder
{
    partial class VideoPlayerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoPlayerForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStartpoz = new System.Windows.Forms.Label();
            this.lblEndpoz = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.vidMute = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.vidCutS = new System.Windows.Forms.ToolStripButton();
            this.vidCutE = new System.Windows.Forms.ToolStripButton();
            this.vidCut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.vidPlay = new System.Windows.Forms.ToolStripButton();
            this.vidPause = new System.Windows.Forms.ToolStripButton();
            this.vidStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.vidRew = new System.Windows.Forms.ToolStripButton();
            this.vidFF = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.vidBChap = new System.Windows.Forms.ToolStripButton();
            this.vidNChap = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.loadedVideoLb = new System.Windows.Forms.ToolStripStatusLabel();
            this.loadedVideo = new System.Windows.Forms.ToolStripStatusLabel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.shadowPanel1 = new ShadowPanel.ShadowPanel();
            this.differentLb = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LenghtVidLb = new System.Windows.Forms.Label();
            this.vidLenght = new System.Windows.Forms.Label();
            this.actualPoz = new System.Windows.Forms.Label();
            this.VideocutSumLb = new System.Windows.Forms.Label();
            this.ActualPosVidLb = new System.Windows.Forms.Label();
            this.countLabel = new System.Windows.Forms.Label();
            this.PanelSperatorImage = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.ListUpBt = new System.Windows.Forms.ToolStripButton();
            this.ListDownBt = new System.Windows.Forms.ToolStripButton();
            this.ListDelBt = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.shadowPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelSperatorImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Location = new System.Drawing.Point(13, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(675, 334);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDoubleClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Kezdő pozíció:";
            // 
            // lblStartpoz
            // 
            this.lblStartpoz.AutoSize = true;
            this.lblStartpoz.BackColor = System.Drawing.Color.Transparent;
            this.lblStartpoz.Location = new System.Drawing.Point(95, 77);
            this.lblStartpoz.Name = "lblStartpoz";
            this.lblStartpoz.Size = new System.Drawing.Size(49, 13);
            this.lblStartpoz.TabIndex = 30;
            this.lblStartpoz.Text = "00:00:00";
            // 
            // lblEndpoz
            // 
            this.lblEndpoz.AutoSize = true;
            this.lblEndpoz.BackColor = System.Drawing.Color.Transparent;
            this.lblEndpoz.Location = new System.Drawing.Point(95, 96);
            this.lblEndpoz.Name = "lblEndpoz";
            this.lblEndpoz.Size = new System.Drawing.Size(49, 13);
            this.lblEndpoz.TabIndex = 29;
            this.lblEndpoz.Text = "00:00:00";
            // 
            // trackBar2
            // 
            this.trackBar2.AutoSize = false;
            this.trackBar2.Location = new System.Drawing.Point(13, 378);
            this.trackBar2.Maximum = 100;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(107, 22);
            this.trackBar2.TabIndex = 28;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar2.Value = 20;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // trackBar1
            // 
            this.trackBar1.AutoSize = false;
            this.trackBar1.Location = new System.Drawing.Point(13, 344);
            this.trackBar1.Maximum = 1000;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(675, 24);
            this.trackBar1.TabIndex = 27;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.vidMute,
            this.toolStripSeparator4,
            this.vidCutS,
            this.vidCutE,
            this.vidCut,
            this.toolStripSeparator1,
            this.vidPlay,
            this.vidPause,
            this.vidStop,
            this.toolStripSeparator2,
            this.vidRew,
            this.vidFF,
            this.toolStripSeparator3,
            this.vidBChap,
            this.vidNChap});
            this.toolStrip1.Location = new System.Drawing.Point(123, 371);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(336, 34);
            this.toolStrip1.TabIndex = 33;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // vidMute
            // 
            this.vidMute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidMute.Image = global::ViennaEncoder.Properties.Resources.muteIcon;
            this.vidMute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidMute.Name = "vidMute";
            this.vidMute.Size = new System.Drawing.Size(23, 31);
            this.vidMute.Text = "Némítás";
            this.vidMute.Click += new System.EventHandler(this.vidMute_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 34);
            // 
            // vidCutS
            // 
            this.vidCutS.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidCutS.Image = global::ViennaEncoder.Properties.Resources.startPozIcon;
            this.vidCutS.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidCutS.Name = "vidCutS";
            this.vidCutS.Size = new System.Drawing.Size(23, 31);
            this.vidCutS.Text = "Kezdő pozíció";
            this.vidCutS.Click += new System.EventHandler(this.vidCutS_Click);
            // 
            // vidCutE
            // 
            this.vidCutE.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidCutE.Image = global::ViennaEncoder.Properties.Resources.endPozIcon;
            this.vidCutE.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidCutE.Name = "vidCutE";
            this.vidCutE.Size = new System.Drawing.Size(23, 31);
            this.vidCutE.Text = "Vég pozíció";
            this.vidCutE.Click += new System.EventHandler(this.vidCutE_Click);
            // 
            // vidCut
            // 
            this.vidCut.AutoSize = false;
            this.vidCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidCut.Image = global::ViennaEncoder.Properties.Resources.cutvideoIcon;
            this.vidCut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidCut.Name = "vidCut";
            this.vidCut.Size = new System.Drawing.Size(26, 31);
            this.vidCut.Text = "toolStripButton10";
            this.vidCut.Click += new System.EventHandler(this.vidCut_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 34);
            // 
            // vidPlay
            // 
            this.vidPlay.AutoSize = false;
            this.vidPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidPlay.Image = global::ViennaEncoder.Properties.Resources.media_playback_start;
            this.vidPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidPlay.Name = "vidPlay";
            this.vidPlay.Size = new System.Drawing.Size(26, 31);
            this.vidPlay.Text = "Lejátszás";
            this.vidPlay.Click += new System.EventHandler(this.vidPlay_Click);
            // 
            // vidPause
            // 
            this.vidPause.AutoSize = false;
            this.vidPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidPause.Image = global::ViennaEncoder.Properties.Resources.media_playback_pause;
            this.vidPause.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidPause.Name = "vidPause";
            this.vidPause.Size = new System.Drawing.Size(26, 31);
            this.vidPause.Text = "Szünet";
            this.vidPause.Click += new System.EventHandler(this.vidPause_Click);
            // 
            // vidStop
            // 
            this.vidStop.AutoSize = false;
            this.vidStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidStop.Image = global::ViennaEncoder.Properties.Resources.media_playback_stop;
            this.vidStop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidStop.Name = "vidStop";
            this.vidStop.Size = new System.Drawing.Size(26, 31);
            this.vidStop.Text = "Leállítás";
            this.vidStop.Click += new System.EventHandler(this.vidStop_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 34);
            // 
            // vidRew
            // 
            this.vidRew.AutoSize = false;
            this.vidRew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidRew.Image = global::ViennaEncoder.Properties.Resources.media_seek_backward;
            this.vidRew.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidRew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidRew.Name = "vidRew";
            this.vidRew.Size = new System.Drawing.Size(26, 31);
            this.vidRew.Text = "Visszatekerés";
            this.vidRew.Click += new System.EventHandler(this.vidRew_Click);
            // 
            // vidFF
            // 
            this.vidFF.AutoSize = false;
            this.vidFF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidFF.Image = global::ViennaEncoder.Properties.Resources.media_seek_forward;
            this.vidFF.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.vidFF.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidFF.Name = "vidFF";
            this.vidFF.Size = new System.Drawing.Size(26, 31);
            this.vidFF.Text = "Előretekerés";
            this.vidFF.Click += new System.EventHandler(this.vidFF_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 34);
            // 
            // vidBChap
            // 
            this.vidBChap.AutoSize = false;
            this.vidBChap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidBChap.Image = global::ViennaEncoder.Properties.Resources.backChapIcon;
            this.vidBChap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidBChap.Name = "vidBChap";
            this.vidBChap.Size = new System.Drawing.Size(23, 31);
            this.vidBChap.Text = "Előző jelenet";
            this.vidBChap.Click += new System.EventHandler(this.vidBChap_Click);
            // 
            // vidNChap
            // 
            this.vidNChap.AutoSize = false;
            this.vidNChap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.vidNChap.Image = global::ViennaEncoder.Properties.Resources.nextChapIcon;
            this.vidNChap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.vidNChap.Name = "vidNChap";
            this.vidNChap.Size = new System.Drawing.Size(23, 31);
            this.vidNChap.Text = "Következő jelenet";
            this.vidNChap.Click += new System.EventHandler(this.vidNChap_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadedVideoLb,
            this.loadedVideo});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 607);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(700, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.Stretch = false;
            this.statusStrip1.TabIndex = 34;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // loadedVideoLb
            // 
            this.loadedVideoLb.Name = "loadedVideoLb";
            this.loadedVideoLb.Size = new System.Drawing.Size(89, 17);
            this.loadedVideoLb.Text = "Betöltött Videó:";
            // 
            // loadedVideo
            // 
            this.loadedVideo.Name = "loadedVideo";
            this.loadedVideo.Size = new System.Drawing.Size(35, 17);
            this.loadedVideo.Text = "nincs";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(13, 410);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(447, 180);
            this.listView1.TabIndex = 35;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Fájlnév";
            this.columnHeader1.Width = 171;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Kezdőpozíció";
            this.columnHeader2.Width = 89;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Végpozíció";
            this.columnHeader3.Width = 85;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Vágási hossz";
            this.columnHeader4.Width = 87;
            // 
            // shadowPanel1
            // 
            this.shadowPanel1.BackgroundImage = global::ViennaEncoder.Properties.Resources.panelbg;
            this.shadowPanel1.BorderColor = System.Drawing.Color.LightBlue;
            this.shadowPanel1.Controls.Add(this.differentLb);
            this.shadowPanel1.Controls.Add(this.label3);
            this.shadowPanel1.Controls.Add(this.label1);
            this.shadowPanel1.Controls.Add(this.LenghtVidLb);
            this.shadowPanel1.Controls.Add(this.vidLenght);
            this.shadowPanel1.Controls.Add(this.actualPoz);
            this.shadowPanel1.Controls.Add(this.VideocutSumLb);
            this.shadowPanel1.Controls.Add(this.ActualPosVidLb);
            this.shadowPanel1.Controls.Add(this.label2);
            this.shadowPanel1.Controls.Add(this.countLabel);
            this.shadowPanel1.Controls.Add(this.lblStartpoz);
            this.shadowPanel1.Controls.Add(this.PanelSperatorImage);
            this.shadowPanel1.Controls.Add(this.lblEndpoz);
            this.shadowPanel1.Controls.Add(this.pictureBox3);
            this.shadowPanel1.Location = new System.Drawing.Point(483, 369);
            this.shadowPanel1.Name = "shadowPanel1";
            this.shadowPanel1.PanelColor = System.Drawing.Color.Empty;
            this.shadowPanel1.Size = new System.Drawing.Size(221, 197);
            this.shadowPanel1.TabIndex = 47;
            // 
            // differentLb
            // 
            this.differentLb.AutoSize = true;
            this.differentLb.BackColor = System.Drawing.Color.Transparent;
            this.differentLb.Location = new System.Drawing.Point(95, 115);
            this.differentLb.Name = "differentLb";
            this.differentLb.Size = new System.Drawing.Size(49, 13);
            this.differentLb.TabIndex = 52;
            this.differentLb.Text = "00:00:00";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 51;
            this.label3.Text = "Vágott hossz:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 50;
            this.label1.Text = "Vég pozíció:";
            // 
            // LenghtVidLb
            // 
            this.LenghtVidLb.AutoSize = true;
            this.LenghtVidLb.Location = new System.Drawing.Point(50, 41);
            this.LenghtVidLb.Name = "LenghtVidLb";
            this.LenghtVidLb.Size = new System.Drawing.Size(39, 13);
            this.LenghtVidLb.TabIndex = 11;
            this.LenghtVidLb.Text = "Hossz:";
            // 
            // vidLenght
            // 
            this.vidLenght.AutoSize = true;
            this.vidLenght.Location = new System.Drawing.Point(95, 41);
            this.vidLenght.Name = "vidLenght";
            this.vidLenght.Size = new System.Drawing.Size(49, 13);
            this.vidLenght.TabIndex = 49;
            this.vidLenght.Text = "00:00:00";
            // 
            // actualPoz
            // 
            this.actualPoz.AutoSize = true;
            this.actualPoz.BackColor = System.Drawing.Color.Transparent;
            this.actualPoz.Location = new System.Drawing.Point(95, 23);
            this.actualPoz.Name = "actualPoz";
            this.actualPoz.Size = new System.Drawing.Size(49, 13);
            this.actualPoz.TabIndex = 48;
            this.actualPoz.Text = "00:00:00";
            // 
            // VideocutSumLb
            // 
            this.VideocutSumLb.AutoSize = true;
            this.VideocutSumLb.BackColor = System.Drawing.Color.Transparent;
            this.VideocutSumLb.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.VideocutSumLb.ForeColor = System.Drawing.Color.SteelBlue;
            this.VideocutSumLb.Location = new System.Drawing.Point(95, 2);
            this.VideocutSumLb.Name = "VideocutSumLb";
            this.VideocutSumLb.Size = new System.Drawing.Size(58, 13);
            this.VideocutSumLb.TabIndex = 45;
            this.VideocutSumLb.Text = "Videóinfó";
            // 
            // ActualPosVidLb
            // 
            this.ActualPosVidLb.AutoSize = true;
            this.ActualPosVidLb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ActualPosVidLb.ForeColor = System.Drawing.Color.Navy;
            this.ActualPosVidLb.Location = new System.Drawing.Point(3, 23);
            this.ActualPosVidLb.Name = "ActualPosVidLb";
            this.ActualPosVidLb.Size = new System.Drawing.Size(86, 13);
            this.ActualPosVidLb.TabIndex = 14;
            this.ActualPosVidLb.Text = "Aktuális Pozíció:";
            // 
            // countLabel
            // 
            this.countLabel.AutoSize = true;
            this.countLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.countLabel.ForeColor = System.Drawing.Color.DimGray;
            this.countLabel.Location = new System.Drawing.Point(3, 175);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(40, 13);
            this.countLabel.TabIndex = 44;
            this.countLabel.Text = "0 elem";
            // 
            // PanelSperatorImage
            // 
            this.PanelSperatorImage.Image = global::ViennaEncoder.Properties.Resources.panelTopSperator;
            this.PanelSperatorImage.Location = new System.Drawing.Point(2, 57);
            this.PanelSperatorImage.Name = "PanelSperatorImage";
            this.PanelSperatorImage.Size = new System.Drawing.Size(209, 11);
            this.PanelSperatorImage.TabIndex = 0;
            this.PanelSperatorImage.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Image = global::ViennaEncoder.Properties.Resources.panelBottomSperator;
            this.pictureBox3.Location = new System.Drawing.Point(3, 167);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(208, 5);
            this.pictureBox3.TabIndex = 43;
            this.pictureBox3.TabStop = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(518, 572);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 48;
            this.cancelButton.Text = "Mégsem";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(615, 572);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(73, 23);
            this.applyButton.TabIndex = 49;
            this.applyButton.Text = "OK";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // toolStrip2
            // 
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ListUpBt,
            this.ListDownBt,
            this.ListDelBt});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip2.Location = new System.Drawing.Point(462, 405);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(21, 92);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // ListUpBt
            // 
            this.ListUpBt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ListUpBt.Image = global::ViennaEncoder.Properties.Resources.up;
            this.ListUpBt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ListUpBt.Name = "ListUpBt";
            this.ListUpBt.Size = new System.Drawing.Size(19, 20);
            this.ListUpBt.Text = "Mozgatás Fel";
            this.ListUpBt.Click += new System.EventHandler(this.ListUpBt_Click);
            // 
            // ListDownBt
            // 
            this.ListDownBt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ListDownBt.Image = global::ViennaEncoder.Properties.Resources.down;
            this.ListDownBt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ListDownBt.Name = "ListDownBt";
            this.ListDownBt.Size = new System.Drawing.Size(19, 20);
            this.ListDownBt.Text = "Mozgatás Le";
            this.ListDownBt.Click += new System.EventHandler(this.ListDownBt_Click);
            // 
            // ListDelBt
            // 
            this.ListDelBt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ListDelBt.Image = global::ViennaEncoder.Properties.Resources.remove;
            this.ListDelBt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ListDelBt.Name = "ListDelBt";
            this.ListDelBt.Size = new System.Drawing.Size(19, 20);
            this.ListDelBt.Text = "Törlés";
            this.ListDelBt.Click += new System.EventHandler(this.ListDelBt_Click);
            // 
            // VideoPlayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(700, 629);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.shadowPanel1);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "VideoPlayerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Videóvágás";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoPlayerForm_FormClosing);
            this.Load += new System.EventHandler(this.VideoPlayerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.shadowPanel1.ResumeLayout(false);
            this.shadowPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelSperatorImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStartpoz;
        private System.Windows.Forms.Label lblEndpoz;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton vidCutS;
        private System.Windows.Forms.ToolStripButton vidCutE;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton vidPlay;
        private System.Windows.Forms.ToolStripButton vidPause;
        private System.Windows.Forms.ToolStripButton vidStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton vidRew;
        private System.Windows.Forms.ToolStripButton vidFF;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton vidBChap;
        private System.Windows.Forms.ToolStripButton vidNChap;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListView listView1;
        private ShadowPanel.ShadowPanel shadowPanel1;
        private System.Windows.Forms.Label LenghtVidLb;
        private System.Windows.Forms.Label vidLenght;
        private System.Windows.Forms.Label actualPoz;
        private System.Windows.Forms.Label VideocutSumLb;
        private System.Windows.Forms.Label ActualPosVidLb;
        private System.Windows.Forms.Label countLabel;
        private System.Windows.Forms.PictureBox PanelSperatorImage;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.ToolStripButton vidCut;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripButton vidMute;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripStatusLabel loadedVideoLb;
        private System.Windows.Forms.ToolStripStatusLabel loadedVideo;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label differentLb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton ListUpBt;
        private System.Windows.Forms.ToolStripButton ListDownBt;
        private System.Windows.Forms.ToolStripButton ListDelBt;
    }
}
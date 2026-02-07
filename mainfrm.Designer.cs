namespace datvreceiver
{
    partial class mainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.titleBarPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMaximize = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.MiddleVideoSplitter = new System.Windows.Forms.SplitContainer();
            this.LeftVideoSplitter = new System.Windows.Forms.SplitContainer();
            this.lab_rx1_nothing = new datvreceiver.OutlinedLabel();
            this.nolock_rx1 = new System.Windows.Forms.PictureBox();
            this.videoRx1 = new LibVLCSharp.WinForms.VideoView();
            this.lab_rx3_nothing = new datvreceiver.OutlinedLabel();
            this.nolock_rx3 = new System.Windows.Forms.PictureBox();
            this.videoRx3 = new LibVLCSharp.WinForms.VideoView();
            this.RightVideoSplitter = new System.Windows.Forms.SplitContainer();
            this.lab_rx2_nothing = new datvreceiver.OutlinedLabel();
            this.nolock_rx2 = new System.Windows.Forms.PictureBox();
            this.videoRx2 = new LibVLCSharp.WinForms.VideoView();
            this.lab_rx4_nothing = new datvreceiver.OutlinedLabel();
            this.nolock_rx4 = new System.Windows.Forms.PictureBox();
            this.videoRx4 = new LibVLCSharp.WinForms.VideoView();

            this.titleBarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MiddleVideoSplitter)).BeginInit();
            this.MiddleVideoSplitter.Panel1.SuspendLayout();
            this.MiddleVideoSplitter.Panel2.SuspendLayout();
            this.MiddleVideoSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftVideoSplitter)).BeginInit();
            this.LeftVideoSplitter.Panel1.SuspendLayout();
            this.LeftVideoSplitter.Panel2.SuspendLayout();
            this.LeftVideoSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightVideoSplitter)).BeginInit();
            this.RightVideoSplitter.Panel1.SuspendLayout();
            this.RightVideoSplitter.Panel2.SuspendLayout();
            this.RightVideoSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx4)).BeginInit();
            this.SuspendLayout();

            //
            // titleBarPanel
            //
            this.titleBarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.titleBarPanel.Controls.Add(this.btnClose);
            this.titleBarPanel.Controls.Add(this.btnMaximize);
            this.titleBarPanel.Controls.Add(this.btnMinimize);
            this.titleBarPanel.Controls.Add(this.lblTitle);
            this.titleBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleBarPanel.Location = new System.Drawing.Point(0, 0);
            this.titleBarPanel.Name = "titleBarPanel";
            this.titleBarPanel.Size = new System.Drawing.Size(1620, 32);
            this.titleBarPanel.TabIndex = 1;
            this.titleBarPanel.Visible = false;
            this.titleBarPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.titleBarPanel_MouseDown);
            this.titleBarPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.titleBarPanel_MouseMove);
            this.titleBarPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.titleBarPanel_MouseUp);
            this.titleBarPanel.DoubleClick += new System.EventHandler(this.titleBarPanel_DoubleClick);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1574, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(46, 32);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseEnter += new System.EventHandler(this.btnClose_MouseEnter);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnClose_MouseLeave);
            //
            // btnMaximize
            //
            this.btnMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximize.FlatAppearance.BorderSize = 0;
            this.btnMaximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnMaximize.ForeColor = System.Drawing.Color.White;
            this.btnMaximize.Location = new System.Drawing.Point(1528, 0);
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.Size = new System.Drawing.Size(46, 32);
            this.btnMaximize.TabIndex = 2;
            this.btnMaximize.Text = "\u25A1";
            this.btnMaximize.UseVisualStyleBackColor = true;
            this.btnMaximize.Click += new System.EventHandler(this.btnMaximize_Click);
            //
            // btnMinimize
            //
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(1482, 0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(46, 32);
            this.btnMinimize.TabIndex = 1;
            this.btnMinimize.Text = "\u2014";
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(8, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(172, 15);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Winterhill Client for Repeaters";
            //
            // MiddleVideoSplitter
            //
            this.MiddleVideoSplitter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MiddleVideoSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MiddleVideoSplitter.Location = new System.Drawing.Point(0, 0);
            this.MiddleVideoSplitter.Name = "MiddleVideoSplitter";
            //
            // MiddleVideoSplitter.Panel1
            //
            this.MiddleVideoSplitter.Panel1.Controls.Add(this.LeftVideoSplitter);
            //
            // MiddleVideoSplitter.Panel2
            //
            this.MiddleVideoSplitter.Panel2.Controls.Add(this.RightVideoSplitter);
            this.MiddleVideoSplitter.Size = new System.Drawing.Size(1620, 900);
            this.MiddleVideoSplitter.SplitterDistance = 802;
            this.MiddleVideoSplitter.TabIndex = 0;

            //
            // LeftVideoSplitter
            //
            this.LeftVideoSplitter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LeftVideoSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftVideoSplitter.Location = new System.Drawing.Point(0, 0);
            this.LeftVideoSplitter.Name = "LeftVideoSplitter";
            this.LeftVideoSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // LeftVideoSplitter.Panel1
            //
            this.LeftVideoSplitter.Panel1.Controls.Add(this.lab_rx1_nothing);
            this.LeftVideoSplitter.Panel1.Controls.Add(this.nolock_rx1);
            this.LeftVideoSplitter.Panel1.Controls.Add(this.videoRx1);
            //
            // LeftVideoSplitter.Panel2
            //
            this.LeftVideoSplitter.Panel2.Controls.Add(this.lab_rx3_nothing);
            this.LeftVideoSplitter.Panel2.Controls.Add(this.nolock_rx3);
            this.LeftVideoSplitter.Panel2.Controls.Add(this.videoRx3);
            this.LeftVideoSplitter.Size = new System.Drawing.Size(802, 900);
            this.LeftVideoSplitter.SplitterDistance = 448;
            this.LeftVideoSplitter.TabIndex = 0;

            //
            // lab_rx1_nothing
            //
            this.lab_rx1_nothing.AutoSize = true;
            this.lab_rx1_nothing.BackColor = System.Drawing.Color.Transparent;
            this.lab_rx1_nothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_rx1_nothing.ForeColor = System.Drawing.Color.White;
            this.lab_rx1_nothing.Location = new System.Drawing.Point(4, 9);
            this.lab_rx1_nothing.Name = "lab_rx1_nothing";
            this.lab_rx1_nothing.Size = new System.Drawing.Size(108, 31);
            this.lab_rx1_nothing.TabIndex = 3;
            this.lab_rx1_nothing.Text = "";

            //
            // nolock_rx1
            //
            this.nolock_rx1.BackColor = System.Drawing.Color.Black;
            this.nolock_rx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nolock_rx1.ErrorImage = null;
            this.nolock_rx1.Image = global::datvreceiver.Properties.Resources.no_signal;
            this.nolock_rx1.InitialImage = null;
            this.nolock_rx1.Location = new System.Drawing.Point(0, 0);
            this.nolock_rx1.Name = "nolock_rx1";
            this.nolock_rx1.Size = new System.Drawing.Size(800, 446);
            this.nolock_rx1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.nolock_rx1.TabIndex = 2;
            this.nolock_rx1.TabStop = false;
            this.nolock_rx1.DoubleClick += new System.EventHandler(this.nolock_rx1_DoubleClick);

            //
            // videoRx1
            //
            this.videoRx1.BackColor = System.Drawing.Color.Black;
            this.videoRx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoRx1.Location = new System.Drawing.Point(0, 0);
            this.videoRx1.MediaPlayer = null;
            this.videoRx1.Name = "videoRx1";
            this.videoRx1.Size = new System.Drawing.Size(800, 446);
            this.videoRx1.TabIndex = 0;
            this.videoRx1.Text = "videoView1";
            this.videoRx1.DoubleClick += new System.EventHandler(this.videoRx1_DoubleClick);

            //
            // lab_rx3_nothing
            //
            this.lab_rx3_nothing.AutoSize = true;
            this.lab_rx3_nothing.BackColor = System.Drawing.Color.Transparent;
            this.lab_rx3_nothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_rx3_nothing.ForeColor = System.Drawing.Color.White;
            this.lab_rx3_nothing.Location = new System.Drawing.Point(4, 11);
            this.lab_rx3_nothing.Name = "lab_rx3_nothing";
            this.lab_rx3_nothing.Size = new System.Drawing.Size(108, 31);
            this.lab_rx3_nothing.TabIndex = 4;
            this.lab_rx3_nothing.Text = "";

            //
            // nolock_rx3
            //
            this.nolock_rx3.BackColor = System.Drawing.Color.Black;
            this.nolock_rx3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nolock_rx3.ErrorImage = null;
            this.nolock_rx3.Image = global::datvreceiver.Properties.Resources.no_signal;
            this.nolock_rx3.InitialImage = null;
            this.nolock_rx3.Location = new System.Drawing.Point(0, 0);
            this.nolock_rx3.Name = "nolock_rx3";
            this.nolock_rx3.Size = new System.Drawing.Size(800, 446);
            this.nolock_rx3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.nolock_rx3.TabIndex = 1;
            this.nolock_rx3.TabStop = false;
            this.nolock_rx3.DoubleClick += new System.EventHandler(this.nolock_rx3_DoubleClick);

            //
            // videoRx3
            //
            this.videoRx3.BackColor = System.Drawing.Color.Black;
            this.videoRx3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoRx3.Location = new System.Drawing.Point(0, 0);
            this.videoRx3.MediaPlayer = null;
            this.videoRx3.Name = "videoRx3";
            this.videoRx3.Size = new System.Drawing.Size(800, 446);
            this.videoRx3.TabIndex = 0;
            this.videoRx3.Text = "videoView3";
            this.videoRx3.DoubleClick += new System.EventHandler(this.videoRx3_DoubleClick);

            //
            // RightVideoSplitter
            //
            this.RightVideoSplitter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RightVideoSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightVideoSplitter.Location = new System.Drawing.Point(0, 0);
            this.RightVideoSplitter.Name = "RightVideoSplitter";
            this.RightVideoSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // RightVideoSplitter.Panel1
            //
            this.RightVideoSplitter.Panel1.Controls.Add(this.lab_rx2_nothing);
            this.RightVideoSplitter.Panel1.Controls.Add(this.nolock_rx2);
            this.RightVideoSplitter.Panel1.Controls.Add(this.videoRx2);
            //
            // RightVideoSplitter.Panel2
            //
            this.RightVideoSplitter.Panel2.Controls.Add(this.lab_rx4_nothing);
            this.RightVideoSplitter.Panel2.Controls.Add(this.nolock_rx4);
            this.RightVideoSplitter.Panel2.Controls.Add(this.videoRx4);
            this.RightVideoSplitter.Size = new System.Drawing.Size(814, 900);
            this.RightVideoSplitter.SplitterDistance = 448;
            this.RightVideoSplitter.TabIndex = 0;

            //
            // lab_rx2_nothing
            //
            this.lab_rx2_nothing.AutoSize = true;
            this.lab_rx2_nothing.BackColor = System.Drawing.Color.Transparent;
            this.lab_rx2_nothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_rx2_nothing.ForeColor = System.Drawing.Color.White;
            this.lab_rx2_nothing.Location = new System.Drawing.Point(10, 9);
            this.lab_rx2_nothing.Name = "lab_rx2_nothing";
            this.lab_rx2_nothing.Size = new System.Drawing.Size(108, 31);
            this.lab_rx2_nothing.TabIndex = 4;
            this.lab_rx2_nothing.Text = "";

            //
            // nolock_rx2
            //
            this.nolock_rx2.BackColor = System.Drawing.Color.Black;
            this.nolock_rx2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nolock_rx2.ErrorImage = null;
            this.nolock_rx2.Image = global::datvreceiver.Properties.Resources.no_signal;
            this.nolock_rx2.InitialImage = null;
            this.nolock_rx2.Location = new System.Drawing.Point(0, 0);
            this.nolock_rx2.Name = "nolock_rx2";
            this.nolock_rx2.Size = new System.Drawing.Size(812, 446);
            this.nolock_rx2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.nolock_rx2.TabIndex = 1;
            this.nolock_rx2.TabStop = false;
            this.nolock_rx2.DoubleClick += new System.EventHandler(this.nolock_rx2_DoubleClick);

            //
            // videoRx2
            //
            this.videoRx2.BackColor = System.Drawing.Color.Black;
            this.videoRx2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoRx2.Location = new System.Drawing.Point(0, 0);
            this.videoRx2.MediaPlayer = null;
            this.videoRx2.Name = "videoRx2";
            this.videoRx2.Size = new System.Drawing.Size(812, 446);
            this.videoRx2.TabIndex = 0;
            this.videoRx2.Text = "videoView2";
            this.videoRx2.DoubleClick += new System.EventHandler(this.videoRx2_DoubleClick);

            //
            // lab_rx4_nothing
            //
            this.lab_rx4_nothing.AutoSize = true;
            this.lab_rx4_nothing.BackColor = System.Drawing.Color.Transparent;
            this.lab_rx4_nothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lab_rx4_nothing.ForeColor = System.Drawing.Color.White;
            this.lab_rx4_nothing.Location = new System.Drawing.Point(10, 11);
            this.lab_rx4_nothing.Name = "lab_rx4_nothing";
            this.lab_rx4_nothing.Size = new System.Drawing.Size(108, 31);
            this.lab_rx4_nothing.TabIndex = 5;
            this.lab_rx4_nothing.Text = "";

            //
            // nolock_rx4
            //
            this.nolock_rx4.BackColor = System.Drawing.Color.Black;
            this.nolock_rx4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nolock_rx4.ErrorImage = null;
            this.nolock_rx4.Image = global::datvreceiver.Properties.Resources.no_signal;
            this.nolock_rx4.InitialImage = null;
            this.nolock_rx4.Location = new System.Drawing.Point(0, 0);
            this.nolock_rx4.Name = "nolock_rx4";
            this.nolock_rx4.Size = new System.Drawing.Size(812, 446);
            this.nolock_rx4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.nolock_rx4.TabIndex = 1;
            this.nolock_rx4.TabStop = false;
            this.nolock_rx4.DoubleClick += new System.EventHandler(this.nolock_rx4_DoubleClick);

            //
            // videoRx4
            //
            this.videoRx4.BackColor = System.Drawing.Color.Black;
            this.videoRx4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoRx4.Location = new System.Drawing.Point(0, 0);
            this.videoRx4.MediaPlayer = null;
            this.videoRx4.Name = "videoRx4";
            this.videoRx4.Size = new System.Drawing.Size(812, 446);
            this.videoRx4.TabIndex = 0;
            this.videoRx4.Text = "videoView4";
            this.videoRx4.DoubleClick += new System.EventHandler(this.videoRx4_DoubleClick);

            //
            // mainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1620, 900);
            this.Controls.Add(this.titleBarPanel);
            this.Controls.Add(this.MiddleVideoSplitter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "mainForm";
            this.Text = "Winterhill Client for Repeaters";
            this.Load += new System.EventHandler(this.mainForm_Load);

            this.titleBarPanel.ResumeLayout(false);
            this.titleBarPanel.PerformLayout();
            this.MiddleVideoSplitter.Panel1.ResumeLayout(false);
            this.MiddleVideoSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MiddleVideoSplitter)).EndInit();
            this.MiddleVideoSplitter.ResumeLayout(false);
            this.LeftVideoSplitter.Panel1.ResumeLayout(false);
            this.LeftVideoSplitter.Panel1.PerformLayout();
            this.LeftVideoSplitter.Panel2.ResumeLayout(false);
            this.LeftVideoSplitter.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftVideoSplitter)).EndInit();
            this.LeftVideoSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx3)).EndInit();
            this.RightVideoSplitter.Panel1.ResumeLayout(false);
            this.RightVideoSplitter.Panel1.PerformLayout();
            this.RightVideoSplitter.Panel2.ResumeLayout(false);
            this.RightVideoSplitter.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightVideoSplitter)).EndInit();
            this.RightVideoSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nolock_rx4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoRx4)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel titleBarPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMaximize;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.SplitContainer MiddleVideoSplitter;
        private System.Windows.Forms.SplitContainer LeftVideoSplitter;
        private System.Windows.Forms.SplitContainer RightVideoSplitter;
        private LibVLCSharp.WinForms.VideoView videoRx1;
        private LibVLCSharp.WinForms.VideoView videoRx3;
        private LibVLCSharp.WinForms.VideoView videoRx2;
        private LibVLCSharp.WinForms.VideoView videoRx4;
        private System.Windows.Forms.PictureBox nolock_rx1;
        private System.Windows.Forms.PictureBox nolock_rx3;
        private System.Windows.Forms.PictureBox nolock_rx2;
        private System.Windows.Forms.PictureBox nolock_rx4;
        private datvreceiver.OutlinedLabel lab_rx1_nothing;
        private datvreceiver.OutlinedLabel lab_rx3_nothing;
        private datvreceiver.OutlinedLabel lab_rx2_nothing;
        private datvreceiver.OutlinedLabel lab_rx4_nothing;
    }
}

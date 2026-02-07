namespace datvreceiver
{
    partial class SettingsForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dbgListBox = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtRX4Offset = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.txtRX3Offset = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.txtRX2Offset = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.txtRX1Offset = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.txtWinterhillBasePort = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtWinterhillHost = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtLocalIP = new System.Windows.Forms.TextBox();
            this.checkForceLocalIP = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.trackBarVideoCaching = new System.Windows.Forms.TrackBar();
            this.labelVideoCaching = new System.Windows.Forms.Label();
            this.labelVideoCachingValue = new System.Windows.Forms.Label();
            this.checkAutoZoom = new System.Windows.Forms.CheckBox();
            this.checkHardwareDecoding = new System.Windows.Forms.CheckBox();
            this.checkCompatibilityMode = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.labLocalIP = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoCaching)).BeginInit();
            this.SuspendLayout();

            // splitContainer1
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";

            // splitContainer1.Panel1
            this.splitContainer1.Panel1.Controls.Add(this.dbgListBox);

            // splitContainer1.Panel2
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(800, 500);
            this.splitContainer1.SplitterDistance = 350;
            this.splitContainer1.TabIndex = 0;

            // dbgListBox
            this.dbgListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbgListBox.FormattingEnabled = true;
            this.dbgListBox.Location = new System.Drawing.Point(0, 0);
            this.dbgListBox.Name = "dbgListBox";
            this.dbgListBox.Size = new System.Drawing.Size(350, 500);
            this.dbgListBox.TabIndex = 0;

            // panel1
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.btnAbout);
            this.panel1.Controls.Add(this.label14);
            this.panel1.Controls.Add(this.labLocalIP);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(446, 500);
            this.panel1.TabIndex = 0;

            // groupBox1
            this.groupBox1.Controls.Add(this.txtRX4Offset);
            this.groupBox1.Controls.Add(this.label28);
            this.groupBox1.Controls.Add(this.txtRX3Offset);
            this.groupBox1.Controls.Add(this.label27);
            this.groupBox1.Controls.Add(this.txtRX2Offset);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.txtRX1Offset);
            this.groupBox1.Controls.Add(this.label25);
            this.groupBox1.Controls.Add(this.txtWinterhillBasePort);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.txtWinterhillHost);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Location = new System.Drawing.Point(15, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(398, 225);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Winterhill Settings ";

            // label15
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(20, 30);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(84, 13);
            this.label15.Text = "Winterhill Host :";

            // txtWinterhillHost
            this.txtWinterhillHost.Location = new System.Drawing.Point(134, 27);
            this.txtWinterhillHost.Name = "txtWinterhillHost";
            this.txtWinterhillHost.Size = new System.Drawing.Size(235, 20);
            this.txtWinterhillHost.TabIndex = 1;

            // label16
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(20, 56);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(63, 13);
            this.label16.Text = "Base Port :";

            // txtWinterhillBasePort
            this.txtWinterhillBasePort.Location = new System.Drawing.Point(134, 53);
            this.txtWinterhillBasePort.Name = "txtWinterhillBasePort";
            this.txtWinterhillBasePort.Size = new System.Drawing.Size(100, 20);
            this.txtWinterhillBasePort.TabIndex = 3;

            // label25
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(20, 95);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(64, 13);
            this.label25.Text = "RX1 Offset :";

            // txtRX1Offset
            this.txtRX1Offset.Location = new System.Drawing.Point(134, 92);
            this.txtRX1Offset.Name = "txtRX1Offset";
            this.txtRX1Offset.Size = new System.Drawing.Size(100, 20);
            this.txtRX1Offset.TabIndex = 5;

            // label26
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(20, 121);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(64, 13);
            this.label26.Text = "RX2 Offset :";

            // txtRX2Offset
            this.txtRX2Offset.Location = new System.Drawing.Point(134, 118);
            this.txtRX2Offset.Name = "txtRX2Offset";
            this.txtRX2Offset.Size = new System.Drawing.Size(100, 20);
            this.txtRX2Offset.TabIndex = 7;

            // label27
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(20, 147);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(64, 13);
            this.label27.Text = "RX3 Offset :";

            // txtRX3Offset
            this.txtRX3Offset.Location = new System.Drawing.Point(134, 144);
            this.txtRX3Offset.Name = "txtRX3Offset";
            this.txtRX3Offset.Size = new System.Drawing.Size(100, 20);
            this.txtRX3Offset.TabIndex = 9;

            // label28
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(20, 173);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(64, 13);
            this.label28.Text = "RX4 Offset :";

            // txtRX4Offset
            this.txtRX4Offset.Location = new System.Drawing.Point(134, 170);
            this.txtRX4Offset.Name = "txtRX4Offset";
            this.txtRX4Offset.Size = new System.Drawing.Size(100, 20);
            this.txtRX4Offset.TabIndex = 11;

            // groupBox3
            this.groupBox3.Controls.Add(this.txtLocalIP);
            this.groupBox3.Controls.Add(this.checkForceLocalIP);
            this.groupBox3.Location = new System.Drawing.Point(15, 250);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(398, 70);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = " Network Settings ";

            // checkForceLocalIP
            this.checkForceLocalIP.AutoSize = true;
            this.checkForceLocalIP.Location = new System.Drawing.Point(20, 30);
            this.checkForceLocalIP.Name = "checkForceLocalIP";
            this.checkForceLocalIP.Size = new System.Drawing.Size(113, 17);
            this.checkForceLocalIP.Text = "Force Local IP to :";

            // txtLocalIP
            this.txtLocalIP.Location = new System.Drawing.Point(140, 28);
            this.txtLocalIP.Name = "txtLocalIP";
            this.txtLocalIP.Size = new System.Drawing.Size(229, 20);
            this.txtLocalIP.TabIndex = 1;

            // groupBox4
            this.groupBox4.Controls.Add(this.trackBarVideoCaching);
            this.groupBox4.Controls.Add(this.labelVideoCaching);
            this.groupBox4.Controls.Add(this.labelVideoCachingValue);
            this.groupBox4.Controls.Add(this.checkAutoZoom);
            this.groupBox4.Controls.Add(this.checkHardwareDecoding);
            this.groupBox4.Controls.Add(this.checkCompatibilityMode);
            this.groupBox4.Location = new System.Drawing.Point(15, 330);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(398, 145);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = " Display Settings ";

            // checkAutoZoom
            this.checkAutoZoom.AutoSize = true;
            this.checkAutoZoom.Location = new System.Drawing.Point(20, 65);
            this.checkAutoZoom.Name = "checkAutoZoom";
            this.checkAutoZoom.Size = new System.Drawing.Size(280, 17);
            this.checkAutoZoom.Text = "Auto zoom when only one video stream is received";
            this.checkAutoZoom.CheckedChanged += new System.EventHandler(this.checkAutoZoom_CheckedChanged);

            // checkHardwareDecoding
            this.checkHardwareDecoding.AutoSize = true;
            this.checkHardwareDecoding.Location = new System.Drawing.Point(20, 90);
            this.checkHardwareDecoding.Name = "checkHardwareDecoding";
            this.checkHardwareDecoding.Size = new System.Drawing.Size(280, 17);
            this.checkHardwareDecoding.Text = "Hardware decoding (requires restart)";
            this.checkHardwareDecoding.CheckedChanged += new System.EventHandler(this.checkHardwareDecoding_CheckedChanged);

            // checkCompatibilityMode
            this.checkCompatibilityMode.AutoSize = true;
            this.checkCompatibilityMode.Location = new System.Drawing.Point(20, 115);
            this.checkCompatibilityMode.Name = "checkCompatibilityMode";
            this.checkCompatibilityMode.Size = new System.Drawing.Size(350, 17);
            this.checkCompatibilityMode.Text = "Compatibility mode for older hardware (requires restart)";
            this.checkCompatibilityMode.CheckedChanged += new System.EventHandler(this.checkCompatibilityMode_CheckedChanged);

            // labelVideoCaching
            this.labelVideoCaching.AutoSize = true;
            this.labelVideoCaching.Location = new System.Drawing.Point(20, 30);
            this.labelVideoCaching.Name = "labelVideoCaching";
            this.labelVideoCaching.Size = new System.Drawing.Size(110, 13);
            this.labelVideoCaching.Text = "Video Buffer (ms) :";

            // trackBarVideoCaching
            this.trackBarVideoCaching.Location = new System.Drawing.Point(140, 25);
            this.trackBarVideoCaching.Maximum = 500;
            this.trackBarVideoCaching.Minimum = 50;
            this.trackBarVideoCaching.Name = "trackBarVideoCaching";
            this.trackBarVideoCaching.Size = new System.Drawing.Size(180, 45);
            this.trackBarVideoCaching.TabIndex = 1;
            this.trackBarVideoCaching.TickFrequency = 50;
            this.trackBarVideoCaching.Value = 100;
            this.trackBarVideoCaching.Scroll += new System.EventHandler(this.trackBarVideoCaching_Scroll);

            // labelVideoCachingValue
            this.labelVideoCachingValue.AutoSize = true;
            this.labelVideoCachingValue.Location = new System.Drawing.Point(325, 30);
            this.labelVideoCachingValue.Name = "labelVideoCachingValue";
            this.labelVideoCachingValue.Size = new System.Drawing.Size(50, 13);
            this.labelVideoCachingValue.Text = "100 ms";

            // label14
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(15, 490);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(54, 13);
            this.label14.Text = "Local IP :";

            // labLocalIP
            this.labLocalIP.AutoSize = true;
            this.labLocalIP.Location = new System.Drawing.Point(75, 490);
            this.labLocalIP.Name = "labLocalIP";
            this.labLocalIP.Size = new System.Drawing.Size(10, 13);
            this.labLocalIP.Text = "-";

            // button1
            this.button1.Location = new System.Drawing.Point(15, 520);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 30);
            this.button1.TabIndex = 4;
            this.button1.Text = "Update Settings";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);

            // btnAbout
            this.btnAbout.Location = new System.Drawing.Point(145, 520);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(80, 30);
            this.btnAbout.TabIndex = 10;
            this.btnAbout.Text = "About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);

            // SettingsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "SettingsForm";
            this.Text = "Debug / Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoCaching)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox dbgListBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtRX4Offset;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox txtRX3Offset;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox txtRX2Offset;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox txtRX1Offset;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox txtWinterhillBasePort;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtWinterhillHost;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtLocalIP;
        private System.Windows.Forms.CheckBox checkForceLocalIP;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labLocalIP;
        private System.Windows.Forms.CheckBox checkAutoZoom;
        private System.Windows.Forms.CheckBox checkHardwareDecoding;
        private System.Windows.Forms.CheckBox checkCompatibilityMode;
        private System.Windows.Forms.TrackBar trackBarVideoCaching;
        private System.Windows.Forms.Label labelVideoCaching;
        private System.Windows.Forms.Label labelVideoCachingValue;
    }
}

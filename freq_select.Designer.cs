namespace datvreceiver
{
    partial class freq_select
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblCurRx = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCurOffset = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtNewFrequency = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboNewSR = new System.Windows.Forms.ComboBox();
            this.btnSet = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "RX : ";
            // 
            // lblCurRx
            // 
            this.lblCurRx.AutoSize = true;
            this.lblCurRx.Location = new System.Drawing.Point(143, 18);
            this.lblCurRx.Name = "lblCurRx";
            this.lblCurRx.Size = new System.Drawing.Size(10, 13);
            this.lblCurRx.TabIndex = 1;
            this.lblCurRx.Text = " ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Current Offset :";
            // 
            // lblCurOffset
            // 
            this.lblCurOffset.AutoSize = true;
            this.lblCurOffset.Location = new System.Drawing.Point(143, 43);
            this.lblCurOffset.Name = "lblCurOffset";
            this.lblCurOffset.Size = new System.Drawing.Size(10, 13);
            this.lblCurOffset.TabIndex = 3;
            this.lblCurOffset.Text = " ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "New Frequency :";
            // 
            // txtNewFrequency
            // 
            this.txtNewFrequency.Location = new System.Drawing.Point(146, 69);
            this.txtNewFrequency.Name = "txtNewFrequency";
            this.txtNewFrequency.Size = new System.Drawing.Size(121, 20);
            this.txtNewFrequency.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "New Symbol Rate :";
            // 
            // comboNewSR
            // 
            this.comboNewSR.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboNewSR.FormattingEnabled = true;
            this.comboNewSR.Items.AddRange(new object[] {
            "2000",
            "1500",
            "1000",
            "500",
            "333",
            "250",
            "125"});
            this.comboNewSR.Location = new System.Drawing.Point(146, 100);
            this.comboNewSR.Name = "comboNewSR";
            this.comboNewSR.Size = new System.Drawing.Size(121, 21);
            this.comboNewSR.TabIndex = 7;
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(192, 145);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(75, 23);
            this.btnSet.TabIndex = 8;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(111, 145);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // freq_select
            // 
            this.AcceptButton = this.btnSet;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(286, 191);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.comboNewSR);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtNewFrequency);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblCurOffset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblCurRx);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "freq_select";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Frequency Select";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lblCurRx;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label lblCurOffset;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox txtNewFrequency;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.ComboBox comboNewSR;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button btnCancel;
    }
}
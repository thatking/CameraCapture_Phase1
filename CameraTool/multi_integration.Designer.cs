namespace CameraTool
{
    partial class multi_integration
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ms_width = new System.Windows.Forms.TextBox();
            this.ms_period = new System.Windows.Forms.TextBox();
            this.Actual_Pulses_Under_FLO_HIGH = new System.Windows.Forms.Label();
            this.NumPulses_FLO = new System.Windows.Forms.TextBox();
            this.PulseCount = new System.Windows.Forms.Label();
            this.PulseWidth = new System.Windows.Forms.Label();
            this.NumPulse = new System.Windows.Forms.TextBox();
            this.TextWidth = new System.Windows.Forms.TextBox();
            this.txtPeriod = new System.Windows.Forms.Label();
            this.txtBoxPeriod = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TriggerMode_Selection = new System.Windows.Forms.ComboBox();
            this.GammaEN = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GammaRatio = new System.Windows.Forms.TrackBar();
            this.btnMultiCancel = new System.Windows.Forms.Button();
            this.btnMultiOK = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GammaRatio)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.ms_width);
            this.groupBox1.Controls.Add(this.ms_period);
            this.groupBox1.Controls.Add(this.Actual_Pulses_Under_FLO_HIGH);
            this.groupBox1.Controls.Add(this.NumPulses_FLO);
            this.groupBox1.Controls.Add(this.PulseCount);
            this.groupBox1.Controls.Add(this.PulseWidth);
            this.groupBox1.Controls.Add(this.NumPulse);
            this.groupBox1.Controls.Add(this.TextWidth);
            this.groupBox1.Controls.Add(this.txtPeriod);
            this.groupBox1.Controls.Add(this.txtBoxPeriod);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 137);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trigger from FPGA";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 28;
            this.label3.Text = "ms";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(252, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 27;
            this.label2.Text = "ms";
            // 
            // ms_width
            // 
            this.ms_width.Location = new System.Drawing.Point(173, 56);
            this.ms_width.Name = "ms_width";
            this.ms_width.ReadOnly = true;
            this.ms_width.Size = new System.Drawing.Size(73, 21);
            this.ms_width.TabIndex = 26;
            // 
            // ms_period
            // 
            this.ms_period.Location = new System.Drawing.Point(173, 18);
            this.ms_period.Name = "ms_period";
            this.ms_period.ReadOnly = true;
            this.ms_period.Size = new System.Drawing.Size(73, 21);
            this.ms_period.TabIndex = 25;
            // 
            // Actual_Pulses_Under_FLO_HIGH
            // 
            this.Actual_Pulses_Under_FLO_HIGH.AutoSize = true;
            this.Actual_Pulses_Under_FLO_HIGH.Location = new System.Drawing.Point(194, 83);
            this.Actual_Pulses_Under_FLO_HIGH.Name = "Actual_Pulses_Under_FLO_HIGH";
            this.Actual_Pulses_Under_FLO_HIGH.Size = new System.Drawing.Size(65, 12);
            this.Actual_Pulses_Under_FLO_HIGH.TabIndex = 24;
            this.Actual_Pulses_Under_FLO_HIGH.Text = "Pulses_FLO";
            // 
            // NumPulses_FLO
            // 
            this.NumPulses_FLO.Location = new System.Drawing.Point(196, 98);
            this.NumPulses_FLO.Name = "NumPulses_FLO";
            this.NumPulses_FLO.Size = new System.Drawing.Size(50, 21);
            this.NumPulses_FLO.TabIndex = 23;
            this.NumPulses_FLO.Text = "3";
            // 
            // PulseCount
            // 
            this.PulseCount.AutoSize = true;
            this.PulseCount.Location = new System.Drawing.Point(35, 107);
            this.PulseCount.Name = "PulseCount";
            this.PulseCount.Size = new System.Drawing.Size(53, 12);
            this.PulseCount.TabIndex = 22;
            this.PulseCount.Text = "NumPulse";
            // 
            // PulseWidth
            // 
            this.PulseWidth.AutoSize = true;
            this.PulseWidth.Location = new System.Drawing.Point(35, 65);
            this.PulseWidth.Name = "PulseWidth";
            this.PulseWidth.Size = new System.Drawing.Size(65, 12);
            this.PulseWidth.TabIndex = 21;
            this.PulseWidth.Text = "PulseWidth";
            // 
            // NumPulse
            // 
            this.NumPulse.Location = new System.Drawing.Point(106, 98);
            this.NumPulse.Name = "NumPulse";
            this.NumPulse.Size = new System.Drawing.Size(56, 21);
            this.NumPulse.TabIndex = 20;
            this.NumPulse.Text = "0x08";
            this.NumPulse.TextChanged += new System.EventHandler(this.NumPulse_TextChanged);
            // 
            // TextWidth
            // 
            this.TextWidth.Location = new System.Drawing.Point(106, 56);
            this.TextWidth.Name = "TextWidth";
            this.TextWidth.Size = new System.Drawing.Size(56, 21);
            this.TextWidth.TabIndex = 19;
            this.TextWidth.Text = "0x820";
            this.TextWidth.TextChanged += new System.EventHandler(this.TextWidth_TextChanged);
            // 
            // txtPeriod
            // 
            this.txtPeriod.AutoSize = true;
            this.txtPeriod.Location = new System.Drawing.Point(35, 27);
            this.txtPeriod.Name = "txtPeriod";
            this.txtPeriod.Size = new System.Drawing.Size(41, 12);
            this.txtPeriod.TabIndex = 18;
            this.txtPeriod.Text = "Period";
            // 
            // txtBoxPeriod
            // 
            this.txtBoxPeriod.Location = new System.Drawing.Point(106, 18);
            this.txtBoxPeriod.Name = "txtBoxPeriod";
            this.txtBoxPeriod.Size = new System.Drawing.Size(56, 21);
            this.txtBoxPeriod.TabIndex = 17;
            this.txtBoxPeriod.Text = "0x7000";
            this.txtBoxPeriod.TextChanged += new System.EventHandler(this.txtBoxPeriod_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 209);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 26;
            this.label4.Text = "TriggerMode:";
            // 
            // TriggerMode_Selection
            // 
            this.TriggerMode_Selection.FormattingEnabled = true;
            this.TriggerMode_Selection.Items.AddRange(new object[] {
            "NONE",
            "ExTrigger",
            "FPGA(Continuous pulses)"});
            this.TriggerMode_Selection.Location = new System.Drawing.Point(117, 206);
            this.TriggerMode_Selection.Name = "TriggerMode_Selection";
            this.TriggerMode_Selection.Size = new System.Drawing.Size(141, 20);
            this.TriggerMode_Selection.TabIndex = 25;
            this.TriggerMode_Selection.SelectedIndexChanged += new System.EventHandler(this.TriggerMode_Selection_SelectedIndexChanged);
            // 
            // GammaEN
            // 
            this.GammaEN.AutoSize = true;
            this.GammaEN.Checked = true;
            this.GammaEN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GammaEN.Location = new System.Drawing.Point(218, 155);
            this.GammaEN.Name = "GammaEN";
            this.GammaEN.Size = new System.Drawing.Size(66, 16);
            this.GammaEN.TabIndex = 24;
            this.GammaEN.Text = "GammaEN";
            this.GammaEN.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 23;
            this.label1.Text = "GammaRatio";
            // 
            // GammaRatio
            // 
            this.GammaRatio.Location = new System.Drawing.Point(108, 155);
            this.GammaRatio.Maximum = 20;
            this.GammaRatio.Minimum = 10;
            this.GammaRatio.Name = "GammaRatio";
            this.GammaRatio.Size = new System.Drawing.Size(104, 45);
            this.GammaRatio.TabIndex = 22;
            this.GammaRatio.Value = 16;
            // 
            // btnMultiCancel
            // 
            this.btnMultiCancel.Location = new System.Drawing.Point(157, 251);
            this.btnMultiCancel.Name = "btnMultiCancel";
            this.btnMultiCancel.Size = new System.Drawing.Size(75, 21);
            this.btnMultiCancel.TabIndex = 21;
            this.btnMultiCancel.Text = "Cancel";
            this.btnMultiCancel.UseVisualStyleBackColor = true;
            this.btnMultiCancel.Click += new System.EventHandler(this.btnMultiCancel_Click);
            // 
            // btnMultiOK
            // 
            this.btnMultiOK.Location = new System.Drawing.Point(36, 251);
            this.btnMultiOK.Name = "btnMultiOK";
            this.btnMultiOK.Size = new System.Drawing.Size(75, 21);
            this.btnMultiOK.TabIndex = 20;
            this.btnMultiOK.Text = "OK";
            this.btnMultiOK.UseVisualStyleBackColor = true;
            this.btnMultiOK.Click += new System.EventHandler(this.btnMultiOK_Click);
            // 
            // multi_integration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 293);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TriggerMode_Selection);
            this.Controls.Add(this.GammaEN);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GammaRatio);
            this.Controls.Add(this.btnMultiCancel);
            this.Controls.Add(this.btnMultiOK);
            this.Name = "multi_integration";
            this.Text = "multi_integration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GammaRatio)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ms_width;
        private System.Windows.Forms.TextBox ms_period;
        private System.Windows.Forms.Label Actual_Pulses_Under_FLO_HIGH;
        private System.Windows.Forms.TextBox NumPulses_FLO;
        private System.Windows.Forms.Label PulseCount;
        private System.Windows.Forms.Label PulseWidth;
        private System.Windows.Forms.TextBox NumPulse;
        private System.Windows.Forms.TextBox TextWidth;
        private System.Windows.Forms.Label txtPeriod;
        private System.Windows.Forms.TextBox txtBoxPeriod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox TriggerMode_Selection;
        private System.Windows.Forms.CheckBox GammaEN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar GammaRatio;
        private System.Windows.Forms.Button btnMultiCancel;
        private System.Windows.Forms.Button btnMultiOK;
    }
}
namespace CameraTool
{
    partial class LyftConfigForm
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
            this.GainBox = new System.Windows.Forms.GroupBox();
            this.Reset_Gain = new System.Windows.Forms.Button();
            this.BlueGainBox = new System.Windows.Forms.RichTextBox();
            this.GreenGainBox = new System.Windows.Forms.RichTextBox();
            this.RedGainBox = new System.Windows.Forms.RichTextBox();
            this.BlueGain = new System.Windows.Forms.HScrollBar();
            this.GreenGain = new System.Windows.Forms.HScrollBar();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.OffsetBox = new System.Windows.Forms.GroupBox();
            this.Reset_Offset = new System.Windows.Forms.Button();
            this.BlueOffsetBox = new System.Windows.Forms.RichTextBox();
            this.GreenOffsetBox = new System.Windows.Forms.RichTextBox();
            this.RedOffsetBox = new System.Windows.Forms.RichTextBox();
            this.BlueOffset = new System.Windows.Forms.HScrollBar();
            this.GreenOffset = new System.Windows.Forms.HScrollBar();
            this.RedOffset = new System.Windows.Forms.HScrollBar();
            this.RedGain = new System.Windows.Forms.HScrollBar();
            this.GainBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.OffsetBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // GainBox
            // 
            this.GainBox.Controls.Add(this.Reset_Gain);
            this.GainBox.Controls.Add(this.BlueGainBox);
            this.GainBox.Controls.Add(this.GreenGainBox);
            this.GainBox.Controls.Add(this.RedGainBox);
            this.GainBox.Controls.Add(this.BlueGain);
            this.GainBox.Controls.Add(this.GreenGain);
            this.GainBox.Controls.Add(this.RedGain);
            this.GainBox.Location = new System.Drawing.Point(0, 37);
            this.GainBox.Name = "GainBox";
            this.GainBox.Size = new System.Drawing.Size(696, 432);
            this.GainBox.TabIndex = 0;
            this.GainBox.TabStop = false;
            this.GainBox.Text = "RGB-Gain";
            this.GainBox.UseCompatibleTextRendering = true;
            // 
            // Reset_Gain
            // 
            this.Reset_Gain.Location = new System.Drawing.Point(242, 377);
            this.Reset_Gain.Name = "Reset_Gain";
            this.Reset_Gain.Size = new System.Drawing.Size(171, 49);
            this.Reset_Gain.TabIndex = 6;
            this.Reset_Gain.Text = "Reset";
            this.Reset_Gain.UseVisualStyleBackColor = true;
            this.Reset_Gain.Click += new System.EventHandler(this.Reset_Gain_Click);
            // 
            // BlueGainBox
            // 
            this.BlueGainBox.BackColor = System.Drawing.Color.Blue;
            this.BlueGainBox.Location = new System.Drawing.Point(6, 257);
            this.BlueGainBox.Name = "BlueGainBox";
            this.BlueGainBox.Size = new System.Drawing.Size(63, 49);
            this.BlueGainBox.TabIndex = 5;
            this.BlueGainBox.Text = "";
            // 
            // GreenGainBox
            // 
            this.GreenGainBox.BackColor = System.Drawing.Color.Lime;
            this.GreenGainBox.Location = new System.Drawing.Point(6, 156);
            this.GreenGainBox.Name = "GreenGainBox";
            this.GreenGainBox.Size = new System.Drawing.Size(63, 49);
            this.GreenGainBox.TabIndex = 4;
            this.GreenGainBox.Text = "";
            // 
            // RedGainBox
            // 
            this.RedGainBox.BackColor = System.Drawing.Color.Red;
            this.RedGainBox.Location = new System.Drawing.Point(6, 60);
            this.RedGainBox.Name = "RedGainBox";
            this.RedGainBox.Size = new System.Drawing.Size(63, 49);
            this.RedGainBox.TabIndex = 3;
            this.RedGainBox.Text = "";
            // 
            // BlueGain
            // 
            this.BlueGain.LargeChange = 25;
            this.BlueGain.Location = new System.Drawing.Point(72, 257);
            this.BlueGain.Maximum = 800;
            this.BlueGain.Name = "BlueGain";
            this.BlueGain.Size = new System.Drawing.Size(585, 49);
            this.BlueGain.TabIndex = 2;
            this.BlueGain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.BlueGain_Scroll);
            // 
            // GreenGain
            // 
            this.GreenGain.LargeChange = 25;
            this.GreenGain.Location = new System.Drawing.Point(72, 156);
            this.GreenGain.Maximum = 800;
            this.GreenGain.Name = "GreenGain";
            this.GreenGain.Size = new System.Drawing.Size(585, 49);
            this.GreenGain.TabIndex = 1;
            this.GreenGain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.GreenGain_Scroll);
            // 
            // OffsetBox
            // 
            this.OffsetBox.Controls.Add(this.Reset_Offset);
            this.OffsetBox.Controls.Add(this.BlueOffsetBox);
            this.OffsetBox.Controls.Add(this.GreenOffsetBox);
            this.OffsetBox.Controls.Add(this.RedOffsetBox);
            this.OffsetBox.Controls.Add(this.BlueOffset);
            this.OffsetBox.Controls.Add(this.GreenOffset);
            this.OffsetBox.Controls.Add(this.RedOffset);
            this.OffsetBox.Location = new System.Drawing.Point(744, 37);
            this.OffsetBox.Name = "OffsetBox";
            this.OffsetBox.Size = new System.Drawing.Size(696, 426);
            this.OffsetBox.TabIndex = 6;
            this.OffsetBox.TabStop = false;
            this.OffsetBox.Text = "RGB-Offset";
            this.OffsetBox.UseCompatibleTextRendering = true;
            // 
            // Reset_Offset
            // 
            this.Reset_Offset.Location = new System.Drawing.Point(259, 377);
            this.Reset_Offset.Name = "Reset_Offset";
            this.Reset_Offset.Size = new System.Drawing.Size(171, 49);
            this.Reset_Offset.TabIndex = 7;
            this.Reset_Offset.Text = "Reset";
            this.Reset_Offset.UseVisualStyleBackColor = true;
            this.Reset_Offset.Click += new System.EventHandler(this.Reset_Offset_Click);
            // 
            // BlueOffsetBox
            // 
            this.BlueOffsetBox.BackColor = System.Drawing.Color.Blue;
            this.BlueOffsetBox.Location = new System.Drawing.Point(6, 257);
            this.BlueOffsetBox.Name = "BlueOffsetBox";
            this.BlueOffsetBox.Size = new System.Drawing.Size(70, 49);
            this.BlueOffsetBox.TabIndex = 5;
            this.BlueOffsetBox.Text = "";
            // 
            // GreenOffsetBox
            // 
            this.GreenOffsetBox.BackColor = System.Drawing.Color.Lime;
            this.GreenOffsetBox.Location = new System.Drawing.Point(6, 156);
            this.GreenOffsetBox.Name = "GreenOffsetBox";
            this.GreenOffsetBox.Size = new System.Drawing.Size(70, 49);
            this.GreenOffsetBox.TabIndex = 4;
            this.GreenOffsetBox.Text = "";
            // 
            // RedOffsetBox
            // 
            this.RedOffsetBox.BackColor = System.Drawing.Color.Red;
            this.RedOffsetBox.Location = new System.Drawing.Point(6, 60);
            this.RedOffsetBox.Name = "RedOffsetBox";
            this.RedOffsetBox.Size = new System.Drawing.Size(70, 49);
            this.RedOffsetBox.TabIndex = 3;
            this.RedOffsetBox.Text = "";
            // 
            // BlueOffset
            // 
            this.BlueOffset.LargeChange = 25;
            this.BlueOffset.Location = new System.Drawing.Point(72, 257);
            this.BlueOffset.Maximum = 800;
            this.BlueOffset.Name = "BlueOffset";
            this.BlueOffset.Size = new System.Drawing.Size(585, 49);
            this.BlueOffset.TabIndex = 2;
            this.BlueOffset.Scroll += new System.Windows.Forms.ScrollEventHandler(this.BlueOffset_Scroll);
            // 
            // GreenOffset
            // 
            this.GreenOffset.LargeChange = 25;
            this.GreenOffset.Location = new System.Drawing.Point(72, 156);
            this.GreenOffset.Maximum = 800;
            this.GreenOffset.Name = "GreenOffset";
            this.GreenOffset.Size = new System.Drawing.Size(585, 49);
            this.GreenOffset.TabIndex = 1;
            this.GreenOffset.Scroll += new System.Windows.Forms.ScrollEventHandler(this.GreenOffset_Scroll);
            // 
            // RedOffset
            // 
            this.RedOffset.LargeChange = 25;
            this.RedOffset.Location = new System.Drawing.Point(72, 60);
            this.RedOffset.Maximum = 800;
            this.RedOffset.Name = "RedOffset";
            this.RedOffset.Size = new System.Drawing.Size(585, 49);
            this.RedOffset.TabIndex = 0;
            this.RedOffset.Scroll += new System.Windows.Forms.ScrollEventHandler(this.RedOffset_Scroll);
            // 
            // RedGain
            // 
            this.RedGain.BackColor = global::CameraTool.Properties.Settings.Default.color;
            this.RedGain.LargeChange = 25;
            this.RedGain.Location = new System.Drawing.Point(72, 60);
            this.RedGain.Maximum = 800;
            this.RedGain.Name = "RedGain";
            this.RedGain.Size = new System.Drawing.Size(585, 49);
            this.RedGain.TabIndex = 0;
            this.RedGain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.RedGain_Scroll);
            // 
            // LyftConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1474, 1429);
            this.Controls.Add(this.OffsetBox);
            this.Controls.Add(this.GainBox);
            this.Name = "LyftConfigForm";
            this.Text = "LyftConfig";
            this.GainBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.OffsetBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GainBox;
        private System.Windows.Forms.BindingSource bindingSource1;
        public System.Windows.Forms.HScrollBar RedGain;
        public System.Windows.Forms.HScrollBar GreenGain;
        public System.Windows.Forms.HScrollBar BlueGain;
        private System.Windows.Forms.RichTextBox BlueGainBox;
        private System.Windows.Forms.RichTextBox GreenGainBox;
        private System.Windows.Forms.RichTextBox RedGainBox;
        private System.Windows.Forms.GroupBox OffsetBox;
        private System.Windows.Forms.RichTextBox BlueOffsetBox;
        private System.Windows.Forms.RichTextBox GreenOffsetBox;
        private System.Windows.Forms.RichTextBox RedOffsetBox;
        public System.Windows.Forms.HScrollBar BlueOffset;
        public System.Windows.Forms.HScrollBar GreenOffset;
        public System.Windows.Forms.HScrollBar RedOffset;
        public System.Windows.Forms.Button Reset_Gain;
        public System.Windows.Forms.Button Reset_Offset;
    }
}
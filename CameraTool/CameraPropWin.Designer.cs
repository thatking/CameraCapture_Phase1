namespace CameraTool
{
    partial class CameraPropWin
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
            this.labelCamGain = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.trackBarGain = new System.Windows.Forms.TrackBar();
            this.textBoxGain = new System.Windows.Forms.TextBox();
            this.textBoxExposure = new System.Windows.Forms.TextBox();
            this.trackBarExposure = new System.Windows.Forms.TrackBar();
            this.labelCamExposure = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxAE = new System.Windows.Forms.CheckBox();
            this.buttonSet = new System.Windows.Forms.Button();
            this.txtBoxExpTime = new System.Windows.Forms.TextBox();
            this.labelExpTime = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).BeginInit();
            this.SuspendLayout();
            // 
            // labelCamGain
            // 
            this.labelCamGain.AutoSize = true;
            this.labelCamGain.Location = new System.Drawing.Point(30, 133);
            this.labelCamGain.Name = "labelCamGain";
            this.labelCamGain.Size = new System.Drawing.Size(38, 17);
            this.labelCamGain.TabIndex = 0;
            this.labelCamGain.Text = "Gain";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(303, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(76, 36);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "CLOSE";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // trackBarGain
            // 
            this.trackBarGain.Location = new System.Drawing.Point(74, 133);
            this.trackBarGain.Name = "trackBarGain";
            this.trackBarGain.Size = new System.Drawing.Size(177, 56);
            this.trackBarGain.TabIndex = 2;
            // 
            // textBoxGain
            // 
            this.textBoxGain.Location = new System.Drawing.Point(257, 133);
            this.textBoxGain.Name = "textBoxGain";
            this.textBoxGain.Size = new System.Drawing.Size(52, 22);
            this.textBoxGain.TabIndex = 3;
            this.textBoxGain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxGain_KeyDown);
            // 
            // textBoxExposure
            // 
            this.textBoxExposure.Location = new System.Drawing.Point(257, 80);
            this.textBoxExposure.Name = "textBoxExposure";
            this.textBoxExposure.Size = new System.Drawing.Size(52, 22);
            this.textBoxExposure.TabIndex = 6;
            this.textBoxExposure.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExposure_KeyDown);
            // 
            // trackBarExposure
            // 
            this.trackBarExposure.Location = new System.Drawing.Point(74, 80);
            this.trackBarExposure.Name = "trackBarExposure";
            this.trackBarExposure.Size = new System.Drawing.Size(177, 56);
            this.trackBarExposure.TabIndex = 5;
            // 
            // labelCamExposure
            // 
            this.labelCamExposure.AutoSize = true;
            this.labelCamExposure.Location = new System.Drawing.Point(5, 80);
            this.labelCamExposure.Name = "labelCamExposure";
            this.labelCamExposure.Size = new System.Drawing.Size(67, 17);
            this.labelCamExposure.TabIndex = 4;
            this.labelCamExposure.Text = "Exposure";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(331, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Auto";
            // 
            // checkBoxAE
            // 
            this.checkBoxAE.AutoSize = true;
            this.checkBoxAE.Location = new System.Drawing.Point(334, 84);
            this.checkBoxAE.Name = "checkBoxAE";
            this.checkBoxAE.Size = new System.Drawing.Size(18, 17);
            this.checkBoxAE.TabIndex = 8;
            this.checkBoxAE.UseVisualStyleBackColor = true;
            this.checkBoxAE.CheckedChanged += new System.EventHandler(this.checkBoxAE_CheckedChanged);
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(266, 191);
            this.buttonSet.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(68, 32);
            this.buttonSet.TabIndex = 27;
            this.buttonSet.Text = "SET";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
            // 
            // txtBoxExpTime
            // 
            this.txtBoxExpTime.Location = new System.Drawing.Point(184, 196);
            this.txtBoxExpTime.Margin = new System.Windows.Forms.Padding(4);
            this.txtBoxExpTime.Name = "txtBoxExpTime";
            this.txtBoxExpTime.Size = new System.Drawing.Size(67, 22);
            this.txtBoxExpTime.TabIndex = 26;
            this.txtBoxExpTime.Text = "6";
            // 
            // labelExpTime
            // 
            this.labelExpTime.AutoSize = true;
            this.labelExpTime.Location = new System.Drawing.Point(22, 199);
            this.labelExpTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelExpTime.Name = "labelExpTime";
            this.labelExpTime.Size = new System.Drawing.Size(154, 17);
            this.labelExpTime.TabIndex = 25;
            this.labelExpTime.Text = "Exposure Time (Lines):";
            // 
            // CameraPropWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 237);
            this.ControlBox = false;
            this.Controls.Add(this.buttonSet);
            this.Controls.Add(this.txtBoxExpTime);
            this.Controls.Add(this.labelExpTime);
            this.Controls.Add(this.checkBoxAE);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxExposure);
            this.Controls.Add(this.trackBarExposure);
            this.Controls.Add(this.labelCamExposure);
            this.Controls.Add(this.textBoxGain);
            this.Controls.Add(this.trackBarGain);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelCamGain);
            this.Name = "CameraPropWin";
            this.Text = "CameraPropWin";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCamGain;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TrackBar trackBarGain;
        private System.Windows.Forms.TextBox textBoxGain;
        private System.Windows.Forms.TextBox textBoxExposure;
        private System.Windows.Forms.TrackBar trackBarExposure;
        private System.Windows.Forms.Label labelCamExposure;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxAE;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.TextBox txtBoxExpTime;
        private System.Windows.Forms.Label labelExpTime;
    }
}
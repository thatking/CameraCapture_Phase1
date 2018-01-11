namespace CameraTool
{
    partial class RegRW_ModeSET
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
            this.S_Mode = new System.Windows.Forms.Label();
            this.SensorMode = new System.Windows.Forms.ComboBox();
            this.Value = new System.Windows.Forms.Label();
            this.Address = new System.Windows.Forms.Label();
            this.REG_Value = new System.Windows.Forms.TextBox();
            this.REG_Address = new System.Windows.Forms.TextBox();
            this.Read = new System.Windows.Forms.Button();
            this.Write = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ComboBox_ROI_Level = new System.Windows.Forms.ComboBox();
            this.txtBoxStartX = new System.Windows.Forms.TextBox();
            this.btnROI_ParameterOK = new System.Windows.Forms.Button();
            this.txtBoxStartY = new System.Windows.Forms.TextBox();
            this.StartYtrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.StartXtrackBar = new System.Windows.Forms.TrackBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPLL = new System.Windows.Forms.Button();
            this.btnExpH = new System.Windows.Forms.Button();
            this.btnExpL = new System.Windows.Forms.Button();
            this.btnOffsetH = new System.Windows.Forms.Button();
            this.btnOffsetL = new System.Windows.Forms.Button();
            this.btnGain = new System.Windows.Forms.Button();
            this.btnBOffsetH = new System.Windows.Forms.Button();
            this.btnBOffsetL = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnADCGain = new System.Windows.Forms.Button();
            this.btnExpMode = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnFPGATriggerMode = new System.Windows.Forms.Button();
            this.btnTriggerDelay = new System.Windows.Forms.Button();
            this.btnFPGAExposureTime = new System.Windows.Forms.Button();
            this.btnFPGAFramePeriod = new System.Windows.Forms.Button();
            this.btnHexDec = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnIRSwitch = new System.Windows.Forms.Button();
            this.SerSet = new System.Windows.Forms.Button();
            this.SerGet = new System.Windows.Forms.Button();
            this.SerNum = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StartYtrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartXtrackBar)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // S_Mode
            // 
            this.S_Mode.AutoSize = true;
            this.S_Mode.Location = new System.Drawing.Point(34, 514);
            this.S_Mode.Name = "S_Mode";
            this.S_Mode.Size = new System.Drawing.Size(71, 12);
            this.S_Mode.TabIndex = 19;
            this.S_Mode.Text = "SensorMode:";
            // 
            // SensorMode
            // 
            this.SensorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SensorMode.FormattingEnabled = true;
            this.SensorMode.Items.AddRange(new object[] {
            "Global Shutter",
            "Rolling Shutter",
            "Global Reset"});
            this.SensorMode.Location = new System.Drawing.Point(112, 506);
            this.SensorMode.Name = "SensorMode";
            this.SensorMode.Size = new System.Drawing.Size(138, 20);
            this.SensorMode.TabIndex = 18;
            this.SensorMode.SelectedIndexChanged += new System.EventHandler(this.SensorMode_SelectedIndexChanged);
            // 
            // Value
            // 
            this.Value.AutoSize = true;
            this.Value.Location = new System.Drawing.Point(209, 46);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(35, 12);
            this.Value.TabIndex = 17;
            this.Value.Text = "Value";
            // 
            // Address
            // 
            this.Address.AutoSize = true;
            this.Address.Location = new System.Drawing.Point(124, 46);
            this.Address.Name = "Address";
            this.Address.Size = new System.Drawing.Size(47, 12);
            this.Address.TabIndex = 16;
            this.Address.Text = "Address";
            // 
            // REG_Value
            // 
            this.REG_Value.Location = new System.Drawing.Point(195, 74);
            this.REG_Value.Name = "REG_Value";
            this.REG_Value.Size = new System.Drawing.Size(64, 21);
            this.REG_Value.TabIndex = 15;
            this.REG_Value.Text = "0x0000";
            // 
            // REG_Address
            // 
            this.REG_Address.Location = new System.Drawing.Point(112, 74);
            this.REG_Address.Name = "REG_Address";
            this.REG_Address.Size = new System.Drawing.Size(68, 21);
            this.REG_Address.TabIndex = 14;
            this.REG_Address.Text = "0x0000";
            // 
            // Read
            // 
            this.Read.Location = new System.Drawing.Point(33, 85);
            this.Read.Name = "Read";
            this.Read.Size = new System.Drawing.Size(67, 20);
            this.Read.TabIndex = 11;
            this.Read.Text = "Read";
            this.Read.UseVisualStyleBackColor = true;
            this.Read.Click += new System.EventHandler(this.Read_Click_1);
            // 
            // Write
            // 
            this.Write.Location = new System.Drawing.Point(33, 58);
            this.Write.Name = "Write";
            this.Write.Size = new System.Drawing.Size(67, 20);
            this.Write.TabIndex = 10;
            this.Write.Text = "Write";
            this.Write.UseVisualStyleBackColor = true;
            this.Write.Click += new System.EventHandler(this.Write_Click_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.ComboBox_ROI_Level);
            this.groupBox1.Controls.Add(this.txtBoxStartX);
            this.groupBox1.Controls.Add(this.btnROI_ParameterOK);
            this.groupBox1.Controls.Add(this.txtBoxStartY);
            this.groupBox1.Controls.Add(this.StartYtrackBar);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.StartXtrackBar);
            this.groupBox1.Location = new System.Drawing.Point(32, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(233, 119);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ROI Window";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 23;
            this.label3.Text = "RoiLevel";
            // 
            // ComboBox_ROI_Level
            // 
            this.ComboBox_ROI_Level.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox_ROI_Level.FormattingEnabled = true;
            this.ComboBox_ROI_Level.Items.AddRange(new object[] {
            "1:1",
            "1:2",
            "1:3"});
            this.ComboBox_ROI_Level.Location = new System.Drawing.Point(60, 92);
            this.ComboBox_ROI_Level.Name = "ComboBox_ROI_Level";
            this.ComboBox_ROI_Level.Size = new System.Drawing.Size(77, 20);
            this.ComboBox_ROI_Level.TabIndex = 22;
            this.ComboBox_ROI_Level.SelectedIndexChanged += new System.EventHandler(this.ComboBox_ROI_Level_SelectedIndexChanged);
            // 
            // txtBoxStartX
            // 
            this.txtBoxStartX.Location = new System.Drawing.Point(160, 20);
            this.txtBoxStartX.Name = "txtBoxStartX";
            this.txtBoxStartX.Size = new System.Drawing.Size(51, 21);
            this.txtBoxStartX.TabIndex = 4;
            this.txtBoxStartX.Text = "12";
            // 
            // btnROI_ParameterOK
            // 
            this.btnROI_ParameterOK.Location = new System.Drawing.Point(160, 88);
            this.btnROI_ParameterOK.Name = "btnROI_ParameterOK";
            this.btnROI_ParameterOK.Size = new System.Drawing.Size(51, 24);
            this.btnROI_ParameterOK.TabIndex = 6;
            this.btnROI_ParameterOK.Text = "OK";
            this.btnROI_ParameterOK.UseVisualStyleBackColor = true;
            this.btnROI_ParameterOK.Click += new System.EventHandler(this.btnROI_ParameterOK_Click);
            // 
            // txtBoxStartY
            // 
            this.txtBoxStartY.Location = new System.Drawing.Point(160, 52);
            this.txtBoxStartY.Name = "txtBoxStartY";
            this.txtBoxStartY.Size = new System.Drawing.Size(51, 21);
            this.txtBoxStartY.TabIndex = 5;
            this.txtBoxStartY.Text = "10";
            // 
            // StartYtrackBar
            // 
            this.StartYtrackBar.LargeChange = 6;
            this.StartYtrackBar.Location = new System.Drawing.Point(50, 52);
            this.StartYtrackBar.Maximum = 896;
            this.StartYtrackBar.Minimum = 6;
            this.StartYtrackBar.Name = "StartYtrackBar";
            this.StartYtrackBar.Size = new System.Drawing.Size(104, 45);
            this.StartYtrackBar.SmallChange = 2;
            this.StartYtrackBar.TabIndex = 3;
            this.StartYtrackBar.Value = 6;
            this.StartYtrackBar.Scroll += new System.EventHandler(this.StartYtrackBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(6, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "StartY:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "StartX:";
            // 
            // StartXtrackBar
            // 
            this.StartXtrackBar.LargeChange = 6;
            this.StartXtrackBar.Location = new System.Drawing.Point(50, 20);
            this.StartXtrackBar.Maximum = 1152;
            this.StartXtrackBar.Minimum = 6;
            this.StartXtrackBar.Name = "StartXtrackBar";
            this.StartXtrackBar.Size = new System.Drawing.Size(104, 45);
            this.StartXtrackBar.SmallChange = 2;
            this.StartXtrackBar.TabIndex = 0;
            this.StartXtrackBar.Value = 6;
            this.StartXtrackBar.Scroll += new System.EventHandler(this.StartXtrackBar_Scroll);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(212, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(51, 24);
            this.btnClose.TabIndex = 25;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPLL
            // 
            this.btnPLL.Location = new System.Drawing.Point(4, 16);
            this.btnPLL.Name = "btnPLL";
            this.btnPLL.Size = new System.Drawing.Size(51, 24);
            this.btnPLL.TabIndex = 26;
            this.btnPLL.Text = "PLL";
            this.btnPLL.UseVisualStyleBackColor = true;
            this.btnPLL.Click += new System.EventHandler(this.btnPLL_Click);
            // 
            // btnExpH
            // 
            this.btnExpH.Location = new System.Drawing.Point(63, 16);
            this.btnExpH.Name = "btnExpH";
            this.btnExpH.Size = new System.Drawing.Size(51, 24);
            this.btnExpH.TabIndex = 27;
            this.btnExpH.Text = "ExpH";
            this.btnExpH.UseVisualStyleBackColor = true;
            this.btnExpH.Click += new System.EventHandler(this.btnExpH_Click);
            // 
            // btnExpL
            // 
            this.btnExpL.Location = new System.Drawing.Point(63, 50);
            this.btnExpL.Name = "btnExpL";
            this.btnExpL.Size = new System.Drawing.Size(51, 24);
            this.btnExpL.TabIndex = 28;
            this.btnExpL.Text = "ExpL";
            this.btnExpL.UseVisualStyleBackColor = true;
            this.btnExpL.Click += new System.EventHandler(this.btnExpL_Click);
            // 
            // btnOffsetH
            // 
            this.btnOffsetH.Location = new System.Drawing.Point(178, 16);
            this.btnOffsetH.Name = "btnOffsetH";
            this.btnOffsetH.Size = new System.Drawing.Size(51, 24);
            this.btnOffsetH.TabIndex = 29;
            this.btnOffsetH.Text = "TOffH";
            this.btnOffsetH.UseVisualStyleBackColor = true;
            this.btnOffsetH.Click += new System.EventHandler(this.btnOffsetH_Click);
            // 
            // btnOffsetL
            // 
            this.btnOffsetL.Location = new System.Drawing.Point(178, 50);
            this.btnOffsetL.Name = "btnOffsetL";
            this.btnOffsetL.Size = new System.Drawing.Size(51, 24);
            this.btnOffsetL.TabIndex = 30;
            this.btnOffsetL.Text = "TOffL";
            this.btnOffsetL.UseVisualStyleBackColor = true;
            this.btnOffsetL.Click += new System.EventHandler(this.btnOffsetL_Click);
            // 
            // btnGain
            // 
            this.btnGain.Location = new System.Drawing.Point(4, 50);
            this.btnGain.Name = "btnGain";
            this.btnGain.Size = new System.Drawing.Size(51, 24);
            this.btnGain.TabIndex = 31;
            this.btnGain.Text = "Gain";
            this.btnGain.UseVisualStyleBackColor = true;
            this.btnGain.Click += new System.EventHandler(this.btnGain_Click);
            // 
            // btnBOffsetH
            // 
            this.btnBOffsetH.Location = new System.Drawing.Point(122, 16);
            this.btnBOffsetH.Name = "btnBOffsetH";
            this.btnBOffsetH.Size = new System.Drawing.Size(51, 24);
            this.btnBOffsetH.TabIndex = 32;
            this.btnBOffsetH.Text = "BOffH";
            this.btnBOffsetH.UseVisualStyleBackColor = true;
            this.btnBOffsetH.Click += new System.EventHandler(this.btnBOffsetH_Click);
            // 
            // btnBOffsetL
            // 
            this.btnBOffsetL.Location = new System.Drawing.Point(122, 50);
            this.btnBOffsetL.Name = "btnBOffsetL";
            this.btnBOffsetL.Size = new System.Drawing.Size(51, 24);
            this.btnBOffsetL.TabIndex = 33;
            this.btnBOffsetL.Text = "BOffL";
            this.btnBOffsetL.UseVisualStyleBackColor = true;
            this.btnBOffsetL.Click += new System.EventHandler(this.btnBOffsetL_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnADCGain);
            this.groupBox2.Controls.Add(this.btnExpMode);
            this.groupBox2.Controls.Add(this.btnPLL);
            this.groupBox2.Controls.Add(this.btnBOffsetL);
            this.groupBox2.Controls.Add(this.btnGain);
            this.groupBox2.Controls.Add(this.btnBOffsetH);
            this.groupBox2.Controls.Add(this.btnExpH);
            this.groupBox2.Controls.Add(this.btnOffsetL);
            this.groupBox2.Controls.Add(this.btnExpL);
            this.groupBox2.Controls.Add(this.btnOffsetH);
            this.groupBox2.Location = new System.Drawing.Point(32, 287);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(233, 114);
            this.groupBox2.TabIndex = 34;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CMV300 Registers";
            // 
            // btnADCGain
            // 
            this.btnADCGain.Location = new System.Drawing.Point(4, 80);
            this.btnADCGain.Name = "btnADCGain";
            this.btnADCGain.Size = new System.Drawing.Size(51, 24);
            this.btnADCGain.TabIndex = 35;
            this.btnADCGain.Text = "AdcG";
            this.btnADCGain.UseVisualStyleBackColor = true;
            this.btnADCGain.Click += new System.EventHandler(this.btnADCGain_Click);
            // 
            // btnExpMode
            // 
            this.btnExpMode.Location = new System.Drawing.Point(63, 80);
            this.btnExpMode.Name = "btnExpMode";
            this.btnExpMode.Size = new System.Drawing.Size(51, 24);
            this.btnExpMode.TabIndex = 34;
            this.btnExpMode.Text = "EMode";
            this.btnExpMode.UseVisualStyleBackColor = true;
            this.btnExpMode.Click += new System.EventHandler(this.btnExpMode_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnFPGATriggerMode);
            this.groupBox3.Controls.Add(this.btnTriggerDelay);
            this.groupBox3.Controls.Add(this.btnFPGAExposureTime);
            this.groupBox3.Controls.Add(this.btnFPGAFramePeriod);
            this.groupBox3.Location = new System.Drawing.Point(32, 410);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(233, 52);
            this.groupBox3.TabIndex = 35;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "FPGA Registers";
            // 
            // btnFPGATriggerMode
            // 
            this.btnFPGATriggerMode.Location = new System.Drawing.Point(4, 16);
            this.btnFPGATriggerMode.Name = "btnFPGATriggerMode";
            this.btnFPGATriggerMode.Size = new System.Drawing.Size(51, 24);
            this.btnFPGATriggerMode.TabIndex = 26;
            this.btnFPGATriggerMode.Text = "TMode";
            this.btnFPGATriggerMode.UseVisualStyleBackColor = true;
            this.btnFPGATriggerMode.Click += new System.EventHandler(this.btnFPGATriggerMode_Click);
            // 
            // btnTriggerDelay
            // 
            this.btnTriggerDelay.Location = new System.Drawing.Point(122, 16);
            this.btnTriggerDelay.Name = "btnTriggerDelay";
            this.btnTriggerDelay.Size = new System.Drawing.Size(51, 24);
            this.btnTriggerDelay.TabIndex = 32;
            this.btnTriggerDelay.Text = "TrigD";
            this.btnTriggerDelay.UseVisualStyleBackColor = true;
            this.btnTriggerDelay.Click += new System.EventHandler(this.btnTriggerDelay_Click);
            // 
            // btnFPGAExposureTime
            // 
            this.btnFPGAExposureTime.Location = new System.Drawing.Point(63, 16);
            this.btnFPGAExposureTime.Name = "btnFPGAExposureTime";
            this.btnFPGAExposureTime.Size = new System.Drawing.Size(51, 24);
            this.btnFPGAExposureTime.TabIndex = 27;
            this.btnFPGAExposureTime.Text = "ExpT";
            this.btnFPGAExposureTime.UseVisualStyleBackColor = true;
            this.btnFPGAExposureTime.Click += new System.EventHandler(this.btnFPGAExposureTime_Click);
            // 
            // btnFPGAFramePeriod
            // 
            this.btnFPGAFramePeriod.Location = new System.Drawing.Point(178, 16);
            this.btnFPGAFramePeriod.Name = "btnFPGAFramePeriod";
            this.btnFPGAFramePeriod.Size = new System.Drawing.Size(51, 24);
            this.btnFPGAFramePeriod.TabIndex = 29;
            this.btnFPGAFramePeriod.Text = "FrmP";
            this.btnFPGAFramePeriod.UseVisualStyleBackColor = true;
            this.btnFPGAFramePeriod.Click += new System.EventHandler(this.btnFPGAFramePeriod_Click);
            // 
            // btnHexDec
            // 
            this.btnHexDec.Location = new System.Drawing.Point(33, 10);
            this.btnHexDec.Name = "btnHexDec";
            this.btnHexDec.Size = new System.Drawing.Size(51, 24);
            this.btnHexDec.TabIndex = 36;
            this.btnHexDec.Text = "HEX";
            this.btnHexDec.UseVisualStyleBackColor = true;
            this.btnHexDec.Click += new System.EventHandler(this.btnHexDec_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 473);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 37;
            this.label4.Text = "IR Swich:";
            // 
            // btnIRSwitch
            // 
            this.btnIRSwitch.Location = new System.Drawing.Point(92, 467);
            this.btnIRSwitch.Name = "btnIRSwitch";
            this.btnIRSwitch.Size = new System.Drawing.Size(51, 24);
            this.btnIRSwitch.TabIndex = 38;
            this.btnIRSwitch.Text = "On";
            this.btnIRSwitch.UseVisualStyleBackColor = true;
            this.btnIRSwitch.Click += new System.EventHandler(this.btnIRSwitch_Click);
            // 
            // SerSet
            // 
            this.SerSet.Location = new System.Drawing.Point(9, 121);
            this.SerSet.Name = "SerSet";
            this.SerSet.Size = new System.Drawing.Size(75, 23);
            this.SerSet.TabIndex = 39;
            this.SerSet.Text = "Ser#_Set";
            this.SerSet.UseVisualStyleBackColor = true;
            this.SerSet.Click += new System.EventHandler(this.SerSet_Click);
            // 
            // SerGet
            // 
            this.SerGet.Location = new System.Drawing.Point(228, 121);
            this.SerGet.Name = "SerGet";
            this.SerGet.Size = new System.Drawing.Size(75, 23);
            this.SerGet.TabIndex = 40;
            this.SerGet.Text = "Ser#_Get";
            this.SerGet.UseVisualStyleBackColor = true;
            this.SerGet.Click += new System.EventHandler(this.SerGet_Click);
            // 
            // SerNum
            // 
            this.SerNum.Location = new System.Drawing.Point(95, 123);
            this.SerNum.Name = "SerNum";
            this.SerNum.Size = new System.Drawing.Size(116, 21);
            this.SerNum.TabIndex = 41;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 42;
            this.label5.Text = "BoardSerial#:";
            // 
            // RegRW_ModeSET
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 580);
            this.ControlBox = false;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SerNum);
            this.Controls.Add(this.SerGet);
            this.Controls.Add(this.SerSet);
            this.Controls.Add(this.btnIRSwitch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnHexDec);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.S_Mode);
            this.Controls.Add(this.SensorMode);
            this.Controls.Add(this.Value);
            this.Controls.Add(this.Address);
            this.Controls.Add(this.REG_Value);
            this.Controls.Add(this.REG_Address);
            this.Controls.Add(this.Read);
            this.Controls.Add(this.Write);
            this.Name = "RegRW_ModeSET";
            this.Text = "REGRW_MODESET";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StartYtrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartXtrackBar)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label S_Mode;
        private System.Windows.Forms.ComboBox SensorMode;
        private System.Windows.Forms.Label Value;
        private System.Windows.Forms.Label Address;
        private System.Windows.Forms.TextBox REG_Value;
        private System.Windows.Forms.TextBox REG_Address;
        private System.Windows.Forms.Button Read;
        private System.Windows.Forms.Button Write;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TrackBar StartYtrackBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar StartXtrackBar;
        private System.Windows.Forms.TextBox txtBoxStartY;
        private System.Windows.Forms.TextBox txtBoxStartX;
        private System.Windows.Forms.Button btnROI_ParameterOK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ComboBox_ROI_Level;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPLL;
        private System.Windows.Forms.Button btnExpH;
        private System.Windows.Forms.Button btnExpL;
        private System.Windows.Forms.Button btnOffsetH;
        private System.Windows.Forms.Button btnOffsetL;
        private System.Windows.Forms.Button btnGain;
        private System.Windows.Forms.Button btnBOffsetH;
        private System.Windows.Forms.Button btnBOffsetL;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnExpMode;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnFPGATriggerMode;
        private System.Windows.Forms.Button btnTriggerDelay;
        private System.Windows.Forms.Button btnFPGAExposureTime;
        private System.Windows.Forms.Button btnFPGAFramePeriod;
        private System.Windows.Forms.Button btnADCGain;
        private System.Windows.Forms.Button btnHexDec;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnIRSwitch;
        private System.Windows.Forms.Button SerSet;
        private System.Windows.Forms.Button SerGet;
        private System.Windows.Forms.TextBox SerNum;
        private System.Windows.Forms.Label label5;
    }
}
namespace DefaultAudioDeviceSwitcher
{
    partial class ConfigForm
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
            this.cancelBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.communicationCheck = new System.Windows.Forms.CheckBox();
            this.nirCmdEdit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.browseNirCmdBtn = new System.Windows.Forms.Button();
            this.headsetCombo = new System.Windows.Forms.ComboBox();
            this.speakerCombo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(210, 140);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 0;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(129, 140);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 1;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Headset";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Speaker";
            // 
            // communicationCheck
            // 
            this.communicationCheck.AutoSize = true;
            this.communicationCheck.Location = new System.Drawing.Point(12, 70);
            this.communicationCheck.Name = "communicationCheck";
            this.communicationCheck.Size = new System.Drawing.Size(254, 19);
            this.communicationCheck.TabIndex = 6;
            this.communicationCheck.Text = "also change default communication device";
            this.communicationCheck.UseVisualStyleBackColor = true;
            // 
            // nirCmdEdit
            // 
            this.nirCmdEdit.Location = new System.Drawing.Point(81, 111);
            this.nirCmdEdit.Name = "nirCmdEdit";
            this.nirCmdEdit.Size = new System.Drawing.Size(174, 23);
            this.nirCmdEdit.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "NirCMD";
            // 
            // browseNirCmdBtn
            // 
            this.browseNirCmdBtn.Image = global::DefaultAudioDeviceSwitcher.Properties.Resources.Explorer;
            this.browseNirCmdBtn.Location = new System.Drawing.Point(261, 111);
            this.browseNirCmdBtn.Name = "browseNirCmdBtn";
            this.browseNirCmdBtn.Size = new System.Drawing.Size(24, 23);
            this.browseNirCmdBtn.TabIndex = 9;
            this.browseNirCmdBtn.UseVisualStyleBackColor = true;
            this.browseNirCmdBtn.Click += new System.EventHandler(this.browseNirCmdBtn_Click);
            // 
            // headsetCombo
            // 
            this.headsetCombo.FormattingEnabled = true;
            this.headsetCombo.Location = new System.Drawing.Point(81, 12);
            this.headsetCombo.Name = "headsetCombo";
            this.headsetCombo.Size = new System.Drawing.Size(204, 23);
            this.headsetCombo.TabIndex = 10;
            // 
            // speakerCombo
            // 
            this.speakerCombo.FormattingEnabled = true;
            this.speakerCombo.Location = new System.Drawing.Point(81, 41);
            this.speakerCombo.Name = "speakerCombo";
            this.speakerCombo.Size = new System.Drawing.Size(204, 23);
            this.speakerCombo.TabIndex = 11;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 175);
            this.Controls.Add(this.speakerCombo);
            this.Controls.Add(this.headsetCombo);
            this.Controls.Add(this.browseNirCmdBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nirCmdEdit);
            this.Controls.Add(this.communicationCheck);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.cancelBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConfigForm";
            this.Text = "Configuration";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button cancelBtn;
        private Button saveBtn;
        private Label label1;
        private Label label2;
        private CheckBox communicationCheck;
        private TextBox nirCmdEdit;
        private Label label3;
        private Button browseNirCmdBtn;
        private ComboBox headsetCombo;
        private ComboBox speakerCombo;
    }
}
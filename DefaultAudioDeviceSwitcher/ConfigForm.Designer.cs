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
            cancelBtn = new Button();
            saveBtn = new Button();
            headsetLabel = new Label();
            speakerLabel = new Label();
            communicationCheck = new CheckBox();
            headsetCombo = new ComboBox();
            speakerCombo = new ComboBox();
            SuspendLayout();
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelBtn.Location = new Point(296, 100);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(75, 23);
            cancelBtn.TabIndex = 0;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = true;
            cancelBtn.Click += cancelBtn_Click;
            // 
            // saveBtn
            // 
            saveBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveBtn.Location = new Point(215, 100);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new Size(75, 23);
            saveBtn.TabIndex = 1;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += saveBtn_Click;
            // 
            // headsetLabel
            // 
            headsetLabel.AutoSize = true;
            headsetLabel.Location = new Point(12, 15);
            headsetLabel.Name = "headsetLabel";
            headsetLabel.Size = new Size(50, 15);
            headsetLabel.TabIndex = 2;
            headsetLabel.Text = "Headset";
            // 
            // speakerLabel
            // 
            speakerLabel.AutoSize = true;
            speakerLabel.Location = new Point(12, 44);
            speakerLabel.Name = "speakerLabel";
            speakerLabel.Size = new Size(48, 15);
            speakerLabel.TabIndex = 4;
            speakerLabel.Text = "Speaker";
            // 
            // communicationCheck
            // 
            communicationCheck.AutoSize = true;
            communicationCheck.Location = new Point(12, 70);
            communicationCheck.Name = "communicationCheck";
            communicationCheck.Size = new Size(254, 19);
            communicationCheck.TabIndex = 6;
            communicationCheck.Text = "also change default communication device";
            communicationCheck.UseVisualStyleBackColor = true;
            // 
            // headsetCombo
            // 
            headsetCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            headsetCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            headsetCombo.FormattingEnabled = true;
            headsetCombo.Location = new Point(81, 12);
            headsetCombo.Name = "headsetCombo";
            headsetCombo.Size = new Size(290, 23);
            headsetCombo.TabIndex = 10;
            // 
            // speakerCombo
            // 
            speakerCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            speakerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            speakerCombo.FormattingEnabled = true;
            speakerCombo.Location = new Point(81, 41);
            speakerCombo.Name = "speakerCombo";
            speakerCombo.Size = new Size(290, 23);
            speakerCombo.TabIndex = 11;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(383, 135);
            Controls.Add(speakerCombo);
            Controls.Add(headsetCombo);
            Controls.Add(communicationCheck);
            Controls.Add(speakerLabel);
            Controls.Add(headsetLabel);
            Controls.Add(saveBtn);
            Controls.Add(cancelBtn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Configuration";
            Load += ConfigForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button cancelBtn;
        private Button saveBtn;
        private Label headsetLabel;
        private Label speakerLabel;
        private CheckBox communicationCheck;
        private ComboBox headsetCombo;
        private ComboBox speakerCombo;
    }
}
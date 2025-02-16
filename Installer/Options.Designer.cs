namespace Installer
{
    partial class Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
            this.label1 = new System.Windows.Forms.Label();
            this.directory = new System.Windows.Forms.TextBox();
            this.selectDirectory = new System.Windows.Forms.Button();
            this.consolePlayer = new System.Windows.Forms.CheckBox();
            this.converter = new System.Windows.Forms.CheckBox();
            this.next = new System.Windows.Forms.Button();
            this.spaceReq = new System.Windows.Forms.Label();
            this.fileAssoc = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Installation Directory";
            // 
            // directory
            // 
            this.directory.Location = new System.Drawing.Point(16, 32);
            this.directory.Name = "directory";
            this.directory.Size = new System.Drawing.Size(388, 20);
            this.directory.TabIndex = 1;
            // 
            // selectDirectory
            // 
            this.selectDirectory.Location = new System.Drawing.Point(410, 31);
            this.selectDirectory.Name = "selectDirectory";
            this.selectDirectory.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.selectDirectory.Size = new System.Drawing.Size(26, 21);
            this.selectDirectory.TabIndex = 2;
            this.selectDirectory.Text = "..";
            this.selectDirectory.UseVisualStyleBackColor = true;
            this.selectDirectory.Click += new System.EventHandler(this.selectDirectory_Click);
            // 
            // consolePlayer
            // 
            this.consolePlayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.consolePlayer.AutoSize = true;
            this.consolePlayer.Location = new System.Drawing.Point(16, 107);
            this.consolePlayer.Name = "consolePlayer";
            this.consolePlayer.Size = new System.Drawing.Size(96, 17);
            this.consolePlayer.TabIndex = 3;
            this.consolePlayer.Text = "Console Player";
            this.consolePlayer.UseVisualStyleBackColor = true;
            this.consolePlayer.CheckedChanged += new System.EventHandler(this.consolePlayer_CheckedChanged);
            // 
            // converter
            // 
            this.converter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.converter.AutoSize = true;
            this.converter.Location = new System.Drawing.Point(16, 154);
            this.converter.Name = "converter";
            this.converter.Size = new System.Drawing.Size(72, 17);
            this.converter.TabIndex = 4;
            this.converter.Text = "Converter";
            this.converter.UseVisualStyleBackColor = true;
            this.converter.CheckedChanged += new System.EventHandler(this.converter_CheckedChanged);
            // 
            // next
            // 
            this.next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.next.Location = new System.Drawing.Point(361, 260);
            this.next.Name = "next";
            this.next.Size = new System.Drawing.Size(75, 23);
            this.next.TabIndex = 5;
            this.next.Text = "Next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.next_Click);
            // 
            // spaceReq
            // 
            this.spaceReq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spaceReq.AutoSize = true;
            this.spaceReq.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.spaceReq.Location = new System.Drawing.Point(13, 260);
            this.spaceReq.Name = "spaceReq";
            this.spaceReq.Size = new System.Drawing.Size(36, 18);
            this.spaceReq.TabIndex = 6;
            this.spaceReq.Text = "Text";
            // 
            // fileAssoc
            // 
            this.fileAssoc.AutoSize = true;
            this.fileAssoc.Location = new System.Drawing.Point(16, 131);
            this.fileAssoc.Name = "fileAssoc";
            this.fileAssoc.Size = new System.Drawing.Size(117, 17);
            this.fileAssoc.TabIndex = 7;
            this.fileAssoc.Text = "Add file association";
            this.fileAssoc.UseVisualStyleBackColor = true;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 295);
            this.Controls.Add(this.fileAssoc);
            this.Controls.Add(this.spaceReq);
            this.Controls.Add(this.next);
            this.Controls.Add(this.converter);
            this.Controls.Add(this.consolePlayer);
            this.Controls.Add(this.selectDirectory);
            this.Controls.Add(this.directory);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Options";
            this.Text = "CCVC Installer";
            this.Load += new System.EventHandler(this.Options_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox directory;
        private System.Windows.Forms.Button selectDirectory;
        private System.Windows.Forms.CheckBox consolePlayer;
        private System.Windows.Forms.CheckBox converter;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Label spaceReq;
        private System.Windows.Forms.CheckBox fileAssoc;
    }
}
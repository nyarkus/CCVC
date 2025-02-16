namespace Installer
{
    partial class Installation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Installation));
            this.taskProgress = new System.Windows.Forms.ProgressBar();
            this.state = new System.Windows.Forms.Label();
            this.totalProgress = new System.Windows.Forms.ProgressBar();
            this.abort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // taskProgress
            // 
            this.taskProgress.Location = new System.Drawing.Point(12, 52);
            this.taskProgress.Name = "taskProgress";
            this.taskProgress.Size = new System.Drawing.Size(423, 23);
            this.taskProgress.TabIndex = 0;
            // 
            // state
            // 
            this.state.AutoSize = true;
            this.state.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.state.Location = new System.Drawing.Point(8, 29);
            this.state.Name = "state";
            this.state.Size = new System.Drawing.Size(113, 20);
            this.state.TabIndex = 1;
            this.state.Text = "Downloading...";
            // 
            // totalProgress
            // 
            this.totalProgress.Location = new System.Drawing.Point(12, 81);
            this.totalProgress.Name = "totalProgress";
            this.totalProgress.Size = new System.Drawing.Size(423, 23);
            this.totalProgress.TabIndex = 2;
            // 
            // abort
            // 
            this.abort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.abort.Location = new System.Drawing.Point(359, 260);
            this.abort.Name = "abort";
            this.abort.Size = new System.Drawing.Size(75, 23);
            this.abort.TabIndex = 3;
            this.abort.Text = "Abort";
            this.abort.UseVisualStyleBackColor = true;
            this.abort.Click += new System.EventHandler(this.abort_Click);
            // 
            // Installation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 295);
            this.Controls.Add(this.abort);
            this.Controls.Add(this.totalProgress);
            this.Controls.Add(this.state);
            this.Controls.Add(this.taskProgress);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Installation";
            this.Text = "CCVC Installer";
            this.Load += new System.EventHandler(this.Installation_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar taskProgress;
        private System.Windows.Forms.Label state;
        private System.Windows.Forms.ProgressBar totalProgress;
        private System.Windows.Forms.Button abort;
    }
}
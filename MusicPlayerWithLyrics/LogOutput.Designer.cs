namespace MusicPlayerWithLyrics
{
    partial class LogOutput
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
            this.logbox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // logbox
            // 
            this.logbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logbox.Location = new System.Drawing.Point(0, 0);
            this.logbox.Name = "logbox";
            this.logbox.Size = new System.Drawing.Size(697, 390);
            this.logbox.TabIndex = 0;
            this.logbox.Text = "日志输出面板\n>已开启";
            // 
            // LogOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 390);
            this.Controls.Add(this.logbox);
            this.Name = "LogOutput";
            this.Text = "LogOutput";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox logbox;
    }
}
namespace SomeAwesomeStub
{
    partial class StubTest
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StubTest));
            this.BtnStubRun = new System.Windows.Forms.Button();
            this.LblTitleInfo = new System.Windows.Forms.Label();
            this.InfoNumBytes = new System.Windows.Forms.Label();
            this.InfoMsg = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnStubRun
            // 
            this.BtnStubRun.Location = new System.Drawing.Point(164, 398);
            this.BtnStubRun.Name = "BtnStubRun";
            this.BtnStubRun.Size = new System.Drawing.Size(463, 40);
            this.BtnStubRun.TabIndex = 0;
            this.BtnStubRun.Text = "Click Me To Run The Stub";
            this.BtnStubRun.UseVisualStyleBackColor = true;
            this.BtnStubRun.Click += new System.EventHandler(this.BtnStubRun_Click);
            // 
            // LblTitleInfo
            // 
            this.LblTitleInfo.AutoSize = true;
            this.LblTitleInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTitleInfo.Location = new System.Drawing.Point(158, 9);
            this.LblTitleInfo.Name = "LblTitleInfo";
            this.LblTitleInfo.Size = new System.Drawing.Size(217, 31);
            this.LblTitleInfo.TabIndex = 1;
            this.LblTitleInfo.Text = "INFORMATION";
            // 
            // InfoNumBytes
            // 
            this.InfoNumBytes.AutoSize = true;
            this.InfoNumBytes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoNumBytes.Location = new System.Drawing.Point(160, 51);
            this.InfoNumBytes.Name = "InfoNumBytes";
            this.InfoNumBytes.Size = new System.Drawing.Size(203, 20);
            this.InfoNumBytes.TabIndex = 2;
            this.InfoNumBytes.Text = "Number of bytes in this exe:";
            // 
            // InfoMsg
            // 
            this.InfoMsg.AutoSize = true;
            this.InfoMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InfoMsg.ForeColor = System.Drawing.Color.Red;
            this.InfoMsg.Location = new System.Drawing.Point(160, 71);
            this.InfoMsg.Name = "InfoMsg";
            this.InfoMsg.Size = new System.Drawing.Size(24, 20);
            this.InfoMsg.TabIndex = 3;
            this.InfoMsg.Text = "---";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(50, 280);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(718, 100);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // StubTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InfoMsg);
            this.Controls.Add(this.InfoNumBytes);
            this.Controls.Add(this.LblTitleInfo);
            this.Controls.Add(this.BtnStubRun);
            this.Name = "StubTest";
            this.Text = "Program Stub";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnStubRun;
        private System.Windows.Forms.Label LblTitleInfo;
        private System.Windows.Forms.Label InfoNumBytes;
        private System.Windows.Forms.Label InfoMsg;
        private System.Windows.Forms.Label label1;
    }
}


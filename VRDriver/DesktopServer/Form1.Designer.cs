namespace DesktopServer
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timerUDPSend = new System.Windows.Forms.Timer(this.components);
            this.timerPing = new System.Windows.Forms.Timer(this.components);
            this.timerDetectLocalIP = new System.Windows.Forms.Timer(this.components);
            this.labelControllerLeft = new System.Windows.Forms.Label();
            this.progressBarControllerLeft = new System.Windows.Forms.ProgressBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.DimGray;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(681, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 35);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DimGray;
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(681, 92);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(89, 35);
            this.button2.TabIndex = 1;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Black;
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(12, 103);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(213, 201);
            this.textBox1.TabIndex = 2;
            // 
            // timerUDPSend
            // 
            this.timerUDPSend.Enabled = true;
            this.timerUDPSend.Interval = 500;
            this.timerUDPSend.Tick += new System.EventHandler(this.timerDetect_Tick);
            // 
            // timerPing
            // 
            this.timerPing.Enabled = true;
            this.timerPing.Interval = 1000;
            this.timerPing.Tick += new System.EventHandler(this.timerPing_Tick);
            // 
            // timerDetectLocalIP
            // 
            this.timerDetectLocalIP.Enabled = true;
            this.timerDetectLocalIP.Interval = 10000;
            this.timerDetectLocalIP.Tick += new System.EventHandler(this.timerDetectLocalIP_Tick);
            // 
            // labelControllerLeft
            // 
            this.labelControllerLeft.AutoSize = true;
            this.labelControllerLeft.BackColor = System.Drawing.Color.Black;
            this.labelControllerLeft.ForeColor = System.Drawing.Color.White;
            this.labelControllerLeft.Location = new System.Drawing.Point(33, 16);
            this.labelControllerLeft.Name = "labelControllerLeft";
            this.labelControllerLeft.Size = new System.Drawing.Size(68, 13);
            this.labelControllerLeft.TabIndex = 3;
            this.labelControllerLeft.Text = "controllerLeft";
            // 
            // progressBarControllerLeft
            // 
            this.progressBarControllerLeft.BackColor = System.Drawing.Color.Black;
            this.progressBarControllerLeft.ForeColor = System.Drawing.Color.RoyalBlue;
            this.progressBarControllerLeft.Location = new System.Drawing.Point(108, 16);
            this.progressBarControllerLeft.Maximum = 40;
            this.progressBarControllerLeft.Name = "progressBarControllerLeft";
            this.progressBarControllerLeft.Size = new System.Drawing.Size(103, 13);
            this.progressBarControllerLeft.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarControllerLeft.TabIndex = 4;
            this.progressBarControllerLeft.Value = 20;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pictureBox1.Location = new System.Drawing.Point(246, 16);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(400, 416);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.progressBarControllerLeft);
            this.Controls.Add(this.labelControllerLeft);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timerUDPSend;
        private System.Windows.Forms.Timer timerPing;
        private System.Windows.Forms.Timer timerDetectLocalIP;
        private System.Windows.Forms.Label labelControllerLeft;
        private System.Windows.Forms.ProgressBar progressBarControllerLeft;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}


﻿using System.Windows.Forms;

namespace ModbusBridge{

    partial class MainForm{
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
        private void InitializeComponent(){
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._connectionPanel = new System.Windows.Forms.Panel();
            this._exitButton = new System.Windows.Forms.Button();
            this._restartButton = new System.Windows.Forms.Button();
            this._link = new System.Windows.Forms.LinkLabel();
            this._newButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _connectionPanel
            // 
            this._connectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._connectionPanel.Location = new System.Drawing.Point(7, 82);
            this._connectionPanel.Name = "_connectionPanel";
            this._connectionPanel.Size = new System.Drawing.Size(586, 483);
            this._connectionPanel.TabIndex = 6;

            this._connectionPanel.HorizontalScroll.Maximum = 0;
            this._connectionPanel.AutoScroll = false;
            this._connectionPanel.VerticalScroll.Visible = false;
            this._connectionPanel.AutoScroll = true;

            // 
            // _exitButton
            // 
            this._exitButton.Location = new System.Drawing.Point(115, 571);
            this._exitButton.Name = "_exitButton";
            this._exitButton.Size = new System.Drawing.Size(75, 23);
            this._exitButton.TabIndex = 8;
            this._exitButton.Text = "Çıkış";
            this._exitButton.UseVisualStyleBackColor = true;
            // 
            // _restartButton
            // 
            this._restartButton.Location = new System.Drawing.Point(7, 571);
            this._restartButton.Name = "_restartButton";
            this._restartButton.Size = new System.Drawing.Size(102, 23);
            this._restartButton.TabIndex = 9;
            this._restartButton.Text = "Yeniden başlat";
            this._restartButton.UseVisualStyleBackColor = true;
            // 
            // _link
            // 
            this._link.AutoSize = true;
            this._link.BackColor = System.Drawing.Color.Transparent;
            this._link.Location = new System.Drawing.Point(490, 575);
            this._link.Name = "_link";
            this._link.Size = new System.Drawing.Size(107, 13);
            this._link.TabIndex = 10;
            this._link.TabStop = true;
            this._link.Text = "www.truekontrol.com";
            // 
            // button1
            // 
            this._newButton.Location = new System.Drawing.Point(520, 56);
            this._newButton.Name = "_newButton";
            this._newButton.Size = new System.Drawing.Size(75, 23);
            this._newButton.TabIndex = 11;
            this._newButton.Text = "Yeni";
            this._newButton.UseVisualStyleBackColor = true;
            //
            //Notify icon
            //
            this._notifyIcon = new NotifyIcon();
            this._notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info; //Shows the info icon so the user doesn't think there is an error.
            this._notifyIcon.BalloonTipText = "True Kontrol";
            this._notifyIcon.BalloonTipTitle = "Modbus Bridge";
            this._notifyIcon.Icon = Properties.Resources.true_logo_yJ1_icon;
            this._notifyIcon.Text = "Modbus Bridge";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Icon = Properties.Resources.true_logo_yJ1_icon;
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.Controls.Add(this._newButton);
            this.Controls.Add(this._link);
            this.Controls.Add(this._restartButton);
            this.Controls.Add(this._exitButton);
            this.Controls.Add(this._connectionPanel);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Modbus Bridge";
            this.ResumeLayout(false);
            this.PerformLayout();


        }

        private Panel _connectionPanel;
        private Button _exitButton;
        private Button _restartButton;
        private LinkLabel _link;
        private Button _newButton;
        private NotifyIcon _notifyIcon;

        public Panel ConnectionPanel {
            get { return _connectionPanel; }
        }

        public Button ExitButton {
            get { return _exitButton; }
        }

        public Button RestartButton {
            get { return _restartButton; }
        }

        #endregion

    }
}


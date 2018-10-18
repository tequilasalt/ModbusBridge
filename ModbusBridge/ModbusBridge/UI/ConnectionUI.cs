﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusBridge.UI{

    public class ConnectionUI:Panel {

        private Label _tcpLabel;
        private Label _serialLabel;

        private Button _editButton;
        private Button _debugButton;

        private PictureBox _tcpIcon;
        private PictureBox _serialIcon;
        
        private ConnectionModel _model;

        private Form _debugForm;
        private Form _settingsForm;

        public ConnectionUI(ConnectionModel model) {

            this.BorderStyle = BorderStyle.None;
            this.Name = "Connection";
            this.Size = new System.Drawing.Size(578, 24);
            this.BackColor = Color.White;

            var tcpHeader = new Label();
            tcpHeader.Size = new Size(60, 20);
            tcpHeader.Location = new Point(5, 2);
            tcpHeader.Font = new Font("Arial", tcpHeader.Font.Size, FontStyle.Bold);
            tcpHeader.ForeColor = Color.Black;
            tcpHeader.BackColor = Color.White;
            tcpHeader.TextAlign = ContentAlignment.MiddleCenter;
            tcpHeader.Text = "TCP Port:";

            this.Controls.Add(tcpHeader);

            _tcpLabel = new Label();
            _tcpLabel.Size = new Size(60, 20);
            _tcpLabel.Location = new Point(65, 2);
            _tcpLabel.Font = new Font("Arial", _tcpLabel.Font.Size, FontStyle.Bold);
            _tcpLabel.BackColor = Color.AliceBlue;
            _tcpLabel.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(_tcpLabel);

            var tcpStatusHeader = new Label();
            tcpStatusHeader.Size = new Size(50, 20);
            tcpStatusHeader.Location = new Point(130, 2);
            tcpStatusHeader.Font = new Font("Arial", tcpHeader.Font.Size, FontStyle.Bold);
            tcpStatusHeader.ForeColor = Color.Black;
            tcpStatusHeader.BackColor = Color.White;
            tcpStatusHeader.TextAlign = ContentAlignment.MiddleCenter;
            tcpStatusHeader.Text = "Durum:";

            this.Controls.Add(tcpStatusHeader);

            _tcpIcon = new PictureBox();
            _tcpIcon.Size = new Size(16, 16);
            _tcpIcon.Location = new Point(180, 4);

            this.Controls.Add(_tcpIcon);
            
            var direction = new PictureBox();
            direction.Size = new Size(24, 24);
            direction.Location = new Point(215, 0);
            direction.Image = Properties.Resources.direction;

            this.Controls.Add(direction);

            var serialHeader = new Label();
            serialHeader.Size = new Size(70, 20);
            serialHeader.Location = new Point(250, 2);
            serialHeader.Font = new Font("Arial", tcpHeader.Font.Size, FontStyle.Bold);
            serialHeader.ForeColor = Color.Black;
            serialHeader.BackColor = Color.White;
            serialHeader.TextAlign = ContentAlignment.MiddleCenter;
            serialHeader.Text = "COM Port:";

            this.Controls.Add(serialHeader);

            _serialLabel = new Label();
            _serialLabel.Size = new Size(60, 20);
            _serialLabel.Location = new Point(320, 2);
            _serialLabel.Font = new Font("Arial", _serialLabel.Font.Size, FontStyle.Bold);
            _serialLabel.BackColor = Color.AliceBlue;
            _serialLabel.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(_serialLabel);

            var serialStatusHeader = new Label();
            serialStatusHeader.Size = new Size(50, 20);
            serialStatusHeader.Location = new Point(385, 2);
            serialStatusHeader.Font = new Font("Arial", tcpHeader.Font.Size, FontStyle.Bold);
            serialStatusHeader.ForeColor = Color.Black;
            serialStatusHeader.BackColor = Color.White;
            serialStatusHeader.TextAlign = ContentAlignment.MiddleCenter;
            serialStatusHeader.Text = "Durum:";

            this.Controls.Add(serialStatusHeader);

            
            _serialIcon = new PictureBox();
            _serialIcon.Size = new Size(16, 16);
            _serialIcon.Location = new Point(435, 4);

            this.Controls.Add(_serialIcon);

            var seperator = new PictureBox();
            seperator.Size = new Size(3, 24);
            seperator.Location = new Point(490, 0);
            seperator.Image = Properties.Resources.seperator;

            this.Controls.Add(seperator);

            
            _debugButton = new Button();
            _debugButton.Text = "";
            _debugButton.Size = new Size(24,24);
            _debugButton.BackgroundImage = Properties.Resources.console;
            _debugButton.Location = new Point(500,0);

            this.Controls.Add(_debugButton);

            _editButton = new Button();
            _editButton.Text = "";
            _editButton.Size = new Size(24, 24);
            _editButton.BackgroundImage = Properties.Resources.settings;
            _editButton.Location = new Point(527, 0);

            _editButton.MouseClick += (sender, args) => {

                if (_settingsForm != null) {
                    return;
                }

                _settingsForm = new SettingsForm(_model, this);
                _settingsForm.Show();
            };

            this.Controls.Add(_editButton);
            
            Rebuild(model);
            
        }

        public void Rebuild(ConnectionModel model) {

            _model = model;

            _tcpLabel.Text = _model.TcpPort.ToString();

            _tcpIcon.Image = Properties.Resources.connection_off;
            
            _serialLabel.Text = _model.SerialPort;
            
            _serialIcon.Image = Properties.Resources.connection_on;

        }

    }

}

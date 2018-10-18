using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModbusBridge.UI{

    public class SettingsForm:Form {

        public SettingsForm(ConnectionModel model, ConnectionUI parent) {

            this.Text = "Ayarlar";
            this.Size = new Size(215,210);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Icon = Properties.Resources.true_logo_yJ1_icon;

            AddLabel("TCP Port", 10);
            AddLabel("COM Port", 35);
            AddLabel("Baudrate", 60);
            AddLabel("Parity", 85);
            AddLabel("Stop Bits", 110);

            NumericUpDown tcpPort = new NumericUpDown();

            tcpPort.Maximum = 65535;
            tcpPort.Minimum = 1;
            tcpPort.Increment = 1m;
            
            tcpPort.Text = model.TcpPort.ToString();
            tcpPort.Size = new Size(100,25);
            tcpPort.Location = new Point(80,13);

            this.Controls.Add(tcpPort);

            TextBox serialPort = new TextBox();
            serialPort.Text = model.SerialPort;
            serialPort.Size = new Size(100, 25);
            serialPort.Location = new Point(80, 38);

            this.Controls.Add(serialPort);

            ComboBox baudrate = new ComboBox();

            baudrate.Items.Add(9600);
            baudrate.Items.Add(14400);
            baudrate.Items.Add(19200);
            baudrate.Items.Add(38400);
            baudrate.Items.Add(57600);
            baudrate.Items.Add(115200);

            baudrate.Size = new Size(100, 25);
            baudrate.Location = new Point(80, 63);

            this.Controls.Add(baudrate);
            
            baudrate.SelectedIndex = baudrate.Items.IndexOf(model.BaudRate);

            ComboBox parity = new ComboBox();

            parity.Items.Add(Parity.None);
            parity.Items.Add(Parity.Odd);
            parity.Items.Add(Parity.Even);
            parity.Items.Add(Parity.Mark);
            parity.Items.Add(Parity.Space);

            parity.Size = new Size(100, 25);
            parity.Location = new Point(80, 88);

            this.Controls.Add(parity);

            parity.SelectedItem = model.Parity;

            ComboBox stopBits = new ComboBox();

            stopBits.Items.Add(StopBits.None);
            stopBits.Items.Add(StopBits.One);
            stopBits.Items.Add(StopBits.OnePointFive);
            stopBits.Items.Add(StopBits.Two);

            stopBits.Size = new Size(100, 25);
            stopBits.Location = new Point(80, 113);

            this.Controls.Add(stopBits);

            stopBits.SelectedItem = model.StopBits;

            Button saveButton = new Button();
            saveButton.Text = "Kaydet";
            saveButton.Size = new Size(80,25);
            saveButton.Location = new Point(20,145);

            this.Controls.Add(saveButton);

            saveButton.MouseClick += (sender, args) => {

                model.TcpPort = Convert.ToInt32(tcpPort.Value);
                model.SerialPort = serialPort.Text;
                model.BaudRate = (int) baudrate.SelectedItem;
                model.Parity = (Parity)parity.SelectedItem;
                model.StopBits = (StopBits) stopBits.SelectedItem;

                parent.Update(model);

            };

            Button deleteButton = new Button();
            deleteButton.Text = "Sil";
            deleteButton.Size = new Size(80, 25);
            deleteButton.Location = new Point(107, 145);

            this.Controls.Add(deleteButton);

            deleteButton.MouseClick += (sender, args) => {

                parent.Delete();

            };

        }

        private void AddLabel(string name, int y) {

            var label = new Label();
            label.Text = name+" :";
            label.Size = new Size(80, 25);
            label.Location = new Point(0, y);
            label.TextAlign = ContentAlignment.MiddleRight;

            this.Controls.Add(label);
        }
    }

}

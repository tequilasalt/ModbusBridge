using System.Drawing;
using System.Windows.Forms;

namespace ModbusBridge.UI {

    public class DebugForm:Form {

        private TextBox _logBox;

        public DebugForm(ConnectionModel model) {

            this.Text = "TCP "+model.TcpPort +" => "+model.SerialPort;
            this.ClientSize = new Size(400, 600);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Icon = Properties.Resources.true_logo_yJ1_icon;

            _logBox = new TextBox();
            _logBox.Size = new Size(400, 600);
            _logBox.Location = new Point(0, 0);
            _logBox.Multiline = true;
            _logBox.ScrollBars = ScrollBars.Vertical;
            _logBox.ReadOnly = true;

            this.Controls.Add(_logBox);

        }

        public TextBox LogBox => _logBox;

    }

}

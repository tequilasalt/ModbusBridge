using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ModbusBridge.UI;
using SimpleJSON;
using Timer = System.Timers.Timer;

namespace ModbusBridge{

    public partial class MainForm : Form {

        private static MainForm _instance;

        private Panel _loadingPanel;
        private List<ConnectionUI> _connections;

        private bool _ready;

        public MainForm() {

            _instance = this;

            InitializeComponent();
            
            _connections = new List<ConnectionUI>();

            Init();

            _notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            this.Resize += ImportStatusForm_Resize;
        }

        private void Init() {

            if (System.IO.File.Exists(@"Setup.json")) {

                string text = System.IO.File.ReadAllText(@"Setup.json");

                JSONNode node = JSON.Parse(text);

                Timer t = new Timer(1000);

                t.Elapsed += (sender, args) => {

                    t.Stop();

                    if (MainForm.Instance.InvokeRequired) {
                        MainForm.Instance.Invoke(new Action<JSONNode, int>(RecursiveInit), new object[] {node["Devices"], 0});
                        return;
                    }

                    RecursiveInit(node["Devices"], 0);

                };

                t.Start();

            } else {

                _restartButton.MouseClick += Restart;
                _exitButton.MouseClick += Exit;
                _newButton.MouseClick += CreateEmptyConnection;
                _link.MouseClick += ToWebPage;

                _ready = true;
            }
        }

        private void RecursiveInit(JSONNode node, int index) {

            if (index == node.Count) {

                _ready = true;

                _restartButton.MouseClick += Restart;
                _exitButton.MouseClick += Exit;
                _newButton.MouseClick += CreateEmptyConnection;
                _link.MouseClick += ToWebPage;

                return;
            }

            JSONNode n = node[index];

            ConnectionModel model = new ConnectionModel(n);

            var ui = new ConnectionUI(model);

            _connectionPanel.Controls.Add(ui);
            _connections.Add(ui);

            ui.Location = new Point(3, 3 + index * 27);

            Timer t = new Timer(100);

            t.Elapsed += (sender, args) => {

                t.Stop();

                if (MainForm.Instance.InvokeRequired){

                    try {
                        MainForm.Instance.Invoke(new Action<JSONNode, int>(RecursiveInit), new object[] { node, index + 1 });
                    } catch (Exception e) {
                    }
                    return;
                }

                RecursiveInit(node, index+1);

            };

            t.Start();

        }

        public void CreateEmptyConnection(object sender, MouseEventArgs args) {

            var ui = new ConnectionUI(new ConnectionModel());
            _connections.Add(ui);

            _connectionPanel.Controls.Add(ui);

            ReOrder();

        }

        public void RemoveConnection(ConnectionUI connection) {

            _connections.Remove(connection);
            _connectionPanel.Controls.Remove(connection);

            SaveAllDumps();
            ReOrder();

        }

        public void SaveAllDumps() {

            if (File.Exists(@"Setup.json")) {
                File.Delete(@"Setup.json");
            }

            FileStream fs = File.Create(@"Setup.json");
            fs.Close();

            using (StreamWriter sw = new StreamWriter(File.Open(@"Setup.json", FileMode.Create), Encoding.UTF8)) {

                sw.WriteLine("{");
                sw.WriteLine("  \"Devices\":[");

                int len = _connections.Count;

                for (int i = 0; i < len; i++){

                    string json = "     "+_connections[i].Model.Dump;

                    if (i < len - 1){
                        json += ",";
                    }

                    sw.WriteLine(json);
                }

                sw.WriteLine("  ]");
                sw.WriteLine("}");

            }

        }

        public void Restart(object sender, MouseEventArgs args) {

            _restartButton.MouseClick -= CreateEmptyConnection;
            _exitButton.MouseClick -= Exit;
            _newButton.MouseClick -= CreateEmptyConnection;
            _link.MouseClick -= ToWebPage;

            foreach (var connectionUi in _connections) {

                _connectionPanel.Controls.Remove(connectionUi);
                connectionUi.Kill();

                Thread.Sleep(10);

            }

            _connections = new List<ConnectionUI>();

            Init();
        }

        private void ReOrder() {

            int len = _connections.Count;

            for (int i = 0; i < len; i++) {
                _connections[i].Location = new Point(3, 3 + i * 27);
            }

        }

        private void Exit(object sender, MouseEventArgs args) {

            foreach (var connectionUi in _connections){

                connectionUi.Kill();
                Thread.Sleep(10);

            }

            Application.Exit();

        }

        private void ToWebPage(object sender, MouseEventArgs args) {
            System.Diagnostics.Process.Start("http://www.truekontrol.com");
        }

        private void ImportStatusForm_Resize(object sender, EventArgs e){
            if (this.WindowState == FormWindowState.Minimized){
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(3000);
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e){
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            _notifyIcon.Visible = false;
        }

        public bool Ready => _ready;

        public static MainForm Instance => _instance;

    }
}

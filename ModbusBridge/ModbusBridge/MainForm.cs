using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModbusBridge.Net;
using ModbusBridge.UI;
using SimpleJSON;
using Timer = System.Timers.Timer;

namespace ModbusBridge{

    public partial class MainForm : Form {

        private static MainForm _instance;

        private List<ConnectionUI> _connections;

        public MainForm() {

            _instance = this;

            InitializeComponent();
            
            _connections = new List<ConnectionUI>();

            Init();

        }

        private void Init() {

            string text = System.IO.File.ReadAllText(@"Setup.json");

            JSONNode node = JSON.Parse(text);

            Timer t = new Timer(1000);

            t.Elapsed += (sender, args) => {

                t.Stop();

                if (MainForm.Instance.InvokeRequired){
                    MainForm.Instance.Invoke(new Action<JSONNode, int>(RecursiveInit), new object[] { node["Devices"], 0 });
                    return;
                }

                RecursiveInit(node["Devices"], 0);

            };

            t.Start();
        }

        private void RecursiveInit(JSONNode node, int index) {

            if (index == node.Count) {
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
                    MainForm.Instance.Invoke(new Action<JSONNode, int>(RecursiveInit), new object[] { node, index+1 });
                    return;
                }

                RecursiveInit(node, index+1);

            };

            t.Start();

        }

        public void CreateEmptyConnection() {

        }

        public void AddConnection(ConnectionUI connection) {

        }

        public void RemoveConnection(ConnectionUI connection) {

        }

        public static MainForm Instance => _instance;

    }
}

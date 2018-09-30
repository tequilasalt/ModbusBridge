using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModbusBridge.Net;
using SimpleJSON;

namespace ModbusBridge{

    public partial class MainForm : Form{

        public MainForm(){

            InitializeComponent();

            string text = System.IO.File.ReadAllText(@"Setup.json");

            JSONNode node = JSON.Parse(text);

            int len = node["Devices"].Count;

            for (int i = 0; i < len; i++){

                JSONNode n = node["Devices"][i];

                var listener = new Connection(n["port"].AsInt, n["com"].Value, n["baudrate"].AsInt, n["stopBits"].Value, n["parity"].Value);
                
            }

        }
    }
}

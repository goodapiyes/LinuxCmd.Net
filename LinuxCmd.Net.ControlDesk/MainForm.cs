using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinuxCmd.Net.NetWork;

namespace LinuxCmd.Net.ControlDesk
{
    public partial class MainForm : Form
    {
        private NetWorker net;
        public MainForm()
        {
            InitializeComponent();
            net = new NetWorker();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            net.Start();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRoomJsBrezzaTest
{
    public partial class BrezzaTestForm : Form
    {
        public BrezzaTestForm()
        {
            InitializeComponent();
            this.Text = "Brezza Test Form";
            js_output.ReadOnly = true;
        }

        private void run_js_Click(object sender, EventArgs e)
        {
            string input = js_input.Text;
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Input is empty.");
                return;
            }
            
        }
    }
}

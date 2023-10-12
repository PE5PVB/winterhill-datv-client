using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace datvreceiver
{
    public partial class freq_select : Form
    {
        public freq_select()
        {
            InitializeComponent();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if ( txtNewFrequency.Text.Length == 0)
            {
                MessageBox.Show("You need to specify a frequency");
                return;
            }

            if (comboNewSR.SelectedIndex < 0)
            {
                MessageBox.Show("You need to specify a Symbol Rate");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

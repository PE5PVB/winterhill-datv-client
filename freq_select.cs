using System;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Dialog for manually selecting frequency and symbol rate for a receiver.
    /// Displays current receiver number and IF offset, allows user to enter new values.
    /// </summary>
    public partial class freq_select : Form
    {
        public freq_select()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Validates input and closes dialog with OK result if valid.
        /// </summary>
        private void btnSet_Click(object sender, EventArgs e)
        {
            // Validate frequency input
            if (txtNewFrequency.Text.Length == 0)
            {
                MessageBox.Show("You need to specify a frequency");
                return;
            }

            // Validate symbol rate selection
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

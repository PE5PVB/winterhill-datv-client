using System;
using System.Drawing;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Provides consistent dark theme styling for all application forms and controls.
    /// Uses a modern Visual Studio-inspired dark color scheme.
    /// </summary>
    public static class ThemeHelper
    {
        #region Color Palette

        /// <summary>Darkest background color for main form backgrounds</summary>
        public static readonly Color BackgroundDark = Color.FromArgb(30, 30, 30);

        /// <summary>Medium background for group boxes and input fields</summary>
        public static readonly Color BackgroundMedium = Color.FromArgb(45, 45, 48);

        /// <summary>Lighter background for buttons and hover states</summary>
        public static readonly Color BackgroundLight = Color.FromArgb(62, 62, 66);

        /// <summary>Border color for controls</summary>
        public static readonly Color BorderColor = Color.FromArgb(67, 67, 70);

        /// <summary>Primary text color (near-white)</summary>
        public static readonly Color TextPrimary = Color.FromArgb(241, 241, 241);

        /// <summary>Secondary/dimmed text color</summary>
        public static readonly Color TextSecondary = Color.FromArgb(160, 160, 160);

        /// <summary>Blue accent for interactive elements and headers</summary>
        public static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);

        /// <summary>Green accent for positive status (locked, connected)</summary>
        public static readonly Color AccentGreen = Color.FromArgb(78, 201, 176);

        /// <summary>Red accent for negative status (no signal, error)</summary>
        public static readonly Color AccentRed = Color.FromArgb(244, 71, 71);

        /// <summary>Orange accent for warnings</summary>
        public static readonly Color AccentOrange = Color.FromArgb(255, 167, 38);

        /// <summary>Header/title bar background color</summary>
        public static readonly Color HeaderBackground = Color.FromArgb(37, 37, 38);

        #endregion

        #region Theme Application Methods

        /// <summary>
        /// Applies dark theme to a form and all its child controls recursively.
        /// </summary>
        /// <param name="form">The form to style</param>
        public static void ApplyTheme(Form form)
        {
            form.BackColor = BackgroundDark;
            form.ForeColor = TextPrimary;
            ApplyThemeToControls(form.Controls);
        }

        /// <summary>
        /// Recursively applies theme to a collection of controls.
        /// </summary>
        public static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyThemeToControl(control);
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// Applies appropriate theme styling based on control type.
        /// </summary>
        public static void ApplyThemeToControl(Control control)
        {
            if (control is Label label)
            {
                label.ForeColor = TextPrimary;
                label.BackColor = Color.Transparent;
            }
            else if (control is Button button)
            {
                StyleButton(button);
            }
            else if (control is TextBox textBox)
            {
                StyleTextBox(textBox);
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = TextPrimary;
                checkBox.BackColor = Color.Transparent;
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.ForeColor = AccentBlue;
                groupBox.BackColor = BackgroundMedium;
            }
            else if (control is Panel panel)
            {
                panel.BackColor = BackgroundDark;
            }
            else if (control is TrackBar trackBar)
            {
                trackBar.BackColor = BackgroundMedium;
            }
            else if (control is ListBox listBox)
            {
                listBox.BackColor = BackgroundMedium;
                listBox.ForeColor = TextPrimary;
                listBox.BorderStyle = BorderStyle.None;
            }
            else if (control is SplitContainer splitContainer)
            {
                splitContainer.BackColor = BackgroundDark;
                splitContainer.Panel1.BackColor = BackgroundDark;
                splitContainer.Panel2.BackColor = BackgroundDark;
            }
            else if (control is StatusStrip statusStrip)
            {
                statusStrip.BackColor = HeaderBackground;
                statusStrip.ForeColor = TextPrimary;
                foreach (ToolStripItem item in statusStrip.Items)
                {
                    item.ForeColor = TextPrimary;
                    item.BackColor = HeaderBackground;
                }
            }
        }

        #endregion

        #region Individual Control Styling

        /// <summary>
        /// Styles a button with flat appearance and hover effect.
        /// </summary>
        public static void StyleButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = AccentBlue;
            button.FlatAppearance.BorderSize = 1;
            button.BackColor = BackgroundLight;
            button.ForeColor = TextPrimary;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += (s, e) => {
                button.BackColor = AccentBlue;
            };
            button.MouseLeave += (s, e) => {
                button.BackColor = BackgroundLight;
            };
        }

        /// <summary>
        /// Styles a text box with dark theme colors.
        /// </summary>
        public static void StyleTextBox(TextBox textBox)
        {
            textBox.BackColor = BackgroundMedium;
            textBox.ForeColor = TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Styles a header label with blue accent color and bold font.
        /// </summary>
        public static void StyleHeaderLabel(Label label)
        {
            label.ForeColor = AccentBlue;
            label.Font = new Font(label.Font, FontStyle.Bold);
        }

        /// <summary>
        /// Styles a data display label, optionally as clickable link.
        /// </summary>
        /// <param name="label">The label to style</param>
        /// <param name="isClickable">If true, style as clickable with hand cursor</param>
        public static void StyleDataLabel(Label label, bool isClickable = false)
        {
            label.ForeColor = TextPrimary;
            if (isClickable)
            {
                label.Cursor = Cursors.Hand;
                label.ForeColor = AccentBlue;
            }
        }

        /// <summary>
        /// Styles a status indicator label with green (good) or red (bad) color.
        /// </summary>
        /// <param name="label">The label to style</param>
        /// <param name="isGood">True for green (good status), false for red (bad status)</param>
        public static void StyleStatusLabel(Label label, bool isGood)
        {
            label.ForeColor = isGood ? AccentGreen : AccentRed;
        }

        #endregion
    }
}

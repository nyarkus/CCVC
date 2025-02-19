﻿using System;
using System.Windows.Forms;

namespace Installer
{
    public partial class License : Form
    {
        public License()
        {
            InitializeComponent();
        }

        private void License_Load(object sender, EventArgs e)
        {
            licenseBox.Text = Prepare.LicenseText;
            licenseBox.ReadOnly = true;
            next.Enabled = false;
        }

        private void next_Click(object sender, EventArgs e)
        {
            var options = new Options();
            options.Size = Size;
            options.StartPosition = StartPosition;
            options.FormClosed += OnClosed;
            options.Show();
            Hide();
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            next.Enabled = checkBox.Checked;
        }

    }
}

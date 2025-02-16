using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        public static bool InstallPlayer = false;
        public static bool AddFileAssociation = false;
        public static bool InstallConverter = false;
        public static string InstallDirectory;

        private void selectDirectory_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Select install directory";

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                var path = dialog.SelectedPath;
                if (Directory.Exists(path) && (Directory.GetFiles(path).Length > 0 || Directory.GetDirectories(path).Length > 0))
                    path = Path.Combine(path, "CCVC");

                var drive = new DriveInfo(path[0].ToString());

                if (!drive.IsReady)
                {
                    MessageBox.Show($"Drive {path[0]} is not ready. Try another", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                directory.Text = path;
            }
        }

        private void Options_Load(object sender, EventArgs e)
        {
            directory.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "CCVC");
            consolePlayer.Checked = true;
            fileAssoc.Checked = true;

            consolePlayer.Text += $" ~{Prepare.PlayerSize / 1024 / 1024}MB";
            converter.Text += $" ~{Prepare.ConverterSize / 1024 / 1024}MB";
            consolePlayer_CheckedChanged(null, null);
        }

        private void consolePlayer_CheckedChanged(object sender, EventArgs e)
        {
            if (!consolePlayer.Checked && !converter.Checked)
            {
                next.Enabled = false;
                return;
            }
            else
                next.Enabled = true;

            long required = 0;
            if (consolePlayer.Checked)
            {
                required += Prepare.PlayerSize;
                fileAssoc.Enabled = true;
            }
            else
                fileAssoc.Enabled = false;

            if (converter.Checked)
                required += Prepare.ConverterSize;

            spaceReq.Text = $"Space required: {required / 1024 / 1024}MB";

            var drive = new DriveInfo(directory.Text[0].ToString());
            if (drive.AvailableFreeSpace < required)
            {
                MessageBox.Show($"There is not enough free space on the {directory.Text[0]} drive for installation", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                next.Enabled = false;
                return;
            }
        }

        private void converter_CheckedChanged(object sender, EventArgs e)
        {
            consolePlayer_CheckedChanged(sender, e);
        }

        private void next_Click(object sender, EventArgs e)
        {
            InstallPlayer = consolePlayer.Checked;
            InstallConverter = converter.Checked;
            if (InstallPlayer)
                AddFileAssociation = fileAssoc.Checked;
            InstallDirectory = directory.Text;

            var installation = new Installation();
            installation.Size = Size;
            installation.StartPosition = StartPosition;
            installation.Show();
            Hide();
        }
    }
}

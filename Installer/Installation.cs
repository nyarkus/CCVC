using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace Installer
{
    public partial class Installation: Form
    {
        public Installation()
        {
            InitializeComponent();
        }

        private CancellationToken cancellation;
        private CancellationTokenSource source;

        #region directories
        private string directory
        {
            get
            {
                return Options.InstallDirectory;
            }
        }
        private string playerDir
        {
            get
            {
                return Path.Combine(directory, "ConsolePlayer");
            }
        }
        private string converterDir
        {
            get
            {
                return Path.Combine(directory, "Converter");
            }
        }
        #endregion

        private async void Installation_Load(object sender, EventArgs e)
        {
            source = new CancellationTokenSource();
            cancellation = source.Token;

            int steps = 0;
            if (Options.InstallPlayer)
                steps+= 2;
            if (Options.InstallConverter)
                steps+= 2;
            if (Options.AddFileAssociation)
                steps++;

            totalProgress.Maximum = steps;

            Directory.CreateDirectory(directory);

            if(Options.InstallPlayer)
            {
                state.Text = "Downloading Console Player...";
                using(var client = new HttpClient())
                {
                    while (true)
                    {
                        var result = await client.GetAsync(Prepare.PlayerURL);
                        if (!result.IsSuccessStatusCode)
                        {
                            var dialog = MessageBox.Show($"API respond with code: \"{result.StatusCode}\"", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                            if (dialog == DialogResult.Cancel)
                            {
                                Cancel();
                                return;
                            }
                            continue;
                        }

                        var stream = await result.Content.ReadAsStreamAsync();
                        var downloaded = await Download(stream);
                        totalProgress.Value++;

                        if (cancellation.IsCancellationRequested)
                            return;

                        state.Text = "Installing Console Player...";
                        Directory.CreateDirectory(playerDir);
                        await Decompress(playerDir, downloaded);
                        totalProgress.Value++;
                        downloaded.Dispose();
                        stream.Dispose();
                    }
                }
            }
            if(Options.InstallConverter)
            {
                state.Text = "Downloading Converter...";
                using (var client = new HttpClient())
                {
                    while (true)
                    {
                        var result = await client.GetAsync(Prepare.ConverterURL);
                        if (!result.IsSuccessStatusCode)
                        {
                            var dialog = MessageBox.Show($"API respond with code: \"{result.StatusCode}\"", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                            if (dialog == DialogResult.Cancel)
                            {
                                Cancel();
                                return;
                            }
                            continue;
                        }

                        var stream = await result.Content.ReadAsStreamAsync();
                        var downloaded = await Download(stream);
                        totalProgress.Value++;

                        if (cancellation.IsCancellationRequested)
                            return;

                        state.Text = "Installing Converter...";
                        Directory.CreateDirectory(converterDir);
                        await Decompress(converterDir, downloaded);
                        totalProgress.Value++;
                        downloaded.Dispose();
                        stream.Dispose();
                    }
                }
            }
            if(Options.AddFileAssociation)
            {
                if (cancellation.IsCancellationRequested)
                    return;

                state.Text = "Creating file association...";
                AddToRegistry();
                totalProgress.Value++;
            }

            MessageBox.Show("CCVC Successfully installed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Environment.Exit(0);
        }

        #region Methods
        private async Task<Stream> Download(Stream stream)
        {
            var memory = new MemoryStream();

            long readed = 0;
            taskProgress.Value = 0;
            taskProgress.Maximum = (int)(stream.Length / 1024);

            while(stream.Position < stream.Length)
            {
                if (cancellation.IsCancellationRequested)
                    return null;

                byte[] buffer = new byte[1024];
                readed += await stream.ReadAsync(buffer, 0, 1024);
                memory.Write(buffer, 0, buffer.Length);

                taskProgress.Value = (int)(readed / 1024);
            }

            return memory;
        }

        private async Task Decompress(string path, Stream input)
        {
            var archive = new ICSharpCode.SharpZipLib.Zip.ZipFile(input);

            taskProgress.Value = 0;
            taskProgress.Maximum = (int)archive.Count;

            foreach (var obj in archive)
            {
                var entry = (ZipEntry)obj;

                using (var zipStream = archive.GetInputStream(entry))
                using (var fileStream = File.Create(Path.Combine(path, entry.Name)))
                {
                    if (cancellation.IsCancellationRequested)
                        return;

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await zipStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                taskProgress.Value++;
            }
        }

        private const string progId = "CCVC";
        private static void AddToRegistry()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\.ccv"))
            {
                key.SetValue("", progId);
            }

            var applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsolePlayer.exe");
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CCV.ico");

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{progId}"))
            {
                key.SetValue("", "Console Player");
                key.CreateSubKey("DefaultIcon").SetValue("", iconPath);
                key.CreateSubKey("shell\\open\\command").SetValue("", $"\"{applicationPath}\" \"%1\"");
            }

            IconUpdater.UpdateIcons();
        }

        private static void RemoveRegistry()
        {
            Registry.CurrentUser.DeleteSubKey($"Software\\Classes\\.ccv");
            var applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsolePlayer.exe");
            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{progId}");

            IconUpdater.UpdateIcons();
        }
        #endregion
        private async void Cancel()
        {
            source.Cancel();
            totalProgress.Value = 0;
            taskProgress.Value = 0;
            state.Text = "Aborting...";
            Directory.Delete(directory, true);
            Environment.Exit(0);
        }

        private void abort_Click(object sender, EventArgs e)
        {
            Cancel();
        }
    }
}

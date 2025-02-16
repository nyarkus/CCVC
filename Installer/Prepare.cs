using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Installer
{
    public partial class Prepare : Form
    {
        public static string LicenseText;

        public static string ConverterURL;
        public static long ConverterSize;

        public static string PlayerURL;
        public static long PlayerSize;

        private const int steps = 3;
        public Prepare()
        {
            InitializeComponent();
        }

        private async void Prepare_Load(object sender, EventArgs e)
        {
            progressBar.Maximum = steps;
            progressBar.Value = 0;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("HttpClient");

                    // License
                    HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/nyarkus/CCVC/contents/LICENSE.txt");
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        var json = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                        string contentBase64 = json.Value<string>("content");

                        byte[] data = Convert.FromBase64String(contentBase64);
                        LicenseText = System.Text.Encoding.UTF8.GetString(data);
                    }
                    else
                    {
                        throw new Exception($"{response.StatusCode}");
                    }
                    progressBar.Value++;


                    // Download URL's
                    response = await client.GetAsync("https://api.github.com/repos/nyarkus/CCVC/releases/latest");
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        var json = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                        var assets = (JArray)json["assets"];

                        foreach (var asset in assets)
                        {
                            var name = asset["name"].ToString();
                            if (name == "consoleplayer.zip")
                            {
                                PlayerURL = asset["browser_download_url"].ToString();
                                PlayerSize = asset["size"].Value<long>();
                                progressBar.Value++;
                            }
                            else if (name == "converter.zip")
                            {
                                ConverterURL = asset["browser_download_url"].ToString();
                                ConverterSize = asset["size"].Value<long>();
                                progressBar.Value++;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"{response.StatusCode}");
                    }
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("Failed to get the required data for the installation. Check the internet connection and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The API returned an error \"{ex.Message}\". try again later", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }

                var license = new License();
                license.FormClosed += OnClosed;
                license.Show();
                Hide();
            }
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
    }
}

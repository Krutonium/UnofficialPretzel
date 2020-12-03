using System;
using System.Timers;
using Eto.Drawing;
using Eto.Forms;
using GLib;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Process = System.Diagnostics.Process;

namespace PretzelRocks
{
    class Program
    {
        static void Main(string[] args)
        {
            new Eto.Forms.Application().Run(new MyForm());
        }
    }

    class MyForm : Eto.Forms.Form
    {
        static WebView Pretzel = new WebView();
        Settings settings = new Settings();
        Timer _timer = new Timer();
        public MyForm()
        {
            if(File.Exists("./settings.json"))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("./settings.json"));
            }
            
            
            TabControl Tabs = new TabControl();
            TabPage PretzelPlayer = new TabPage();
            Tabs.Pages.Add(PretzelPlayer);
            Pretzel.Url = new Uri("https://play.pretzel.rocks/home");
            Pretzel.DocumentLoaded += PretzelOnDocumentLoaded;
            PretzelPlayer.Content = Pretzel;
            PretzelPlayer.Text = "Pretzel Player";
            
            
            
            TabPage Settings = new TabPage();
            Tabs.Pages.Add(Settings);
            Settings.Text = "Settings";
            
            StackLayout settingsLayout = new StackLayout();
            Settings.Content = settingsLayout;
            TextBox twitchId = new TextBox();
            twitchId.PlaceholderText = "Twitch Username";
            twitchId.Width = 200;
            twitchId.Text = settings.UserID;
            settingsLayout.Items.Add(twitchId);
            Button getTwitchId = new Button();
            getTwitchId.Text = "Get Twitch ID";
            getTwitchId.Width = 200;
            getTwitchId.Click += (sender, args) =>
            {
                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string result = client.UploadString(new Uri("https://twitchuidlookup.herokuapp.com/resolve"), "names=" + twitchId.Text);
                //twitchId.Text = JsonConvert.DeserializeObject<twitchID>(result).pfckrutonium;
                string tempObject = Convert.ToString(JsonConvert.DeserializeObject(result));
                tempObject = tempObject.Replace("{", "");
                tempObject = tempObject.Replace("}", "");
                tempObject = tempObject.Trim();
                tempObject = tempObject.Split(":")[1];
                tempObject = tempObject.Replace("\"", "");
                tempObject = tempObject.Trim();
                twitchId.Text = tempObject;
                settings.UserID = twitchId.Text;
            };
            settingsLayout.Items.Add(getTwitchId);
            Button save = new Button();
            save.Text = "Save";
            save.Width = 200;
            
            CheckBox SaveAlbumArt = new CheckBox();
            SaveAlbumArt.Checked = settings.ShouldSaveAlbumArt;
            if (settings.AlbumArtSaveLocation == null)
            {
                SaveAlbumArt.Enabled = false;
            }
            SaveAlbumArt.CheckedChanged += 
                (sender, args) => settings.ShouldSaveAlbumArt = (bool) SaveAlbumArt.Checked;
            CheckBox SaveAlbumInfo = new CheckBox();
            SaveAlbumInfo.Checked = settings.ShouldSaveAlbumInfo;
            if (settings.AlbumInfoSaveLocation == null)
            {
                SaveAlbumInfo.Enabled = false;
            }
            SaveAlbumInfo.CheckedChanged +=
                (sender, args) => settings.ShouldSaveAlbumInfo = (bool) SaveAlbumInfo.Checked;
            SaveAlbumArt.Text = "Save Album Art";
            SaveAlbumInfo.Text = "Save Album Info";

            if (settings.ShouldSaveAlbumArt || settings.ShouldSaveAlbumInfo)
            {
                _timer.Enabled = true;
            }
            else
            {
                _timer.Enabled = false;
            }

            _timer.Interval = 1000;
            _timer.Elapsed += TimerOnElapsed;
            settingsLayout.Items.Add(SaveAlbumArt);
            settingsLayout.Items.Add(SaveAlbumInfo);

            Button saveLocationAlbum = new Button();
            Button saveLocationInfo = new Button();
            saveLocationAlbum.Width = 200;
            saveLocationInfo.Width = 200;
            saveLocationAlbum.Text = "Save Album Art";
            saveLocationInfo.Text = "Save Album Info";
            
            
            saveLocationAlbum.Click += (sender, args) =>
            {
                SaveFileDialog SF = new SaveFileDialog();
                DialogResult result = SF.ShowDialog(this);
                if (result == DialogResult.Ok)
                {
                    settings.AlbumArtSaveLocation = SF.FileName;
                    SaveAlbumArt.Enabled = true;
                }
            };
            saveLocationInfo.Click += (sender, args) =>
            {
                SaveFileDialog SF = new SaveFileDialog();
                DialogResult result = SF.ShowDialog(this);
                if (result == DialogResult.Ok)
                {
                    settings.AlbumInfoSaveLocation = SF.FileName;
                    SaveAlbumInfo.Enabled = true;
                }
            };


            settingsLayout.Items.Add(saveLocationAlbum);
            settingsLayout.Items.Add(saveLocationInfo);


            save.Click += (sender, args) => SaveSettings();
            settingsLayout.Items.Add(save);
            settingsLayout.Spacing = 5;
            settingsLayout.Padding = 5;
            this.ClientSize = new Size(800,600);

            this.Content = Tabs;
            Console.WriteLine("Phase 1");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (settings.ShouldSaveAlbumArt)
            {
                //Not Implemented
            }
            if (settings.ShouldSaveAlbumInfo)
            {
                WebClient client = new WebClient();
                File.WriteAllText(settings.AlbumInfoSaveLocation, 
                    client.DownloadString("https://api.pretzel.tv/playing/twitch/" + settings.UserID));
            }
        }

        private void PretzelOnDocumentLoaded(object sender, WebViewLoadedEventArgs e)
        {
            Console.WriteLine("Phase 2");
            //document.evaluate('/html/body/div[1]/div/div/div/div[5]/div/div[3]/div[2]/img', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            //Console.WriteLine(Pretzel.ExecuteScript("alert(document.evaluate(//img[1], document, null))"));
        }

        public void SaveSettings()
        {
            File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public class Settings
        {
            public string UserID;
            public string AlbumArtSaveLocation;
            public string AlbumInfoSaveLocation;
            public bool ShouldSaveAlbumArt;
            public bool ShouldSaveAlbumInfo;
        }
        
    }
    //document.evaluate('/html/body/div[1]/div/div/div/div[5]/div/div[3]/div[2]/img', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
}
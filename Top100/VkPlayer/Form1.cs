using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VkPlayer
{
    public partial class Form1 : Form
    {
        WMPLib.IWMPPlaylist PlayList; //создаем плейлист
        WMPLib.IWMPMedia Media;// работаем с ним (корректируем)
        string[] ArrayOfSongs = new string[100];//массив для топ 100 трэков
        string[] TmpArrayUrl;//временный массив для url при скачивании
        string[] TmpArrayName;//временный массив для имен трэков при скачивании
        int real_count_tracks;//реальное количество трэков
        List<string> TrackInList = new List<string>();//лист дя доступных трэков

        public Form1()
        {
            InitializeComponent();//инициализируем компоненты 1-й формы
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Form2().Show();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!Settings1.Default.Auth) { Thread.Sleep(500); }
            AddTopTracks();
            VkQuery();
        }

        private void AddTopTracks()//находим и добавляем в массив top 100 трэков
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Отсутствует или ограниченно физическое подключение к сети\nПроверьте настройки вашего сетевого подключения");
                return;
            }
            else
            {
                listBox2.Items.Add(" Please wait a little...");
                listBox2.Enabled = false;
                DateTime date = DateTime.Today;
                listBox1.Items.Clear();
                listBox1.Items.Add(" Top tracks " + date.ToString("dddd dd MMMM",
                    CultureInfo.CreateSpecificCulture("en-US")) + ':');
                WebClient w = new WebClient();
                listBox1.Items.Add("");
                string page = w.DownloadString("http://www.bptoptracker.com/top/top100/0/");
                string track2 = "class=\"cover\"><a href=\"/track/(.*?)\" title=\"(.*?)\"";
                string track1 = "class=\"artists\"><a href=\"/artist/(.*?)\" title=\"(.*?)\">(.*?)";

                int i = 0;
                foreach (Match match in Regex.Matches(page, track1))
                {
                    if (i == 100)
                        break;
                    ArrayOfSongs[i] = match.Groups[2].Value;
                    i++;
                }
                i = 0;

                foreach (Match match in Regex.Matches(page, track2))
                {
                    if (i == 100)
                        break;
                    ArrayOfSongs[i] += " - " + match.Groups[2].Value;
                    i++;
                }
                i = 0;
                for (; i < 100; i++)
                {
                    listBox1.Items.Add((i + 1).ToString() + ".  " + ArrayOfSongs[i]);
                }

            }
        }

        private void VkQuery() 
        {
        // Чтобы обратиться к методу API ВКонтакте, Вам необходимо выполнить POST или GET запрос такого вида:
        //https://api.vk.com/method/METHOD_NAME?PARAMETERS&access_token=ACCESS_TOKEN&v=V

            string mytoken = Settings1.Default.Token;
            string error = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<response>\n <count>0</count>\n <items>\n </items>\n</response>\n";
            string captcha = "<error_msg>Captcha needed</error_msg>";
            string http_captcha="<captcha_img>(.*?)&amp;s=1</captcha_img>";
            string sid_captcha= "<captcha_sid>(.*?)</captcha_sid>";
            string captcha_https="";
            string captcha_sid="";
            for (int i = 0; i < 100; i++)//добавляем реально сушествующие трэки в лист
            {
                 string resp = GET_http("https://api.vk.com/method/audio.search.xml?q=" + ArrayOfSongs[i] + "&count=1&access_token=" + mytoken + "&v=5.53");
                 Settings1.Default.Qwery = resp;

//блок работы с капчей
//------------------------------------------------------------------------------------
                bool b = resp.Contains(captcha);
                if (b == true)
                {
                   
                    foreach (Match match in Regex.Matches(resp, http_captcha))
                    {
                        captcha_https = match.Groups[1].Value;
                    }
                    foreach (Match match2 in Regex.Matches(resp, sid_captcha))
                    {
                        captcha_sid = match2.Groups[1].Value;
                    }

                    Process.Start("IExplore.exe", captcha_https);
                    
                    Settings1.Default.CaptchaSet = captcha_https;
                    Settings1.Default.CaptchaSid = captcha_sid;
                    Settings1.Default.tmpArrOfSong = ArrayOfSongs[i];
                    new Form3().ShowDialog();
                }
//-------------------------------------------------------------------------------------
                resp = Settings1.Default.Qwery;
                //если ответ положительный, добавляем ответ от сервера в list
                bool c = resp.Contains("</audio>\n </items>\n</response>\n");
                if (resp != error && c==true)
              //      if (resp != error )
                TrackInList.Add(resp);
                //задержка чтобы сервер не блокировал, если заблокирует нужна обработка капчи :(
                //можно совершать 5 запросов в секундe, также может быть временно ограничен (в таком случае сервер не возвращает ответ на вызов конкретного метода, но без проблем обрабатывает любые другие запросы).
                Thread.Sleep(210);
            }


            MessageBox.Show("Available " + TrackInList.Count.ToString()+" tracks");// показываем сколько трэков доступно для прослушивания
            real_count_tracks = TrackInList.Count;
            string[] array_url = new string[real_count_tracks];
            string[] array_name = new string[real_count_tracks];


            int j = 0;
            this.Invoke((MethodInvoker)delegate()
            {
               string track1 = "<url>(.*?)</url>";
               string track2 = "<artist>(.*?)</artist>\n   <title>(.*?)</title>";

               for (int i = 0; i < real_count_tracks; i++)
               {
                   foreach (Match match in Regex.Matches(TrackInList[i].ToString(), track1))
                   {
                       array_url[i] = match.Groups[1].Value;
                       j++;
                       if (j == 1)
                           break;
                   }

                   j = 0;

                   foreach (Match match in Regex.Matches(TrackInList[i], track2))
                   {
                       array_name[i] = match.Groups[1].Value +" - "+ match.Groups[2].Value;
                       j++;
                       if (j == 1)
                           break;
                   }
               }
               listBox2.Enabled = true;
               listBox2.Items.Clear();
               TmpArrayUrl = array_url;
               TmpArrayName = array_name;
               PlayList = axWindowsMediaPlayer1.playlistCollection.newPlaylist("Beatport playlist");
               for (int i = 0; i < real_count_tracks; i++)
               {
                   listBox2.Items.Add((i + 1).ToString() + ". " + array_name[i]+"  "+ array_url[i]);
                   Media = axWindowsMediaPlayer1.newMedia(array_url[i]);
                   PlayList.appendItem(Media);
               }
               axWindowsMediaPlayer1.currentPlaylist = PlayList;
               axWindowsMediaPlayer1.Ctlcontrols.stop();
            });
        }

        //Get запроc на сервер вконтакте
       static public string GET_http(string url)
        {   //первая строка  некоторых источниках закомментирована
            //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.WebRequest reqGet = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = reqGet.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string html = sr.ReadToEnd();
            //sr.Close();
            return html;
        }

        //событие при нажатии кнопки мышки в проигрывателе
        private void listBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                axWindowsMediaPlayer1.Ctlcontrols.currentItem = axWindowsMediaPlayer1.currentPlaylist.get_Item(listBox2.SelectedIndex);

            }
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                string tmp_url = TmpArrayUrl[Settings1.Default.ListIndex];
                Settings1.Default.SongUrl = tmp_url;
                string tmp_name = TmpArrayName[Settings1.Default.ListIndex];
                Settings1.Default.SongName = tmp_name;
                new Form4().Show();
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.TopIndex != listBox1.SelectedIndex)
                // Make the currently selected item the top item in the ListBox.
                listBox2.TopIndex = listBox2.SelectedIndex;
            Settings1.Default.ListIndex = listBox2.SelectedIndex;
            string name2 = listBox2.SelectedIndex.ToString();
        }
//        private void listBox2_MouseDown(object sender, MouseEventArgs e)
//        {
//            if (e.Button == System.Windows.Forms.MouseButtons.Right)
//            {
//  //              MessageBox.Show(listBox1.SelectedItem.ToString());
//                int y = e.Y / listBox1.ItemHeight;
//                if (y < listBox1.Items.Count)
//                {
//                    listBox1.SelectedIndex = listBox1.TopIndex + y;
//                    string tmp_url = TmpArrayUrl[listBox1.SelectedIndex];
//                    Settings1.Default.SongUrl = tmp_url;
//                    string tmp_name = TmpArrayName[listBox1.SelectedIndex];
//                    Settings1.Default.SongName = tmp_name;
////                    MessageBox.Show(tmp_url);
//                    new Form4().Show();
//                }
//            }
//        }
    }
}

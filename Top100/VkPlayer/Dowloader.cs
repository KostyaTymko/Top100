using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace VkPlayer
{
    public partial class Form4 : Form
    {
        WebClient wc = new WebClient();

        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(FileDownloadComplete);
            Uri songs_url = new Uri(Settings1.Default.SongUrl);
            //wc.DownloadFileAsync(songs_url, "D:\\" + Settings1.Default.SongName + ".mp3");
            wc.DownloadFileAsync(songs_url, Settings1.Default.SongName + ".mp3");
            this.Close();
        }

        private void FileDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            this.Close();
            MessageBox.Show("Download completed");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}

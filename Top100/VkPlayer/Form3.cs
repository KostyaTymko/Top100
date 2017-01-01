using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VkPlayer
{
    public partial class Form3 : Form
    {

        TextBox textBox;

          public Form3()
        {
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Size = new Size(300, 150);
            this.Text = "Captcha";

            /* Создаем текстовое поле. -> */

            textBox = new TextBox();
            textBox.Size = new Size(250, 25);
            textBox.Font = new Font(TextBox.DefaultFont, FontStyle.Regular);
            textBox.Location = new Point(20, 50);
            textBox.Text = "";

            this.Controls.Add(textBox);
            textBox.Show();
            textBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);

            /* Создаем метку. -> */

            Label label = new Label();
            label.AutoSize = false;
            label.Size = new Size(250, 25);
            label.Font = new Font(label.Font, FontStyle.Regular);
            label.Location = new Point(20, 25);
            label.Text = "Enter key: ";

            this.Controls.Add(label);
            label.Show();

            /* Создаем метку. <- */

            /* Создаем кнопку "OK". -> */

            Button buttonOK = new Button();
            buttonOK.Size = new Size(80, 25);
            buttonOK.Location = new Point(105, 75);
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Text = "OK";

            this.Controls.Add(buttonOK);
            buttonOK.Show();

            /* Создаем кнопку "Cancel". -> */
            Button buttonCancel = new Button();
            buttonCancel.Size = new Size(80, 25);
            buttonCancel.Location = new Point(190, 75);
            buttonCancel.Text = "Cancel";

            this.Controls.Add(buttonCancel);
            buttonCancel.Show();

            buttonCancel.Click += new EventHandler(buttonCancel_Click);
            buttonOK.Click += new EventHandler(buttonOk_Click);
        }

        public void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == (Char)Keys.Enter)
            //{
            //    this.DialogResult = System.Windows.Forms.DialogResult.OK;

            //    this.Close();
            //}
        }

        public void buttonCancel_Click(object sander, EventArgs e)
        {
            this.Close();
        }
        public void buttonOk_Click(object sander, EventArgs e)
        {
            string CaptchaCode = getString();
            string resp2 = Form1.GET_http("https://api.vk.com/method/audio.search.xml?q=" + Settings1.Default.tmpArrOfSong + "&count=1&access_token=" + Settings1.Default.Token + "&v=5.53" + "&captcha_sid=" + Settings1.Default.CaptchaSid + "&captcha_key=" + CaptchaCode);
            MessageBox.Show(resp2);
            Settings1.Default.Qwery = resp2;
        }


        public string getString()
        {
  //        if (this.ShowDialog() != System.Windows.Forms.DialogResult.OK)
  //              return null;
            return textBox.Text;
        }
    }
}

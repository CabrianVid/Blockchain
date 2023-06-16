using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace Vaja4___p2p
{
    public partial class Form1 : Form
    {
        TcpClient client;

        List<Chat> chats = new List<Chat>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            //string ip = textBox2.Text;
            string str_port = textBox3.Text;
            int port = 0;

            if (username.Length == 0)
            {
                MessageBox.Show("Niste vnesli uporabniškega imena prosim vnesite uporabniško ime");
            }
            /*else if (ip.Length == 0) {
                MessageBox.Show("Niste vnesli ip naslova prosim vnesite ip naslov");
            }*/
            else if (str_port.Length == 0)
            {
                MessageBox.Show("Niste vnesli porta (vrat )prosim vnesite port");
            }
            else
            {
                port = Int32.Parse(str_port);
                /*Chat chat_ = new Chat(port, username);
                chats.Add(chat_);
                chat_.Show();*/
                //Chat chat_form = new Chat(ip, port, username);
                new Thread(() => new Chat(port, username).ShowDialog()).Start();
                //Send("/D testings!", client);
                if (textBox3.Text == "42069")
                {
                    textBox3.Text = "42169";
                }
                else if (textBox3.Text == "42169")
                {
                    textBox3.Text = "42109";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Send("/D testings!", client);
        }
    }
}

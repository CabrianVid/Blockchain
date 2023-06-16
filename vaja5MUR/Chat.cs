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
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.IO;
using System.Net;

namespace Vaja4___p2p
{
    public partial class Chat : Form
    {
        class Block
        {
            public int Index { get; set; }
            public DateTime Timestamp { get; set; }
            public string Data { get; set; }
            public string Hash { get; set; }
            public string Previous_hash { get; set; }
            public int Difficulty { get; set; }
            public int Nonce { get; set; }
            public string Miner { get; set; }

            public Block() { }

            public Block(int index_, DateTime timestamp_, string data_, string hash_, string previous_hash_, int difficulty_, int nonce_, string miner_)
            {
                Index = index_;
                Timestamp = timestamp_;
                Data = data_;
                Hash = hash_;
                Previous_hash = previous_hash_;
                Difficulty = difficulty_;
                Nonce = nonce_;
                Miner = miner_;
            }

            /*public string stringify() {
                List<string> temp;
                temp.Add(index.ToString());
                temp.Add(index.ToString());
            }*/
        }

        class User
        {
            public TcpClient user_client;
            public string username;
            public User(TcpClient client_, string username_)
            {
                user_client = client_;
                username = username_;
            }
            string getUsername()
            {
                return username;
            }
        }

        private delegate void SafeCallDelegate(ListBox list_box_, string text, bool scrol_to_end);// za med niten dostop do  forms elementov
        const int packet_size_KB = 111;
        List<User> client_users = new List<User>();
        List<Block> chain = new List<Block>();
        string partner_ip;
        string my_ip = "127.0.0.1";

        int partner_port;
        string username;
        string last_message_username = "";// z tem beležimo, kdo je poslal zadnje spročilo, da lahko v primeru ne ujemanja izpišemo header z usernamom ob spročilu novega uporabnika

        List<User> users = new List<User>();
        int my_port;


        public Chat(int port_, string username_)
        {
            my_port = port_;
            username = username_;
            InitializeComponent();
            label1.Text += username;
            label6.Text += port_;

            Thread thread = new Thread(new ThreadStart(ServerThread));
            thread.Name = "server main";
            thread.Start();


            
        }
       
        void ServerThread()//za spostavitev povezave med clientom in serverom, ko se client poveze na server se ustvari nov thread
        {

            TcpListener listener = new TcpListener(IPAddress.Parse(my_ip), my_port);
            listener.Start();

            bool run = true;
            for (int i = 0; run; i++)
            {
                Console.WriteLine("Poslušamo!");
                TcpClient server_client = new TcpClient();
                server_client = listener.AcceptTcpClient();

                Thread thread = new Thread(new ParameterizedThreadStart(Connection));
                thread.Name = "server sub (" + i.ToString() + ")";
                thread.Start(server_client);
                
            }
        }

        void Connection(object client_)
        {
            TcpClient server_client = (TcpClient)client_;


            string connection_message = Recieve(server_client);

            List<string> args = (List<string>)JsonSerializer.Deserialize(connection_message, typeof(List<string>));//prejmemo seznam stringov, ki vsebujejo podatke o partnerju(o osebi as kiro se povezujemo)
            string server_username = args[1];


            if (args[0] == "/C")
            {
                users.Add(new User(server_client, server_username));//dodamo uporabnika v seznam uporabnikov
                List<string> usernames = new List<string>();//seznam uporabnikov, ki ga pošljemo partnerju(o osebi as kiro se povezujemo)
                foreach (User u in users) usernames.Add(u.username);
                AddTextSafe(listBox1, "uporabnik: " + server_username + " - se je povezal na naš client");
                AddTextSafe(listBox1, "trenutno povezani uporabniki na nas: " + JsonSerializer.Serialize(usernames));
                Send(username, server_client);//pošljemo uporabniku naš username
            }
            else
            {
                MessageBox.Show("Napak pri pridobivanju uporabniškega imena.");
                return;
            }

            bool run = true;
            while (run)
            {
                string message = Recieve(server_client);
                args = (List<string>)JsonSerializer.Deserialize(message, typeof(List<string>));//prejmemo seznam stringov, ki vsebujejo podatke o partnerju(o osebi as kiro se povezujemo)

                switch (args[0])
                {
                    case "/M":
                        addMessageToListbox(server_username, args[1]);//tega ne uporabljamo
                        break;
                    case "/B"://ko se kreira nov blog
                        List<Block> recieved_chain = (List<Block>)JsonSerializer.Deserialize(args[1], typeof(List<Block>));
                        CompareChain(recieved_chain, server_client, server_username); //primerjamo prejeto verigo z našo
                        break;
                }
            }
        }

        void addMessageToListbox(string username_, string message)//izpisovanje sporocil
        {
            if (last_message_username != username_)
            {
                AddTextSafe(listBox1, "   " + username_ + ": ");
                last_message_username = username_;
            }
            AddTextSafe(listBox1, message);
        }


        string Recieve(TcpClient client_)
        {
            NetworkStream stream = client_.GetStream();
            try
            {
                
                byte[] byte_message = new byte[1024 * packet_size_KB];
                int len = stream.Read(byte_message, 0, byte_message.Length);
                
                string encrypted_message = Encoding.UTF8.GetString(byte_message, 0, len);
                return encrypted_message;
            }
            catch (Exception e)
            {
                MessageBox.Show("Prišlo je do napake pri pošiljanju sporočila: \n" + e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        void Send(string message, TcpClient client_)
        {
            NetworkStream stream = client_.GetStream();
            try
            {
                byte[] byte_message = Encoding.UTF8.GetBytes(message);
                stream.Write(byte_message, 0, byte_message.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Prišlo je do napake pri pošiljanju sporočila: \n" + e.Message + "\n" + e.StackTrace);
            }
        }

        
        private void AddTextSafe(ListBox list_box_, string text, bool scrol_to_end = false)
        {
            if (list_box_.InvokeRequired)
            {
                var d = new SafeCallDelegate(AddTextSafe);
                list_box_.Invoke(new MethodInvoker(delegate {
                    list_box_.Items.Add(text);
                    if (scrol_to_end) list_box_.TopIndex = list_box_.Items.Count - 1;

                }));
            }
            else
            {
                list_box_.Items.Add(text);
                if (scrol_to_end) list_box_.TopIndex = list_box_.Items.Count - 1;

            }
        }

        string Sha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        int diff = 2;       
        bool mine = false;
        void Mine()//rudarjenje
        {
            while (mine)
            {
                string diff_compare = "";
                string hash = "";
                for (int i = 0; i < diff; i++)
                {
                    diff_compare += "0";
                    hash += "X";
                }

                int index = 0;

                string data = "sporocilo";
                

                string previous_hash = "0";


                DateTime time_stamp = DateTime.Now;
                int nonce = 0;
                bool valid_block = false;
                while (!valid_block && mine)
                {
                    time_stamp = DateTime.Now;
                    if (chain.Count() > 0) previous_hash = chain[chain.Count() - 1].Hash;
                    if (chain.Count() > 0) index = chain.Count();

                    string to_hash = index.ToString() + time_stamp + data + previous_hash + diff.ToString() + nonce.ToString();
                    hash = Sha256Hash(to_hash);
                    float time_diff_from_now = (DateTime.Now - time_stamp).Minutes;
                    float time_diff_from_prev = 0;
                    if (chain.Count() > 0) time_diff_from_prev = (time_stamp - chain[chain.Count() - 1].Timestamp).Minutes;//racuna razliko v minutah med trenutnim casom in casom prejsnjega bloka
                    valid_block = hash.Substring(0, diff) == diff_compare && time_diff_from_now < 1 && time_diff_from_prev < 1;//preverjamo ce je hash validen in ce je cas med bloki manjsi od 1 minute
                    if (!valid_block)
                    {
                        AddTextSafe(listBox2, "wrong: " + hash + " diff: " + diff.ToString());
                    }
                    nonce++;
                }
                if (!mine) break;
                chain.Add(new Block(index, time_stamp, data, hash, previous_hash, diff, nonce, username));//dodamo blok v verigo

                AddTextSafe(listBox2, "correct: " + hash + " diff: " + diff.ToString());
                AddTextSafe(listBox2, "Broadcasting our BlockChain:");

                CheckTimeDiff();

                List<string> param = new List<string>();
                param.Add("/B");
                param.Add(JsonSerializer.Serialize(chain));
                
                PrintChain();
                
            }
        }

        const int diff_n_interval = 3;//koliko casa mora minuti med bloki da se spremeni tezavnost
        const float block_gen_time = 10;//koliko casa naj traja generiranje bloka
        const float diff_sensetivity_multiplier = 2;//koliko se spremeni tezavnost

        void CheckTimeDiff()//urejamo spremembe tezavnosti
        {
            if (chain.Count() < diff_n_interval || (chain.Count() % diff_n_interval) != 0) return;
            Block prevAdjBlock = chain[chain.Count() - diff_n_interval];
            float expected_time = diff_n_interval * block_gen_time;
            Block last_block = chain[chain.Count() - 1];
            float taken_time = (last_block.Timestamp - prevAdjBlock.Timestamp).Seconds;
            int t = chain.Count();
            if (taken_time < expected_time / diff_sensetivity_multiplier)
            {
                diff++;
                AddTextSafe(listBox1, "!!!!");
                AddTextSafe(listBox1, "raised the difficulty to: " + diff.ToString());
                AddTextSafe(listBox1, "!!!!");
            }
            else if (taken_time > expected_time * diff_sensetivity_multiplier)
            {
                diff--;
                AddTextSafe(listBox1, "!!!!");
                AddTextSafe(listBox1, "lowered the difficulty to: " + diff.ToString());
                AddTextSafe(listBox1, "!!!!");
            }
        }

        void PrintChain()//izpis bloka
        {
            foreach (Block block in chain)
            {
                AddTextSafe(listBox1, "Block " + (block.Index + 1).ToString() + "[");
                AddTextSafe(listBox1, "     data: " + block.Data);
                AddTextSafe(listBox1, "     Time stamp: " + block.Timestamp);
                AddTextSafe(listBox1, "     Previous hash: ");
                AddTextSafe(listBox1, "         " + block.Previous_hash);
                AddTextSafe(listBox1, "     Difficulty: " + block.Difficulty.ToString());
                AddTextSafe(listBox1, "     Nonce: " + block.Nonce.ToString());
                AddTextSafe(listBox1, "     Miner: " + block.Miner);
                AddTextSafe(listBox1, "     Hash: ");
                AddTextSafe(listBox1, "         " + block.Hash);
                AddTextSafe(listBox1, "]", true);
            }
        }

        void CompareChain(List<Block> chain_, TcpClient client_, string username_)//primerjamo verigi
        {
            //preveris tezavnosti verig
            double comulative_diff_ours = 0;
            double comulative_diff_recv = 0;
            foreach (Block block in chain)
            {
                comulative_diff_ours += System.Math.Pow(2, block.Difficulty);
            }
            foreach (Block block in chain_)
            {
                comulative_diff_recv += System.Math.Pow(2, block.Difficulty);
            }
            if (comulative_diff_recv > comulative_diff_ours)//ce je tisti chain ki ga sprejmemo "tezji" ga spremenimo da je nas chain tisti
            {
                chain = chain_;
                AddTextSafe(listBox1, "Recieved and updated BlockChain:");
                List<string> param = new List<string>();
                param.Add("/B");
                param.Add(JsonSerializer.Serialize(chain));
                PrintChain();
            }
            else if (comulative_diff_recv < comulative_diff_ours)//drugace pa posljemo drugim nas chain
            {
                AddTextSafe(listBox1, "Recieved and discarded BlockChain!");
                bool found = false;
                foreach (User u in client_users)
                {
                    if (u.username == username_)
                    {
                        found = true;
                        AddTextSafe(listBox1, "Sender is connected (sender was found in our list of outgoing peers)!");
                        AddTextSafe(listBox1, "Responding with our BlockChain to sender!");

                        List<string> param = new List<string>();
                        param.Add("/B");
                        param.Add(JsonSerializer.Serialize(chain));

                        Send(JsonSerializer.Serialize(param), u.user_client);
                    }
                }
                if (!found)
                {
                    AddTextSafe(listBox1, "Sender is NOT connected (sender was NOT found in our list of outgoing peers)!");
                    AddTextSafe(listBox1, "NO response given to sender!");
                }
            }
        }


        
        

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client_client = new TcpClient();
                partner_ip = textBox3.Text;
                partner_port = Int32.Parse(textBox2.Text);
                client_client.Connect(partner_ip, partner_port);
                List<string> param = new List<string>();
                param.Add("/C");
                param.Add(username);

                Send(JsonSerializer.Serialize(param), client_client);
                string partner_username = Recieve(client_client);
                User new_outgoing_user = new User(client_client, partner_username);
                client_users.Add(new_outgoing_user);

                

                AddTextSafe(listBox1, "povezali ste se na novega uporabnika!");
            }
            catch (Exception eg)
            {
                MessageBox.Show("Prišlo je do napake pri povezovanju na partnerja (najverjetneje partner nima prižganega serverja), ali pa niste vnesli pravilnega porta: \n" + eg.Message + "\n" + eg.StackTrace);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!mine)
            {
                mine = true;
                Thread thread = new Thread(new ThreadStart(Mine));
                thread.Name = "MineCraft";
                thread.Start();
                button5.Text = "Stop mining";
            }
            else
            {
                mine = false;
                button5.Text = "Mine";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketPiirto_receiver
{
    public partial class Form1 : Form
    {
        private static Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        static int count = 1;
        public Form1()
        {
            InitializeComponent();
            Thread t = new Thread(server);
            t.Start();
        }

        private void server()
        {
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();
            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                list_clients.Add(count, client);
                Console.WriteLine("user " + count + " connected");
                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }
        public void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;
            client = list_clients[id];
            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count).TrimEnd('\r', '\n');

                if (data.Contains("exit"))
                {
                    break;
                }
                if (data.Length > 0)
                {
                    Console.WriteLine(data);
                    SetText(data);
                } 
            }
            Console.WriteLine("user " + id + " disconnected");
            list_clients.Remove(id);

            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        delegate void SetTextCallback(string t);
        private void SetText(string t)
        {
            if (this.panel.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, new object[] { t });
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.Red);
                panel.BringToFront();
                Graphics g = panel.CreateGraphics();
                
                int x = int.Parse(t.Split(',')[0].Split('=')[1]);
                int y = int.Parse(t.Split(',')[1].Split('=')[1].Replace("}", ""));
                Console.WriteLine("x: " + x);
                Console.WriteLine("y: " + y);
                g.FillEllipse(brush, x, y, 4, 4);
            }
        }
    }
}

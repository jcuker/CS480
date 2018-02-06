using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

namespace WindowsFormsApplication
{
    public partial class Form1 : Form
    {
        private Socket client;
        private byte[] data = new byte[1024];
        private int size = 1024;
        private static bool blnConnected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(textBox1.Text);
            listBox1.Items.Add(textBox1.Text);
            textBox1.Clear();
            client.BeginSend(message, 0, message.Length, SocketFlags.None,
                   new AsyncCallback(SendData), client);
        }

        void SendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
            remote.BeginReceive(data, 0, size, SocketFlags.None,
                   new AsyncCallback(ReceiveData), remote);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if( !blnConnected)
            {
                textBox2.Text = "Connecting...";
                Socket newsock = new Socket(AddressFamily.InterNetwork,
                           SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
                newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);
            }
        }

        void Connected(IAsyncResult iar)
        {
            client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                blnConnected = true;
                this.Invoke((MethodInvoker)delegate
                {
                    textBox2.Text = "Connected to: " + client.RemoteEndPoint.ToString();
                });

                client.BeginReceive(data, 0, size, SocketFlags.None,
                       new AsyncCallback(ReceiveData), client);
            }
            catch (SocketException)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    textBox2.Text = "Error connecting";
                });
            }
        }

        void ReceiveData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int recv = remote.EndReceive(iar);
            string stringData = Encoding.ASCII.GetString(data, 0, recv);
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add("From Server: " + stringData);
            }); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client.Close();
            this.Invoke((MethodInvoker)delegate
            {
                textBox2.Text = "Disconnected";
            });
        }
    }
}

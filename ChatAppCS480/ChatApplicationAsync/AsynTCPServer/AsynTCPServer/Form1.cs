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

namespace AsynTCPServer
{
    public partial class Form1 : Form
    {
        private Socket client;
        private byte[] data = new byte[1024];
        private int size = 1024;

        public Form1()
        {
            InitializeComponent();
            client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 12345);
            client.Bind(iep);
            client.Listen(5);
            client.BeginAccept(new AsyncCallback(AcceptConn), client);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(textBox1.Text);
            listBox1.Items.Add(textBox1.Text);
            textBox1.Clear();
            client.BeginSend(message, 0, message.Length, SocketFlags.None,
                   new AsyncCallback(SendData), client);
        }
        

        private void button3_Click(object sender, EventArgs e)
        {
            client.Close();
            this.Invoke((MethodInvoker)delegate
            {
                textBox2.Text = "Disconnected";
            });
        }

        void AcceptConn(IAsyncResult iar)
        {
            Socket oldserver = (Socket)iar.AsyncState;
            client = oldserver.EndAccept(iar);
            this.Invoke((MethodInvoker)delegate
            {
                textBox2.Text = "Connected to: " + client.RemoteEndPoint.ToString();
            });
            string stringData = "Welcome to my server";
            byte[] message1 = Encoding.ASCII.GetBytes(stringData);
            client.BeginSend(message1, 0, message1.Length, SocketFlags.None,
                  new AsyncCallback(SendData), client);
        }
        void SendData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
            client.BeginReceive(data, 0, size, SocketFlags.None,
                  new AsyncCallback(ReceiveData), client);
        }
        void ReceiveData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int recv = client.EndReceive(iar);
            if (recv == 0)
            {
                client.Close();
                this.Invoke((MethodInvoker)delegate
                {
                    textBox1.Text = "Waiting for client...";
                });
                this.client.BeginAccept(new AsyncCallback(AcceptConn), this.client);
                return;
            }
            string receivedData = Encoding.ASCII.GetString(data, 0, recv);
            this.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add("From Client: " + receivedData);
            });
            client.BeginReceive(data, 0, size, SocketFlags.None,
                  new AsyncCallback(ReceiveData), client);
        }

    }
}

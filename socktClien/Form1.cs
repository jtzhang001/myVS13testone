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
using System.Net.Sockets;

namespace socktClien
{
    public partial class Form1 : Form
    {

        private Socket m_clientSocket;
        private byte[] m_receiveBuffer = new byte[1024];


        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //txtIP.Text = "172.18.20.234";
            txtIP.Text = "192.168.100.25";
            txtPort.Text = "8899";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // V1
            // V2
            string serverIP = txtIP.Text;
            int serverPort = Int32.Parse(txtPort.Text);
            m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            try
            {
                m_clientSocket.Connect(remoteEndPoint);
                if (m_clientSocket.Connected)
                {
                    m_clientSocket.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallBack), null);
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Client connect server success.");
                }
            }
            catch (Exception)
            {
                this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Client connect server fail.");
                m_clientSocket = null;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // T1
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
                btnSend.Enabled = false;
                this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Client disconnected.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strSendData = txtSend.Text;
            byte[] sendBuffer = new byte[1024];
            sendBuffer = Encoding.Unicode.GetBytes(strSendData);
            if (m_clientSocket != null)
            {
                //if (m_clientSocket.Poll(10, SelectMode.SelectRead))
                //{
                //    MessageBox.Show("链接断开！！");
                //    return;
                //}
                m_clientSocket.Send(sendBuffer);
            }
        }
        //jin.1993


        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                int REnd = m_clientSocket.EndReceive(ar);
                string strReceiveData = Encoding.Unicode.GetString(m_receiveBuffer, 0, REnd);
                this.HandleMessage(strReceiveData);
                m_clientSocket.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, 0, new AsyncCallback(ReceiveCallBack), null);
            }
            catch (Exception ex)
            {
               // throw new Exception(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        private void HandleMessage(string message)
        {
            message = message.Replace("/0", "");
            if (!string.IsNullOrEmpty(message))
            {
                this.AddRunningInfo(">> Receive Data from server:" + message);
            }
        }
        private void AddRunningInfo(string message)
        {

           // this.txtSend.Text += message;
            //lstRunningInfo.BeginUpdate();
            //lstRunningInfo.Items.Insert(0, message);
            //if (lstRunningInfo.Items.Count > 100)
            //{
            //    lstRunningInfo.Items.RemoveAt(100);
            //}
            //lstRunningInfo.EndUpdate();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSend.Text) && m_clientSocket != null)
            {
                btnSend.Enabled = true;
            }
            else
            {
                btnSend.Enabled = false;
            }
        }

    }
}

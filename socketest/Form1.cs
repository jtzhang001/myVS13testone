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
using System.Threading;

namespace socketest
{
    public partial class Form1 : Form
    {


        private Thread m_serverThread;
        private Socket m_serverSocket;
        private string m_serverIP;
        private int m_serverPort;
        public delegate void ReceiveMessageDelegate(Client client);
        ReceiveMessageDelegate receiveMessageDelegate;

        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_serverIP = cmbIP.Text;
            m_serverPort = Int32.Parse(txtPort.Text);
            Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Stop();
        } 

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] addresses = GetLocalAddresses();
            cmbIP.Items.Clear();
            if (addresses.Length > 0)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    cmbIP.Items.Add(addresses[i]);
                }
                cmbIP.Text = (string)cmbIP.Items[0];
            }
            this.txtPort.Text = "8899";
        }





        /// <summary>
        /// 开始服务
        /// </summary>
        private void Start()
        {
            try
            {
                m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(m_serverIP), m_serverPort);
                m_serverSocket.Bind(localEndPoint);
                m_serverSocket.Listen(10);
                m_serverThread = new Thread(new ThreadStart(ReceiveAccept));
                m_serverThread.Start();
                this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Server started.");
            }
            catch (SocketException se)
            {
                throw new Exception(se.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        private void Stop()
        {
            try
            {
                m_serverSocket.Close();
                m_serverThread.Abort(); // 线程终止
                 // Socket Close
                this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Server stoped.");
                this.btnStart.Enabled = true;
                this.btnStop.Enabled = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        private void ReceiveAccept()
        {
            while (true)
            {
                Client client = new Client();
                try
                {
                    client.ClientSocket = m_serverSocket.Accept();
                    this.AddRunningInfo(">> " + DateTime.Now.ToString() + " Client[" + client.ClientSocket.RemoteEndPoint.ToString() + "] connected.");
                    receiveMessageDelegate = new ReceiveMessageDelegate(ReceiveMessages);
                    receiveMessageDelegate.BeginInvoke(client, ReceiveMessagesCallback, "");
                }
                catch (Exception ex)
                {
                  //  throw new Exception(ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void ReceiveMessages(Client client)
        {
            while (true)
            {
                byte[] receiveBuffer = new byte[1024];
                try
                {
                    client.ClientSocket.Receive(receiveBuffer);
                    string strReceiveData = Encoding.Unicode.GetString(receiveBuffer);
                    if (!string.IsNullOrEmpty(strReceiveData))
                    {
                        this.AddRunningInfo(">> Receive data from [" + client.ClientSocket.RemoteEndPoint.ToString() + "]:" + strReceiveData);
                        string strSendData = "OK. The content is:" + strReceiveData;
                        int sendBufferSize = Encoding.Unicode.GetByteCount(strSendData);
                        byte[] sendBuffer = new byte[sendBufferSize];
                        sendBuffer = Encoding.Unicode.GetBytes(strSendData);
                        client.ClientSocket.Send(sendBuffer);
                    }
                }
                catch (SocketException se)
                {
                    //throw se;
                    MessageBox.Show(se.Message);
                    break;
                }
            }
           
        }
        private void ReceiveMessagesCallback(IAsyncResult AR)
        {
            receiveMessageDelegate.EndInvoke(AR);
        }



        /// <summary>
        /// 将运行信息加入显示列表
        /// </summary>
        private void AddRunningInfo(string message)
        {

            this.textBox2.Text += message;
            //lstRunningInfo.BeginUpdate();
            //lstRunningInfo.Items.Insert(0, message);
            //if (lstRunningInfo.Items.Count > 100)
            //{
            //    lstRunningInfo.Items.RemoveAt(100);
            //}
            //lstRunningInfo.EndUpdate();
        }
        /// <summary>
        /// 获取本机地址列表
        /// </summary>
        public string[] GetLocalAddresses()
        {
            // 获取主机名
            string strHostName = Dns.GetHostName();
            // 根据主机名进行查找
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            string[] retval = new string[iphostentry.AddressList.Length];
            int i = 0;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                retval[i] = ipaddress.ToString();
                i++;
            }
            return retval;
        }

      
    }
    /// <summary>
    /// 客户端会话信息类
    /// </summary>
    public class Client
    {
        Socket m_clientSocket;
        public Client() { }
        public Socket ClientSocket
        {
            get { return m_clientSocket; }
            set { this.m_clientSocket = value; }
        }
    }
}
    


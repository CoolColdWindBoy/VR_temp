using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace DesktopServer
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        

        private string BaseIP = "192.168.1.";
        private int StartIP = 1;
        private int StopIP = 255;
        private string ip;
        private int port=4210;
        

        public const int SIO_UDP_CONNRESET = -1744830452;
        UdpClient client = new UdpClient(4210);



        private int timeout = 100;
        private int nFound = 0;

        private string[] ipFound = new string[255];

        static object lockObj = new object();
        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;

        private string [] localIP=new string[255];
        private int localIPcount = 0;

        public class Ips
        {
            public int count;
            public string[] ip= new string[255];
            public int[] status= new int[255];//0 unknown, 1 device
            public string[] deviceName = new string[255];
        }
        private Ips ips=new Ips();

        private void button1_Click(object sender, EventArgs e)
        {

            
        }

        void updateLocalIP() {
            int i = 0;
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    localIP[i] = addr.Address.ToString();
                    i++;
                }
            }
            localIPcount = i;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        public void updateIp(int count, string[] ipS)
        {
            for (int i = 0; i < count; i++)
            {
                bool contain = false;
                for (int j = 0; j < localIPcount; j++)
                {
                    if (ipS[i]==localIP[j])
                    {
                        contain = true;
                    }
                }
                if (contain)
                {
                    //MessageBox.Show(ipS[i]);
                    continue;
                }
                bool found = false;
                for(int j = 0; j < ips.count; j++)
                {
                    if (ips.ip[j] == ipS[i])
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    ips.ip[ips.count] = ipS[i];
                    ips.count++;
                }
            }
            this.Invoke(new MethodInvoker(delegate ()
            {
                textBox1.Text = "";
                for (int i = 0; i < ips.count; i++)
                {
                    textBox1.Text += ips.ip[i] + "\r\n";
                }
            }));
        }

        public async void RunPingSweep_Async()
        {
            ipFound = new string[255];
            nFound = 0;
            var tasks = new List<Task>();
            stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = StartIP; i <= StopIP; i++)
            {
                ip = BaseIP + i.ToString();

                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                var task = PingAndUpdateAsync(p, ip);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ContinueWith(t =>
            {
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                updateIp(nFound, ipFound);
            });
        }

        private async Task PingAndUpdateAsync(System.Net.NetworkInformation.Ping ping, string ip)
        {
            var reply = await ping.SendPingAsync(ip, timeout);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                ipFound[nFound] = ip;
                lock (lockObj)
                {
                    nFound++;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client.Client.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null
            );
            updateLocalIP();
            RunPingSweep_Async();
            try
            {
                client.BeginReceive(new AsyncCallback(DataReceived), null);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.ToString());
            }
        }

        private void DataReceived(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] data = client.EndReceive(res, ref RemoteIpEndPoint);
            client.BeginReceive(new AsyncCallback(DataReceived), null);

            processUDP(RemoteIpEndPoint.Address.ToString(), System.Text.Encoding.UTF8.GetString(data));
        }

        private void processUDP(string ip, string data)
        {
            if (data == "") return;
            if (data == "VR") return;
            //MessageBox.Show(ip + "\r\n" + data);
            if (data[0] != '[') return;
            string type = "";
            string content = "";
            int j = 0;
            for(int i = 1; i < data.Length; i++)
            {
                if (data[i] == ']')
                {
                    j = i+1;
                    break;
                }
                else
                {
                    type+=data[i];
                }
            }
            for(int k=j;k<data.Length; k++)
            {
                content+=data[k];
            }
            if (type == "device")
            {
                bool found = false;
                for(int i = 0; i < ips.count; i++)
                {
                    if (ip == ips.ip[i])
                    {
                        found = true;
                        ips.status[i] = 1;
                        ips.deviceName[i] = content;
                        break;
                    }
                }
                if (!found)
                {
                    ips.ip[ips.count]=ip;
                    ips.status[ips.count] = 1;
                    ips.deviceName[ips.count] = content;
                    ips.count++;
                }
            }
            else if(type == "Rssi")
            {
                for(int i = 0; i < ips.count; i++)
                {
                    if (ip == ips.ip[i])
                    {
                        int Rssi= Int32.Parse(content);
                        if (Rssi < 0 && Rssi > -70)
                        {
                            Rssi = 20;
                        }
                        else if (Rssi < -90)
                        {
                            Rssi = 0;
                        }
                        else if (Rssi == 0)
                        {
                            Rssi = 0;
                        }
                        else
                        {
                            Rssi += 90;
                        }
                        if (Rssi < 0)
                        {
                            Rssi = 0;
                        }else if (Rssi > 20)
                        {
                            Rssi = 20;
                        }

                        string deviceName=ips.deviceName[i];
                        if (deviceName == "controllerLeft")
                        {
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                progressBarControllerLeft.Value = Rssi;
                            }));
                        }
                        

                        break;
                    }
                }
            }
                
        }

        

        private void timerDetect_Tick(object sender, EventArgs e)
        {
            for(int i = 0; i < ips.count; i++)
            {
                if (ips.status[i] == 0)
                {
                    try
                    {
                        client.Send(Encoding.ASCII.GetBytes("VR"), Encoding.ASCII.GetBytes("VR").Length, ips.ip[i], port);
                        //Console.WriteLine(ips.ip[i]);
                    }
                    catch(Exception E)
                    {
                        Console.WriteLine(E.ToString());
                    }
                }
            }
        }

        private void timerPing_Tick(object sender, EventArgs e)
        {
            RunPingSweep_Async();
        }

        private void timerDetectLocalIP_Tick(object sender, EventArgs e)
        {
            updateLocalIP();
        }
    }

}
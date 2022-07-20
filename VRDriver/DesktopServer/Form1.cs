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




        private int StartIP = 1;
        private int StopIP = 255;
        private string ip;
        private int port = 4210;


        public const int SIO_UDP_CONNRESET = -1744830452;
        UdpClient client = new UdpClient(4210);



        private int timeout = 100;
        private int nFound = 0;

        private string[] ipFound = new string[255];

        static object lockObj = new object();
        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;

        private string[] localIP = new string[255];
        private int localIPcount = 0;

        public class Ips
        {
            public int count;
            public string[] ip = new string[255];
            public int[] status = new int[255];//0 unknown or disappeared, 1 active device
            public string[] deviceName = new string[255];
            public int[] pitch = new int[255];
            public int[] yaw = new int[255];
            //calibration: at T-pose, pitch and yaw should be 0. 
            //calibration can be done through matrix transformation. 

        }
        
        public class Coordinate
        {
            public float x;
            public float y;
            public float z;
            public int u;
            public int v;
            public int w;
            //xyz are positions in meters, uvw are angles in deg. 
            public int pitch;
            public int yaw;
        }

        public class Skeleton
        {
            public int id=0;
            public string name="";
            public string discription="";
            // unit of skeletons should be in meters. 
            public float headL=0.15F;
            public float neckL=0.1F;
            public float shoulderL=0.2F;
            public float elbowL=0.35F;
            public float wristL=0.3F;
            public float handL=0.1F;
            public float backL=0.3F;
            public float waistL=0.25F;
            public float hipL=0.15F;
            public float kneeL=0.25F;
            public float ankleL=0.45F;
            public float feetL=0.15F;
            //coordinates are for points, but they are also sensors on segaments between point.
            public Coordinate head=new Coordinate();
            public Coordinate neck = new Coordinate();
            public Coordinate shoulder = new Coordinate();
            public Coordinate shoulderLeft = new Coordinate();
            public Coordinate shoulderRight = new Coordinate();
            public Coordinate elbowLeft = new Coordinate();
            public Coordinate elbowRight = new Coordinate();
            public Coordinate wristLeft = new Coordinate();
            public Coordinate wristRight = new Coordinate();
            public Coordinate handLeft = new Coordinate();
            public Coordinate handRight = new Coordinate();
            public Coordinate back = new Coordinate();
            public Coordinate hip= new Coordinate();
            public Coordinate hipLeft = new Coordinate();
            public Coordinate hipRight = new Coordinate();
            public Coordinate kneeLeft=new Coordinate();
            public Coordinate kneeRight = new Coordinate();
            public Coordinate ankleLeft = new Coordinate();
            public Coordinate ankleRight = new Coordinate();
            public Coordinate feetLeft = new Coordinate();
            public Coordinate feetRight = new Coordinate();

        }
        Skeleton Monika=new Skeleton();

        private Ips ips = new Ips();

        private void button1_Click(object sender, EventArgs e)
        {
            Monika.hip.pitch = 70;
            Monika.hip.yaw = 15;
            Monika.shoulder.pitch = 85;
            Monika.shoulder.yaw= 15;
            Monika.shoulderLeft.pitch = -15;
            Monika.shoulderRight.pitch = 0;
            Monika.shoulderLeft.yaw = -75;
            Monika.shoulderRight.yaw = 90;
            Monika.elbowLeft.pitch = 40;
            Monika.elbowRight.pitch = 50;
            Monika.elbowLeft.yaw = -90;
            Monika.elbowRight.yaw = 60;
            Monika.wristLeft.pitch = -70;
            Monika.wristRight.pitch = -60;
            Monika.wristLeft.yaw = -75;
            Monika.wristRight.yaw = 80;
            Monika.handLeft.yaw = -90;
            Monika.handRight.yaw = 75;



            Monika.kneeLeft.pitch = 70;
            Monika.kneeLeft.yaw = -30;
            Monika.kneeRight.pitch = 70;
            Monika.kneeRight.yaw = 30;
            Monika.ankleLeft.pitch = 80;
            Monika.ankleLeft.yaw = 0;
            Monika.ankleRight.pitch = 80;
            Monika.ankleRight.yaw = 0;
            Monika.feetRight.yaw = 0;
            Monika.feetLeft.yaw = 0;

            Monika.head.pitch = 70;
            Monika.head.yaw = 0;
            

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
            updateSkeleton();
        }

        public void updateIp(int count, string[] ipS)
        {
            for (int i = 0; i < count; i++)
            {
                bool contain = false;
                for (int j = 0; j < localIPcount; j++)
                {
                    if (ipS[i] == localIP[j])
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
                for (int j = 0; j < ips.count; j++)
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
            if(ips.count>0)
            this.Invoke(new MethodInvoker(delegate ()
            {
                textBox1.Text = "";
                for (int i = 0; i < ips.count; i++)
                {
                    textBox1.Text += ips.ip[i] + "\r\n";
                }
            }));
        }

        public async void RunPingSweep_Async(string BaseIP)
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
            RunPingSweep_Async("192.168.1.");
            RunPingSweep_Async("192.168.137.");
            try
            {
                client.BeginReceive(new AsyncCallback(DataReceived), null);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.ToString());
            }
            Monika.head.pitch = 90;
            Monika.shoulder.pitch = 90;
            Monika.elbowLeft.yaw = -90;
            Monika.elbowRight.yaw = 90;
            Monika.wristLeft.yaw = -90;
            Monika.wristRight.yaw = 90;
            Monika.handLeft.yaw = -90;
            Monika.handRight.yaw = 90;
            Monika.hip.pitch = 90;
            Monika.kneeLeft.pitch = 90;
            Monika.kneeRight.pitch = 90;
            Monika.ankleLeft.pitch = 90;
            Monika.ankleRight.pitch = 90;
            Monika.shoulderLeft.yaw = -90;
            Monika.shoulderRight.yaw = 90;

            


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
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] == ']')
                {
                    j = i + 1;
                    break;
                }
                else
                {
                    type += data[i];
                }
            }
            for (int k = j; k < data.Length; k++)
            {
                content += data[k];
            }
            if (type == "device")
            {
                bool found = false;
                for (int i = 0; i < ips.count; i++)
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
                    ips.ip[ips.count] = ip;
                    ips.status[ips.count] = 1;
                    ips.deviceName[ips.count] = content;
                    ips.count++;
                }
            }
            else if (type == "Rssi")
            {
                for (int i = 0; i < ips.count; i++)
                {
                    if (ip == ips.ip[i])
                    {
                        int Rssi = Int32.Parse(content);
                        if (Rssi < 0 && Rssi > -50)
                        {
                            Rssi = 40;
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
                        } else if (Rssi > 40)
                        {
                            Rssi = 40;
                        }

                        string deviceName = ips.deviceName[i];
                        if (deviceName == "controllerLeft")//change this
                        {
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                progressBarControllerLeft.Value = Rssi;
                            }));
                        }else if(deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }
                        else if (deviceName == "controllerLeft")
                        {

                        }


                        break;
                    }
                }
            } else if (type == "mpu")
            {
                string[] datas = new string[2];
                datas = content.Split(',');
                this.Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < ips.count; i++)
                    {
                        if (ips.ip[i] == ip)
                        {
                            if (ips.deviceName[i] == "controllerLeft")
                            {
                                Monika.handLeft.pitch = Int32.Parse(datas[0]);
                                Monika.handLeft.yaw = Int32.Parse(datas[1]);
                            }else if(ips.deviceName[i] == "controllerRight")
                            {
                                Monika.handRight.pitch = Int32.Parse(datas[0]);
                                Monika.handRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "head")
                            {
                                Monika.head.pitch = Int32.Parse(datas[0]);
                                Monika.head.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "shoulderLeft")
                            {
                                Monika.shoulderLeft.pitch = Int32.Parse(datas[0]);
                                Monika.shoulderLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "shoulderRight")
                            {
                                Monika.shoulderRight.pitch = Int32.Parse(datas[0]);
                                Monika.shoulderRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "elbowLeft")
                            {
                                Monika.elbowLeft.pitch = Int32.Parse(datas[0]);
                                Monika.elbowLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "elbowRight")
                            {
                                Monika.elbowRight.pitch = Int32.Parse(datas[0]);
                                Monika.elbowRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "wristLeft")
                            {
                                Monika.wristLeft.pitch = Int32.Parse(datas[0]);
                                Monika.wristLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "wristRight")
                            {
                                Monika.wristRight.pitch = Int32.Parse(datas[0]);
                                Monika.wristRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "shoulder")
                            {
                                Monika.shoulder.pitch = Int32.Parse(datas[0]);
                                Monika.shoulder.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "hip")
                            {
                                Monika.hip.pitch = Int32.Parse(datas[0]);
                                Monika.hip.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "kneeLeft")
                            {
                                Monika.kneeLeft.pitch = Int32.Parse(datas[0]);
                                Monika.kneeLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "kneeRight")
                            {
                                Monika.kneeRight.pitch = Int32.Parse(datas[0]);
                                Monika.kneeRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "ankleLeft")
                            {
                                Monika.ankleLeft.pitch = Int32.Parse(datas[0]);
                                Monika.ankleLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "ankleRight")
                            {
                                Monika.ankleRight.pitch = Int32.Parse(datas[0]);
                                Monika.ankleRight.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "feetLeft")
                            {
                                Monika.feetLeft.pitch = Int32.Parse(datas[0]);
                                Monika.feetLeft.yaw = Int32.Parse(datas[1]);
                            }
                            else if (ips.deviceName[i] == "feetRight")
                            {
                                Monika.feetRight.pitch = Int32.Parse(datas[0]);
                                Monika.feetRight.yaw = Int32.Parse(datas[1]);
                            }





                            break;
                        }
                    }
                }));
            }

        }



        private void timerDetect_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < ips.count; i++)
            {
                if (ips.status[i] == 0)
                {
                    try
                    {
                        client.Send(Encoding.ASCII.GetBytes("VR"), Encoding.ASCII.GetBytes("VR").Length, ips.ip[i], port);
                        //Console.WriteLine(ips.ip[i]);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.ToString());
                    }
                }
            }
        }

        private void timerPing_Tick(object sender, EventArgs e)
        {
            RunPingSweep_Async("192.168.1.");
            RunPingSweep_Async("172.20.10");
        }

        private void timerDetectLocalIP_Tick(object sender, EventArgs e)
        {
            updateLocalIP();
        }

        private void updateSkeleton()
        {
            Monika.hip.v = Monika.hip.pitch;
            Monika.hip.w = Monika.hip.yaw;
            Monika.hip.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.hip.v) * Monika.waistL;
            Monika.hip.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.hip.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.hip.v) * Monika.waistL;
            Monika.hip.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.hip.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.hip.v) * Monika.waistL;
            Monika.hipLeft.z = Monika.hip.z;
            Monika.hipRight.z = Monika.hip.z;
            Monika.hipRight.w = Monika.hip.w + 90;
            Monika.hipLeft.w = Monika.hip.w - 90;
            Monika.hipLeft.x = Monika.hip.x + (float)Math.Cos(3.14159265358979 / 180 * Monika.hipLeft.w) * Monika.hipL;
            Monika.hipLeft.y = Monika.hip.y + (float)Math.Sin(3.14159265358979 / 180 * Monika.hipLeft.w) * Monika.hipL;
            Monika.hipRight.x = Monika.hip.x + (float)Math.Cos(3.14159265358979 / 180 * Monika.hipRight.w) * Monika.hipL;
            Monika.hipRight.y = Monika.hip.y + (float)Math.Sin(3.14159265358979 / 180 * Monika.hipRight.w) * Monika.hipL;
            Monika.kneeLeft.w = Monika.kneeLeft.yaw;
            Monika.kneeRight.w = Monika.kneeRight.yaw;
            Monika.kneeLeft.v = Monika.kneeLeft.pitch;
            Monika.kneeRight.v = Monika.kneeRight.pitch;
            Monika.kneeLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.kneeLeft.v) * Monika.kneeL + Monika.hipLeft.z;
            Monika.kneeRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.kneeRight.v) * Monika.kneeL + Monika.hipRight.z;
            Monika.kneeLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeLeft.v) * Monika.kneeL + Monika.hipLeft.x;
            Monika.kneeRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeRight.v) * Monika.kneeL + Monika.hipRight.x;
            Monika.kneeLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.kneeLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeLeft.v) * Monika.kneeL + Monika.hipLeft.y;
            Monika.kneeRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.kneeRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.kneeRight.v) * Monika.kneeL + Monika.hipRight.y;
            Monika.ankleLeft.w = Monika.ankleLeft.yaw;
            Monika.ankleLeft.v = Monika.ankleLeft.pitch;
            Monika.ankleRight.w = Monika.ankleRight.yaw;
            Monika.ankleRight.v = Monika.ankleRight.pitch;
            Monika.ankleLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.ankleLeft.v) * Monika.ankleL + Monika.kneeLeft.z;
            Monika.ankleRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.ankleRight.v) * Monika.ankleL + Monika.kneeRight.z;
            Monika.ankleLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleLeft.v) * Monika.ankleL + Monika.kneeLeft.x;
            Monika.ankleRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleRight.v) * Monika.ankleL + Monika.kneeRight.x;
            Monika.ankleLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.ankleLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleLeft.v) * Monika.ankleL + Monika.kneeLeft.y;
            Monika.ankleRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.ankleRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.ankleRight.v) * Monika.ankleL + Monika.kneeRight.y;
            Monika.feetLeft.w = Monika.feetLeft.yaw;
            Monika.feetLeft.v = Monika.feetLeft.pitch;
            Monika.feetRight.w = Monika.feetRight.yaw;
            Monika.feetRight.v = Monika.feetRight.pitch;
            Monika.feetLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.feetLeft.v) * Monika.feetL + Monika.ankleLeft.z;
            Monika.feetRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.feetRight.v) * Monika.feetL + Monika.ankleRight.z;
            Monika.feetLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.feetLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.feetLeft.v) * Monika.feetL + Monika.ankleLeft.x;
            Monika.feetRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.feetRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.feetRight.v) * Monika.feetL + Monika.ankleRight.x;
            Monika.feetLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.feetLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.feetLeft.v) * Monika.feetL + Monika.ankleLeft.y;
            Monika.feetRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.feetRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.feetRight.v) * Monika.feetL + Monika.ankleRight.y;
            Monika.shoulder.w = Monika.shoulder.yaw;
            Monika.shoulder.v = Monika.shoulder.pitch;
            Monika.shoulder.z = -(float)Math.Sin(3.14159265358979 / 180 * Monika.shoulder.v) * Monika.backL;
            Monika.shoulder.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulder.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulder.v) * Monika.shoulderL;
            Monika.shoulder.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.shoulder.v) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulder.v) * Monika.shoulderL;
            Monika.shoulderLeft.w = Monika.shoulderLeft.yaw;
            Monika.shoulderRight.w = Monika.shoulderRight.yaw;
            Monika.shoulderLeft.v = Monika.shoulderLeft.pitch;
            Monika.shoulderRight.v = Monika.shoulderRight.pitch;
            Monika.shoulderLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.shoulderLeft.v) * Monika.shoulderL + Monika.shoulder.z;
            Monika.shoulderRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.shoulderRight.v) * Monika.shoulderL + Monika.shoulder.z;
            Monika.shoulderLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderLeft.v) * Monika.shoulderL + Monika.shoulder.x;
            Monika.shoulderRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderRight.v) * Monika.shoulderL + Monika.shoulder.x;
            Monika.shoulderLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.shoulderLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderLeft.v) * Monika.shoulderL + Monika.shoulder.y;
            Monika.shoulderRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.shoulderRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.shoulderRight.v) * Monika.shoulderL + Monika.shoulder.y;
            Monika.elbowLeft.w = Monika.elbowLeft.yaw;
            Monika.elbowLeft.v = Monika.elbowLeft.pitch;
            Monika.elbowRight.w = Monika.elbowRight.yaw;
            Monika.elbowRight.v = Monika.elbowRight.pitch;
            Monika.elbowLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.elbowLeft.v) * Monika.elbowL + Monika.shoulderLeft.z;
            Monika.elbowRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.elbowRight.v) * Monika.elbowL + Monika.shoulderRight.z;
            Monika.elbowLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowLeft.v) * Monika.elbowL + Monika.shoulderLeft.x;
            Monika.elbowRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowRight.v) * Monika.elbowL + Monika.shoulderRight.x;
            Monika.elbowLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.elbowLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowLeft.v) * Monika.elbowL + Monika.shoulderLeft.y;
            Monika.elbowRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.elbowRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.elbowRight.v) * Monika.elbowL + Monika.shoulderRight.y;
            Monika.wristLeft.w = Monika.wristLeft.yaw;
            Monika.wristLeft.v = Monika.wristLeft.pitch;
            Monika.wristRight.w = Monika.wristRight.yaw;
            Monika.wristRight.v = Monika.wristRight.pitch;
            Monika.wristLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.wristLeft.v) * Monika.wristL + Monika.elbowLeft.z;
            Monika.wristRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.wristRight.v) * Monika.wristL + Monika.elbowRight.z;
            Monika.wristLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.wristLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.wristLeft.v) * Monika.wristL + Monika.elbowLeft.x;
            Monika.wristRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.wristRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.wristRight.v) * Monika.wristL + Monika.elbowRight.x;
            Monika.wristLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.wristLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.wristLeft.v) * Monika.wristL + Monika.elbowLeft.y;
            Monika.wristRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.wristRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.wristRight.v) * Monika.wristL + Monika.elbowRight.y;
            Monika.handLeft.w = Monika.handLeft.yaw;
            Monika.handLeft.v = Monika.handLeft.pitch;
            Monika.handRight.w = Monika.handRight.yaw;
            Monika.handRight.v = Monika.handRight.pitch;
            Monika.handLeft.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.handLeft.v) * Monika.handL + Monika.wristLeft.z;
            Monika.handRight.z = (float)Math.Sin(3.14159265358979 / 180 * Monika.handRight.v) * Monika.handL + Monika.wristRight.z;
            Monika.handLeft.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.handLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.handLeft.v) * Monika.handL + Monika.wristLeft.x;
            Monika.handRight.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.handRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.handRight.v) * Monika.handL + Monika.wristRight.x;
            Monika.handLeft.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.handLeft.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.handLeft.v) * Monika.handL + Monika.wristLeft.y;
            Monika.handRight.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.handRight.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.handRight.v) * Monika.handL + Monika.wristRight.y;
            Monika.neck.v = Monika.shoulder.v;
            Monika.neck.w = Monika.shoulder.w;
            Monika.neck.z = -(float)Math.Sin(3.14159265358979 / 180 * Monika.neck.v) * Monika.neckL + Monika.shoulder.z;
            Monika.neck.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.neck.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.neck.v) * Monika.neckL + Monika.shoulder.x;
            Monika.neck.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.neck.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.neck.v) * Monika.neckL + Monika.shoulder.y;
            Monika.head.v = Monika.head.pitch;
            Monika.head.w = Monika.head.yaw;
            Monika.head.z = -(float)Math.Sin(3.14159265358979 / 180 * Monika.head.v) * Monika.headL + Monika.neck.z;
            Monika.head.x = (float)Math.Cos(3.14159265358979 / 180 * Monika.head.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.head.v) * Monika.headL + Monika.neck.x;
            Monika.head.y = (float)Math.Sin(3.14159265358979 / 180 * Monika.head.w) * (float)Math.Cos(3.14159265358979 / 180 * Monika.head.v) * Monika.headL + Monika.neck.y;




            Graphics graphics=pictureBox1.CreateGraphics();
            graphics.Clear(pictureBox1.BackColor);
            Pen normal = new Pen(Brushes.White);
            Pen flat= new Pen(Brushes.Gray);
            normal.Width = 4;
            flat.Width = normal.Width;
            float scale = 1.4142135F / 4;
            


            graphics.DrawLine(flat, new Point((int)(Monika.back.y*200+200), (int)(Monika.back.z*200+200)),new Point((int)(Monika.hip.y*200+200),(int)(Monika.hip.z*200+200)));
            graphics.DrawLine(flat, new Point((int)(Monika.hip.y * 200 + 200), (int)(Monika.hip.z * 200 + 200)), new Point((int)(Monika.hipLeft.y * 200 + 200), (int)(Monika.hipLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.hip.y * 200 + 200), (int)(Monika.hip.z * 200 + 200)), new Point((int)(Monika.hipRight.y * 200 + 200), (int)(Monika.hipRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.hipLeft.y * 200 + 200), (int)(Monika.hipLeft.z * 200 + 200)), new Point((int)(Monika.kneeLeft.y * 200 + 200), (int)(Monika.kneeLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.hipRight.y * 200 + 200), (int)(Monika.hipRight.z * 200 + 200)), new Point((int)(Monika.kneeRight.y * 200 + 200), (int)(Monika.kneeRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.kneeLeft.y * 200 + 200), (int)(Monika.kneeLeft.z * 200 + 200)), new Point((int)(Monika.ankleLeft.y * 200 + 200), (int)(Monika.ankleLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.kneeRight.y * 200 + 200), (int)(Monika.kneeRight.z * 200 + 200)), new Point((int)(Monika.ankleRight.y * 200 + 200), (int)(Monika.ankleRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.ankleLeft.y * 200 + 200), (int)(Monika.ankleLeft.z * 200 + 200)), new Point((int)(Monika.feetLeft.y * 200 + 200), (int)(Monika.feetLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.ankleRight.y * 200 + 200), (int)(Monika.ankleRight.z * 200 + 200)), new Point((int)(Monika.feetRight.y * 200 + 200), (int)(Monika.feetRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.back.y * 200 + 200), (int)(Monika.back.z * 200 + 200)), new Point((int)(Monika.shoulder.y * 200 + 200), (int)(Monika.shoulder.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.shoulder.y * 200 + 200), (int)(Monika.shoulder.z * 200 + 200)), new Point((int)(Monika.shoulderLeft.y * 200 + 200), (int)(Monika.shoulderLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.shoulder.y * 200 + 200), (int)(Monika.shoulder.z * 200 + 200)), new Point((int)(Monika.shoulderRight.y * 200 + 200), (int)(Monika.shoulderRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.shoulderLeft.y * 200 + 200), (int)(Monika.shoulderLeft.z * 200 + 200)), new Point((int)(Monika.elbowLeft.y * 200 + 200), (int)(Monika.elbowLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.shoulderRight.y * 200 + 200), (int)(Monika.shoulderRight.z * 200 + 200)), new Point((int)(Monika.elbowRight.y * 200 + 200), (int)(Monika.elbowRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.elbowLeft.y * 200 + 200), (int)(Monika.elbowLeft.z * 200 + 200)), new Point((int)(Monika.wristLeft.y * 200 + 200), (int)(Monika.wristLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.elbowRight.y * 200 + 200), (int)(Monika.elbowRight.z * 200 + 200)), new Point((int)(Monika.wristRight.y * 200 + 200), (int)(Monika.wristRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.wristLeft.y * 200 + 200), (int)(Monika.wristLeft.z * 200 + 200)), new Point((int)(Monika.handLeft.y * 200 + 200), (int)(Monika.handLeft.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.wristRight.y * 200 + 200), (int)(Monika.wristRight.z * 200 + 200)), new Point((int)(Monika.handRight.y * 200 + 200), (int)(Monika.handRight.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.shoulder.y * 200 + 200), (int)(Monika.shoulder.z * 200 + 200)), new Point((int)(Monika.neck.y * 200 + 200), (int)(Monika.neck.z * 200 + 200)));
            graphics.DrawLine(flat, new Point((int)(Monika.neck.y * 200 + 200), (int)(Monika.neck.z * 200 + 200)), new Point((int)(Monika.head.y * 200 + 200), (int)(Monika.head.z * 200 + 200)));

            graphics.DrawLine(normal, new Point((int)(Monika.back.y * 200 + 200 + Monika.back.x * scale * 200), (int)(Monika.back.z * 200 + 200 - Monika.back.x * scale * 200)), new Point((int)(Monika.hip.y * 200 + 200 + Monika.hip.x * scale * 200), (int)(Monika.hip.z * 200 + 200 - Monika.hip.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.hip.y * 200 + 200 + Monika.hip.x * scale * 200), (int)(Monika.hip.z * 200 + 200 - Monika.hip.x * scale * 200)), new Point((int)(Monika.hipLeft.y * 200 + 200 + Monika.hipLeft.x * scale * 200), (int)(Monika.hipLeft.z * 200 + 200 - Monika.hipLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.hip.y * 200 + 200 + Monika.hip.x * scale * 200), (int)(Monika.hip.z * 200 + 200 - Monika.hip.x * scale * 200)), new Point((int)(Monika.hipRight.y * 200 + 200 + Monika.hipRight.x * 200 * scale), (int)(Monika.hipRight.z * 200 + 200 - Monika.hipRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.hipLeft.y * 200 + 200 + Monika.hipLeft.x * scale * 200), (int)(Monika.hipLeft.z * 200 + 200 - Monika.hipLeft.x * scale * 200)), new Point((int)(Monika.kneeLeft.y * 200 + 200 + Monika.kneeLeft.x * scale * 200), (int)(Monika.kneeLeft.z * 200 + 200 - Monika.kneeLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.hipRight.y * 200 + 200 + Monika.hipRight.x * 200 * scale), (int)(Monika.hipRight.z * 200 + 200 - Monika.hipRight.x * scale * 200)), new Point((int)(Monika.kneeRight.y * 200 + 200 + Monika.kneeRight.x * scale * 200), (int)(Monika.kneeRight.z * 200 + 200 - Monika.kneeRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.kneeLeft.y * 200 + 200 + Monika.kneeLeft.x * scale * 200), (int)(Monika.kneeLeft.z * 200 + 200 - Monika.kneeLeft.x * scale * 200)), new Point((int)(Monika.ankleLeft.y * 200 + 200 + Monika.ankleLeft.x * scale * 200), (int)(Monika.ankleLeft.z * 200 + 200 - Monika.ankleLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.kneeRight.y * 200 + 200 + Monika.kneeRight.x * scale * 200), (int)(Monika.kneeRight.z * 200 + 200 - Monika.kneeRight.x * scale * 200)), new Point((int)(Monika.ankleRight.y * 200 + 200 + Monika.ankleRight.x * scale * 200), (int)(Monika.ankleRight.z * 200 + 200 - Monika.ankleRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.ankleLeft.y * 200 + 200 + Monika.ankleLeft.x * scale * 200), (int)(Monika.ankleLeft.z * 200 + 200 - Monika.ankleLeft.x * scale * 200)), new Point((int)(Monika.feetLeft.y * 200 + 200 + Monika.feetLeft.x * scale * 200), (int)(Monika.feetLeft.z * 200 + 200 - Monika.feetRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.ankleRight.y * 200 + 200 + Monika.ankleRight.x * scale * 200), (int)(Monika.ankleRight.z * 200 + 200 - Monika.ankleRight.x * scale * 200)), new Point((int)(Monika.feetRight.y * 200 + 200 + Monika.feetRight.x * scale * 200), (int)(Monika.feetRight.z * 200 + 200 - Monika.feetRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.back.y * 200 + 200 + Monika.back.x * scale * 200), (int)(Monika.back.z * 200 + 200 - Monika.back.x * scale * 200)), new Point((int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200)), new Point((int)(Monika.shoulderLeft.y * 200 + 200 + Monika.shoulderLeft.x * scale * 200), (int)(Monika.shoulderLeft.z * 200 + 200 - Monika.shoulderLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200)), new Point((int)(Monika.shoulderRight.y * 200 + 200 + Monika.shoulderRight.x * scale * 200), (int)(Monika.shoulderRight.z * 200 + 200 - Monika.shoulderRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.shoulderLeft.y * 200 + 200 + Monika.shoulderLeft.x * scale * 200), (int)(Monika.shoulderLeft.z * 200 + 200 - Monika.shoulderLeft.x * scale * 200)), new Point((int)(Monika.elbowLeft.y * 200 + 200 + Monika.elbowLeft.x * scale * 200), (int)(Monika.elbowLeft.z * 200 + 200 - Monika.elbowLeft.x * scale * 200))); 
            graphics.DrawLine(normal, new Point((int)(Monika.shoulderRight.y * 200 + 200 + Monika.shoulderRight.x * scale * 200), (int)(Monika.shoulderRight.z * 200 + 200 - Monika.shoulderRight.x * scale * 200)), new Point((int)(Monika.elbowRight.y * 200 + 200 + Monika.elbowRight.x * scale * 200), (int)(Monika.elbowRight.z * 200 + 200 - Monika.elbowRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.elbowLeft.y * 200 + 200 + Monika.elbowLeft.x * scale * 200), (int)(Monika.elbowLeft.z * 200 + 200 - Monika.elbowLeft.x * scale * 200)), new Point((int)(Monika.wristLeft.y * 200 + 200 + Monika.wristLeft.x * scale * 200), (int)(Monika.wristLeft.z * 200 + 200 - Monika.wristLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.elbowRight.y * 200 + 200 + Monika.elbowRight.x * scale * 200), (int)(Monika.elbowRight.z * 200 + 200 - Monika.elbowRight.x * scale * 200)), new Point((int)(Monika.wristRight.y * 200 + 200 + Monika.wristRight.x * scale * 200), (int)(Monika.wristRight.z * 200 + 200 - Monika.wristRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.wristLeft.y * 200 + 200 + Monika.wristLeft.x * scale * 200), (int)(Monika.wristLeft.z * 200 + 200 - Monika.wristLeft.x * scale * 200)), new Point((int)(Monika.handLeft.y * 200 + 200 + Monika.handLeft.x * scale * 200), (int)(Monika.handLeft.z * 200 + 200 - Monika.handLeft.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.wristRight.y * 200 + 200 + Monika.wristRight.x * scale * 200), (int)(Monika.wristRight.z * 200 + 200 - Monika.wristRight.x * scale * 200)), new Point((int)(Monika.handRight.y * 200 + 200 + Monika.handRight.x * scale * 200), (int)(Monika.handRight.z * 200 + 200 - Monika.handRight.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200)), new Point((int)(Monika.neck.y * 200 + 200 + Monika.neck.x * scale * 200), (int)(Monika.neck.z * 200 + 200 - Monika.neck.x * scale * 200)));
            graphics.DrawLine(normal, new Point((int)(Monika.neck.y * 200 + 200 + Monika.neck.x * scale * 200), (int)(Monika.neck.z * 200 + 200 - Monika.neck.x * scale * 200)), new Point((int)(Monika.head.y * 200 + 200 + Monika.head.x * scale * 200), (int)(Monika.head.z * 200 + 200 - Monika.head.x * scale * 200)));

            int radius = 6;
            Brush c = Brushes.DimGray;
            graphics.FillCircle(c, (int)(Monika.back.y * 200 + 200 + Monika.back.x * scale * 200), (int)(Monika.back.z * 200 + 200 - Monika.back.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.hip.y * 200 + 200 + Monika.hip.x * scale * 200), (int)(Monika.hip.z * 200 + 200 - Monika.hip.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.hip.y * 200 + 200 + Monika.hip.x * scale * 200), (int)(Monika.hip.z * 200 + 200 - Monika.hip.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.hipLeft.y * 200 + 200 + Monika.hipLeft.x * scale * 200), (int)(Monika.hipLeft.z * 200 + 200 - Monika.hipLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.hipRight.y * 200 + 200 + Monika.hipRight.x * 200 * scale), (int)(Monika.hipRight.z * 200 + 200 - Monika.hipRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.kneeLeft.y * 200 + 200 + Monika.kneeLeft.x * scale * 200), (int)(Monika.kneeLeft.z * 200 + 200 - Monika.kneeLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.kneeRight.y * 200 + 200 + Monika.kneeRight.x * scale * 200), (int)(Monika.kneeRight.z * 200 + 200 - Monika.kneeRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.ankleLeft.y * 200 + 200 + Monika.ankleLeft.x * scale * 200), (int)(Monika.ankleLeft.z * 200 + 200 - Monika.ankleLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.ankleRight.y * 200 + 200 + Monika.ankleRight.x * scale * 200), (int)(Monika.ankleRight.z * 200 + 200 - Monika.ankleRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.back.y * 200 + 200 + Monika.back.x * scale * 200), (int)(Monika.back.z * 200 + 200 - Monika.back.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.shoulderLeft.y * 200 + 200 + Monika.shoulderLeft.x * scale * 200), (int)(Monika.shoulderLeft.z * 200 + 200 - Monika.shoulderLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.shoulderRight.y * 200 + 200 + Monika.shoulderRight.x * scale * 200), (int)(Monika.shoulderRight.z * 200 + 200 - Monika.shoulderRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.elbowLeft.y * 200 + 200 + Monika.elbowLeft.x * scale * 200), (int)(Monika.elbowLeft.z * 200 + 200 - Monika.elbowLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.elbowRight.y * 200 + 200 + Monika.elbowRight.x * scale * 200), (int)(Monika.elbowRight.z * 200 + 200 - Monika.elbowRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.wristLeft.y * 200 + 200 + Monika.wristLeft.x * scale * 200), (int)(Monika.wristLeft.z * 200 + 200 - Monika.wristLeft.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.wristRight.y * 200 + 200 + Monika.wristRight.x * scale * 200), (int)(Monika.wristRight.z * 200 + 200 - Monika.wristRight.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.shoulder.y * 200 + 200 + Monika.shoulder.x * scale * 200), (int)(Monika.shoulder.z * 200 + 200 - Monika.shoulder.x * scale * 200), radius);
            graphics.FillCircle(c, (int)(Monika.neck.y * 200 + 200 + Monika.neck.x * scale * 200), (int)(Monika.neck.z * 200 + 200 - Monika.neck.x * scale * 200), radius);


            Pen x = new Pen(Color.Red);
            Pen y = new Pen(Color.Green);
            Pen z = new Pen(Color.Blue);
            int length = 40;
            x.Width = 4;
            y.Width = 4;
            z.Width = 4;
            graphics.DrawLine(x, new Point(200, 200), new Point(200 + (int)(length * scale),200 - (int)(length * scale)));
            graphics.DrawLine(y, new Point(200, 200), new Point(200 - length, 200));
            graphics.DrawLine(z, new Point(200,200), new Point(200, 200-length));
            

        }

        






    }
    public static class GraphicsExtensions
    {
        public static void DrawCircle(this Graphics g, Pen pen, float centerX, float centerY, float radius)
        {
            g.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }

        public static void FillCircle(this Graphics g, Brush brush, float centerX, float centerY, float radius)
        {
            g.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
    }

}
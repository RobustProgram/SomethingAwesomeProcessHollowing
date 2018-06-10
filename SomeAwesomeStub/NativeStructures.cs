using System;
using System.Runtime.InteropServices;

namespace SomeAwesomeStub
{
    public static class CONSTANTS
    {
        public const int IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 15;
    }

    /*
     * Native Process Environment Block
     */
    [StructLayout(LayoutKind.Sequential)]
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

    /*
     * The DOS Header Block
     */
    [StructLayout(LayoutKind.Sequential)]
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

    [StructLayout(LayoutKind.Sequential)]
    struct ImageDataDirectory
    {
        int VirtualAddress;
        int Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ImageFileHeader
    {
        public short Machine;
        public short NumberOfSections;
        public int TimeDateStamp;
        public int PointerToSymbolTable;
        public int NumberOfSymbols;
        public short SizeOfOptionalHeader;
        public short Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ImageOptionalHeader
    {
        public short Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public int SizeOfCode;
        public int SizeOfInitializedData;
        public int SizeOfUninitializedData;
        public int AddressOfEntryPoint;
        public int BaseOfCode;
        public long ImageBase;
        public int SectionAlignment;
        public int FileAlignment;
        public short MajorOperatingSystemVersion;
        public short MinorOperatingSystemVersion;
        public short MajorImageVersion;
        public short MinorImageVersion;
        public short MajorSubsystemVersion;
        public short MinorSubsystemVersion;
        public int Win32VersionValue;
        public int SizeOfImage;
        public int SizeOfHeaders;
        public int CheckSum;
        public short Subsystem;
        public short DllCharacteristics;
        public long SizeOfStackReserve;
        public long SizeOfStackCommit;
        public long SizeOfHeapReserve;
        public long SizeOfHeapCommit;
        public int LoaderFlags;
        public int NumberOfRvaAndSizes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CONSTANTS.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)] public ImageDataDirectory[] reserved2;
    }

    struct ImageNTHeader
    {
        public int Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
    }
}

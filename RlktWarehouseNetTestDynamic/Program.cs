using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RlktWarehouseNetTestDynamic
{


    internal class Program
    {
#if X64
        const string LibraryName = "RlktWarehouseUpdaterLib_x64.dll";
#else
        const string LibraryName = "RlktWarehouseUpdaterLib_x86.dll"; 
#endif

        [DllImport(LibraryName)]
        static extern int Initialize(int nAppId, int nCurVersion, [MarshalAs(UnmanagedType.LPStr)] string pszServerAddr, int nPort);

        [DllImport(LibraryName)]
        static extern int HasNewerVersion();

        static void Main(string[] args)
        {
            Initialize(0, 0, "gls.ralukat.eu", 9666);

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}

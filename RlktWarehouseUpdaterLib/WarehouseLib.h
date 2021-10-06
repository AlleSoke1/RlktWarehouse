#pragma once

#ifdef RLKT_DLL
#define RLKTDYNAMIC __declspec(dllexport) 
#else
#define RLKTDYNAMIC __declspec(dllimport) 

#ifdef _WIN64
#pragma comment(lib, "RlktWarehouseUpdaterLib_x64.lib") 
#else
#pragma comment(lib, "RlktWarehouseUpdaterLib_x86.lib") 
#endif

#endif

extern "C"
{
	RLKTDYNAMIC DWORD WINAPI Initialize(int nAppId, int nCurVersion, const char* pszServerAddr, int nPort);
	RLKTDYNAMIC BOOL WINAPI HasNewerVersion();
}
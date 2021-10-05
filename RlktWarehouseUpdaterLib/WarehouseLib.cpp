#include <Windows.h>
#include <string>
#include <vector>

#include "PacketDefs.h"
#include "WarehouseLib.h"

#include "Patcher.h"

#pragma comment(lib, "wsock32.lib")
#pragma comment(lib, "Ws2_32.lib")

static int g_iAppID		 = -1;
static int g_iCurVersion = -1;
static int g_iWarehouseVersion = -1;
static bool g_bInitialized = false;

extern "C"
{
	DWORD WINAPI Initialize(int nAppId, int nCurVersion, const char* pszServerAddr, int nPort)
	{
		g_iAppID = nAppId;
		g_iCurVersion = nCurVersion;
		g_bInitialized = true;

		CPatcher* pPatcher = new CPatcher(pszServerAddr, nPort);
		pPatcher->Initialize(nAppId, nCurVersion);

		return 0;
	}


	BOOL WINAPI HasNewerVersion()
	{
		if (g_bInitialized == false)
			return FALSE;

		if (g_iWarehouseVersion == -1)
			return FALSE;

		if (g_iWarehouseVersion > g_iCurVersion)
			return TRUE;

		return FALSE;
	}
}
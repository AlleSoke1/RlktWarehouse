#include <Windows.h>

#include <string>
#include <vector>
#include <thread>
#include <stdio.h>

#include "PacketDefs.h"

#include "Patcher.h"


#define AppID 0
#define AppVersion 1

#define IpAddr "gls.ralukat.eu"
#define Port 9666

#pragma comment(lib, "wsock32.lib")
#pragma comment(lib, "Ws2_32.lib")

int main(int argc, char** argv)
{
	CPatcher *pPatcher = new CPatcher(IpAddr, Port);
	pPatcher->Initialize(AppID, AppVersion);
	///pPatcher->CheckForUpdates();

	/*
	PCS_CheckForUpdates packet(AppID, AppVersion);
	//
	CNetwork *pNetwork = g_NetworkManager.AddNetwork(IpAddr, Port);
	if (pNetwork->Connect() == false)
	{
		printf("Failed to connect [%s]:[%d].\n", IpAddr, Port);
		return 1;
	}

	pNetwork->OnSendPacket((char*)&packet, sizeof(packet));
	*/

	while (1)
	{
		Sleep(100);
	}

	return 0;
}
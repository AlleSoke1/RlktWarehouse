#include "stdafx.h"
#include "Network.h"
#include "PacketDefs.h"
#include "Utils.h"

CNetwork::CNetwork(std::string strServerAddr, int nPort)
	: m_ServerIp(strServerAddr)
	, m_nPort(nPort)
	, m_bConnected(false)
	, m_Socket(INVALID_SOCKET)
{
	// Initialize WinSock
	WSADATA data = {};
	sockaddr_in addrRemote = {};

	WSAStartup(MAKEWORD(2, 2), &data);

	m_Socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (m_Socket == INVALID_SOCKET)
	{
		//Could not create socket.
	}
}

bool CNetwork::Connect()
{
	int iRet = SOCKET_ERROR;

	//Resolve hostname
	sockaddr_in addrRemote;
	iRet = NetUtils::ResolveHostName(m_ServerIp.c_str(), &addrRemote);
	if (iRet == SOCKET_ERROR)
	{
		//Could not resolve hostname.
		return false;
	}

	//Set port and net type
	addrRemote.sin_family = AF_INET;
	addrRemote.sin_port = htons(m_nPort);

	//Init Connection
	iRet = connect(m_Socket, (sockaddr*)&addrRemote, sizeof(addrRemote));
	if (iRet == SOCKET_ERROR)
	{
		//Could not connect to remote endpoint.
		return false;
	}

	//Setup socket ops (tcp no delay)
	int tcpnodelay = 1;
	iRet = setsockopt(m_Socket, IPPROTO_TCP, TCP_NODELAY, (char*)&tcpnodelay, sizeof(int));
	if (iRet == SOCKET_ERROR)
	{
		//Could not set socket options
		return false;
	}

	//Start Recv Thread
	m_RecvThread = std::thread(&CNetwork::RecvThread, this);
	m_RecvThread.detach();

	//
	m_bConnected = true;

	return true;
}

bool CNetwork::Disconnect()
{
	if (m_Socket == INVALID_SOCKET)
		return true;

	int iRet = closesocket(m_Socket);
	if (iRet == SOCKET_ERROR) {
		//Close socket failed.
		WSACleanup();
		return false;
	}

	WSACleanup();

	m_bConnected = false;

	return true;
}

void CNetwork::AddListener(CNetworkListener* pListener)
{
	m_vecListeners.push_back(pListener);
}

void CNetwork::OnSendPacket(char* pData, int nSize)
{
	int iRet = send(m_Socket, (const char*)pData, nSize, 0);
	if (iRet <= 0)
	{
		printf("[%s] SendPacket failed [%d]\n", __FUNCTION__, iRet);
	}
}

void CNetwork::OnRecvPacket(char* pData, int nSize)
{
	WPacket* pPacket = (WPacket*)pData;
	if (pPacket)
	{
		printf("[%s] RecvPacket [%d]\n", __FUNCTION__, pPacket->base.type);
	}

	for (const auto& it : m_vecListeners)
	{
		it->OnRecvPacket(pData, nSize);
	}
}

void CNetwork::RecvThread(CNetwork* pNetwork)
{
	char *data = new char[MAX_WPACKET_SIZE];
	memset(data, 0, sizeof(data));

	while (true)
	{
		int iRecvSize = recv(pNetwork->m_Socket, data, MAX_WPACKET_SIZE, 0);
		if (iRecvSize > 0)
		{
			pNetwork->OnRecvPacket(data, iRecvSize);
		}
		else
		{
			printf("[%s]RecvError:%d\n", __FUNCTION__, iRecvSize);
			break;
		}
	}
}
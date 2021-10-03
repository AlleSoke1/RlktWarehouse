#include "stdafx.h"
#include "NetworkManager.h"
#include "Network.h"

CNetworkManager g_NetworkManager;

CNetworkManager::CNetworkManager()
{

}

CNetworkManager::~CNetworkManager()
{

}

CNetwork* CNetworkManager::AddNetwork(std::string ipAddr, int nPort)
{
	CNetwork *pNetwork = new CNetwork(ipAddr, nPort);
	m_vecNetworks.push_back(pNetwork);
	return pNetwork;
}

CNetwork* CNetworkManager::GetFirstActiveNetwork()
{
	for (const auto& it : m_vecNetworks)
	{
		if (it->IsConnected())
			return it;
	}

	return NULL;
}

bool CNetworkManager::CloseAllNetworks()
{
	for (const auto& it : m_vecNetworks)
	{
		it->Disconnect();
	}

	return true;
}

#pragma once

class CNetwork;
class CNetworkManager
{
public:
	CNetworkManager();
	~CNetworkManager();

	CNetwork* AddNetwork(std::string ipAddr, int nPort);
	CNetwork* GetFirstActiveNetwork();
	bool CloseAllNetworks();

protected:
	std::vector<CNetwork*> m_vecNetworks;
};

extern CNetworkManager g_NetworkManager;
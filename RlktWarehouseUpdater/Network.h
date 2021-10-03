#pragma once

class CNetworkListener;
class CNetwork
{
public:
	CNetwork(std::string strServerAddr, int nPort);

	bool Connect();
	bool Disconnect();
	bool IsConnected() { return m_bConnected; }


	void OnSendPacket(char* pData, int nSize);
	void OnRecvPacket(char* pData, int nSize);

	static void RecvThread(CNetwork* pNetwork);

	void AddListener(CNetworkListener* pListener);

protected:
	std::string m_ServerIp;
	int m_nPort;

	bool m_bConnected;

	SOCKET m_Socket;
	std::thread m_RecvThread;

	std::vector<CNetworkListener*> m_vecListeners;
};
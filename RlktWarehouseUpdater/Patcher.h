#pragma once

#include "NetworkListener.h"

class CPatcherWnd;
class CNetworkListener;

class CPatcher : public CNetworkListener
{
public:
	CPatcher(std::string strIpAddr, int nPort);
	~CPatcher();

	void Initialize(int nAppId, int nCurrentVersion);

	void CheckForUpdates();

	virtual void OnRecvPacket(char* pData, int nSize);

	void OnRecvUpdateInfo(PSC_CheckForUpdates* pData);
	void OnRecvTransferStart(PSC_TransferStart* pData);
	void OnRecvTransferData(PSC_TransferData* pData);
	void OnRecvTransferEnd(PSC_TransferEnd* pData);


protected:
	int m_nAppId;
	int m_nCurVersion;
	
	std::string m_strIpAddr;
	int m_nPort;

	CPatcherWnd* m_pPatchWnd;
};


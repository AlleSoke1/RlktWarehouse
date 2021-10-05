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

	void OnRecvHandshake();
	void OnRecvUpdateInfo(PSC_CheckForUpdates* pData);
	void OnRecvTransferStart(PSC_TransferStart* pData);
	void OnRecvTransferData(PSC_TransferData* pData);
	void OnRecvTransferEnd(PSC_TransferEnd* pData);

	struct XferStruct
	{
		int nSize;
		int nNowSize;
		bool bCompressed;
		int nUncompressSize;
		unsigned int nChecksum;

		char* pBuffer;
		int pBufferIndex;

		XferStruct() : nSize(0), nNowSize(0), bCompressed(false), nChecksum(0), pBuffer(NULL), pBufferIndex(0) { }
		~XferStruct() { delete pBuffer; }
	};
	
	enum TransferResult
	{
		SUCCESS,
		TRANSFER_FAILED,
		CHECKSUM_FAILED,
	};


	void OnTransferResult(TransferResult result);

protected:
	int m_nAppId;
	int m_nCurVersion;
	
	std::string m_strIpAddr;
	int m_nPort;

	XferStruct m_XferData;

	CPatcherWnd* m_pPatchWnd;
};


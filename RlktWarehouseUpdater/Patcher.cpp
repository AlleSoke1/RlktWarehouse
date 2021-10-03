#include "stdafx.h"
#include "Patcher.h"

#include "PatcherWnd.h"

#include "Network.h"
#include "NetworkManager.h"

CPatcher::CPatcher(std::string strIpAddr, int nPort)
	: m_nAppId(0)
	, m_nCurVersion(0)
	, m_pPatchWnd(NULL)
	, m_strIpAddr(strIpAddr)
	, m_nPort(nPort)
{
}

CPatcher::~CPatcher()
{
	if (m_pPatchWnd) delete m_pPatchWnd;
}

void CPatcher::Initialize(int nAppId, int nCurrentVersion)
{
	//Set App info
	m_nAppId = nAppId;
	m_nCurVersion = nCurrentVersion;

	//Create update window
	m_pPatchWnd = new CPatcherWnd();

	//Initialise a network connection
	CNetwork* pNetwork = g_NetworkManager.AddNetwork(m_strIpAddr, m_nPort);
	if (pNetwork->Connect() == false)
	{
		printf("Failed to connect [%s]:[%d].\n", m_strIpAddr.c_str(), m_nPort);
	}

	//Add this class as a listener for new packets
	pNetwork->AddListener(this);
}

void CPatcher::OnRecvPacket(char* pData, int nSize)
{
	WPacket* pPacket = (WPacket*)pData;
	if (pPacket)
	{
		switch (pPacket->base.type)
		{
			case WEPacketType::HANDSHAKE:			OnRecvHandshake(); break;
			case WEPacketType::CHECK_FOR_UPDATES:	OnRecvUpdateInfo((PSC_CheckForUpdates*)pData); break;
			case WEPacketType::XFER_START:			OnRecvTransferStart((PSC_TransferStart*)pData); break;
			case WEPacketType::XFER_DATACHUNK:		OnRecvTransferData((PSC_TransferData*)pData); break;
			case WEPacketType::XFER_END:			OnRecvTransferEnd((PSC_TransferEnd*)pData); break;
		}
	}
}

void CPatcher::CheckForUpdates()
{
	//Get network
	CNetwork* pNetwork = g_NetworkManager.GetFirstActiveNetwork();
	if (!pNetwork)
		return;

	//Send check for update 
	PCS_CheckForUpdates packet(m_nAppId, m_nCurVersion);
	pNetwork->OnSendPacket((char*)&packet, sizeof(packet));
}

void CPatcher::OnRecvHandshake()
{	
	//Get network
	CNetwork* pNetwork = g_NetworkManager.GetFirstActiveNetwork();
	if (!pNetwork)
		return;
	
	//
	WPEmptyPacket packet(WEPacketType::HANDSHAKE);
	pNetwork->OnSendPacket((char*)&packet, sizeof(packet));
}

void CPatcher::OnRecvUpdateInfo(PSC_CheckForUpdates* pData)
{
	if (pData->nAppVersion > m_nCurVersion)
	{
		const int result = MessageBox(NULL, L"A newer version is available, do you want to update?", L"Warehouse Patching Services", MB_YESNO);

		switch (result)
		{
			case IDYES:
			{
				//Get network
				CNetwork* pNetwork = g_NetworkManager.GetFirstActiveNetwork();
				if (!pNetwork)
					return;

				//Send check for update 
				PCS_FileRequest packet(m_nAppId, pData->nAppVersion);
				pNetwork->OnSendPacket((char*)&packet, sizeof(packet));

				//Show update wnd
				m_pPatchWnd->InitWindow();
			}
			break;
		}
	}
}


void CPatcher::OnRecvTransferStart(PSC_TransferStart* pData)
{

}

void CPatcher::OnRecvTransferData(PSC_TransferData* pData)
{

}

void CPatcher::OnRecvTransferEnd(PSC_TransferEnd* pData)
{
	//Destroy window
	m_pPatchWnd->DestroyWindow();
}
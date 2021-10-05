#include "stdafx.h"
#include "Patcher.h"
#include "PatcherWnd.h"
#include "Network.h"
#include "NetworkManager.h"
#include "Utils.h"
#include "zstd.hpp"
#include <fstream>
#pragma comment(lib, "Shlwapi.lib")

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

	//
	CheckForUpdates();
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

				//Send a file request 
				PCS_FileRequest packet(m_nAppId, pData->nAppVersion);
				pNetwork->OnSendPacket((char*)&packet, sizeof(packet));

				//Show update wnd
				m_pPatchWnd->InitWindow();
			}
			break;
			case IDNO:
			{
				//Gracefully close all active sockets
				g_NetworkManager.CloseAllNetworks();
			}
			break;
		}
	}
	else
	{
		//Gracefully close all active sockets
		g_NetworkManager.CloseAllNetworks();
	}
}

void CPatcher::OnRecvTransferStart(PSC_TransferStart* pData)
{
	m_XferData.nNowSize = 0;
	m_XferData.nSize = pData->nAppSize;
	m_XferData.bCompressed = pData->bIsCompressed;
	m_XferData.nUncompressSize = pData->nUncompressSize;
	m_XferData.nChecksum = 0;

	if (m_XferData.pBuffer)
		delete[] m_XferData.pBuffer;
	m_XferData.pBuffer = new char[m_XferData.nSize];
	m_XferData.pBufferIndex = 0;

	memset(m_XferData.pBuffer, 0, m_XferData.nSize);
}

void CPatcher::OnRecvTransferData(PSC_TransferData* pData)
{
	int iDataLen = pData->len - sizeof(WPacketBase);

	for (int i = 0; i < iDataLen; i++)
	{
		m_XferData.pBuffer[m_XferData.pBufferIndex++] = pData->data[i];
	}

	m_XferData.nNowSize += iDataLen;
	m_pPatchWnd->OnProgressUpdate(m_XferData.nNowSize, m_XferData.nSize);
}

void CPatcher::OnRecvTransferEnd(PSC_TransferEnd* pData)
{
	//Integrity check
	if (m_XferData.nNowSize != m_XferData.nSize)
	{
		printf("[%s] Transfer did not ended successfully, [%d out of %d bytes]\n", __FUNCTION__, m_XferData.nNowSize, m_XferData.nSize);
		OnTransferResult(TransferResult::TRANSFER_FAILED);
		return;
	}

	//Checksum
	UINT nChecksum = NetUtils::GetChecksum((const unsigned char*)m_XferData.pBuffer, m_XferData.nSize);
	if (nChecksum != pData->nAppChecksum)
	{
		printf("[%s] Transfer did not ended successfully, checksum is missmatched.\n", __FUNCTION__);
		OnTransferResult(TransferResult::CHECKSUM_FAILED);
		return;
	}

	//Get current executable name
	TCHAR buffer[MAX_PATH] = { 0 };
	if (GetModuleFileName(NULL, buffer, MAX_PATH) == NULL)
	{
		OnTransferResult(TransferResult::TRANSFER_FAILED);
		return;
	}

	//Rename old executable
	std::wstring newExeName(buffer);
	newExeName += L".old";
	DeleteFile(newExeName.c_str());
	_wrename(buffer, newExeName.c_str());

	//If is compressed, decompress first.
	if (m_XferData.bCompressed)
	{
		char* pBuffer = new char[m_XferData.nUncompressSize];
		int iResult = ZSTD_decompress(pBuffer, m_XferData.nUncompressSize, m_XferData.pBuffer, m_XferData.nSize);
		if (iResult <= 0)
		{
			printf("[%s] ZStd Decompress Failed with errcode[%d]\n", __FUNCTION__, iResult);		
			OnTransferResult(TransferResult::TRANSFER_FAILED);
			return;
		}

		m_XferData.pBuffer = pBuffer;
		m_XferData.nSize = iResult;
	}

	//Flush to disk
	std::ofstream outFile(buffer, std::ios::out | std::ios::binary);
	if (outFile.is_open())
	{
		outFile.write((const char*)m_XferData.pBuffer, m_XferData.nSize);
		outFile.close();
	}

	//Destroy window
	m_pPatchWnd->DestroyWindow();

	//Gracefully close all active sockets
	g_NetworkManager.CloseAllNetworks();

	//Get command line and restart process
	std::wstring cmdLine = GetCommandLineW();

	SHELLEXECUTEINFO shExecInfo;
	shExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
	shExecInfo.fMask = NULL;
	shExecInfo.hwnd = NULL;
	shExecInfo.lpVerb = L"open";
	shExecInfo.lpFile = buffer;
	shExecInfo.lpParameters = cmdLine.c_str();
	shExecInfo.lpDirectory = NULL;
	shExecInfo.nShow = SW_NORMAL;
	shExecInfo.hInstApp = NULL;
	ShellExecuteEx(&shExecInfo);

	Sleep(500);

	ExitProcess(0);
}

void CPatcher::OnTransferResult(TransferResult result)
{
	switch (result)
	{
		case TransferResult::TRANSFER_FAILED:
		{
			//Retry?
		}
		break;

		case TransferResult::CHECKSUM_FAILED:
		{
			//Retry?
		}
		break;
	}
}
#pragma once

class CNetworkListener
{
public:
	virtual void OnRecvPacket(char* pData, int nSize) { };
};
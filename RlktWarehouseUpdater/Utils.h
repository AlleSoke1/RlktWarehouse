#pragma once

namespace NetUtils
{
    int ResolveHostName(const char* pszHostName, sockaddr_in* pAddr);
    unsigned int GetChecksum(const unsigned char* pData, int nSize);
}
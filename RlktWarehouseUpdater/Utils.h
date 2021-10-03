#pragma once

namespace NetUtils
{
    int ResolveHostName(const char* pszHostName, sockaddr_in* pAddr);
}
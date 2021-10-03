#include "stdafx.h"
#include "Utils.h"
#include <WS2tcpip.h>

namespace NetUtils
{
    int ResolveHostName(const char* pszHostName, sockaddr_in* pAddr)
    {
        int ret;
        HRESULT hr = S_OK;
        addrinfo* pResultList = NULL;
        addrinfo hints = {};
        int result = -1;

        hints.ai_family = AF_INET;
        hints.ai_socktype = SOCK_STREAM;

        ret = getaddrinfo(pszHostName, NULL, &hints, &pResultList);

        result = (ret == 0) ? 1 : -1;
        if (result != -1)
        {
            *pAddr = *(sockaddr_in*)(pResultList->ai_addr);
            result = 0;
        }

        if (pResultList != NULL)
        {
            ::freeaddrinfo(pResultList);
        }

        return result;
    }
}
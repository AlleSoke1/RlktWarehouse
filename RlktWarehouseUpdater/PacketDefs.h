#pragma once

#pragma pack(push,1)

#define HEADER_SIZE	sizeof(WPacketBase)
#define MAX_WPACKET_SIZE 64*1024
#define MAX_WPACKET_SIZE_ENCAP MAX_WPACKET_SIZE - HEADER_SIZE

#pragma region Base Packet

enum WEPacketType
{
	NOT_DEFINED,
	HANDSHAKE,
	CHECK_FOR_UPDATES,
	FILE_REQUEST,
	XFER_START,
	XFER_DATACHUNK,
	XFER_END,
	FILE_DEPOSIT,
	PACKET_TYPE_END
};

struct WPacketBase
{
	WEPacketType type;
	int len;
	WPacketBase() : len(0), type(NOT_DEFINED) {}
};

struct WPacket
{
	WPacketBase base;
	unsigned char data[MAX_WPACKET_SIZE_ENCAP];
};

struct WPEmptyPacket : WPacketBase
{
	WPEmptyPacket(WEPacketType packetType)
	{
		type = packetType;
		len = sizeof(WPEmptyPacket);
	}
};

#pragma endregion

#pragma region Packet Definitions

struct PCS_CheckForUpdates : WPacketBase
{
	int nAppId;
	int nCurAppVersion;
	PCS_CheckForUpdates(int appId, int curAppVersion) : nAppId(appId), nCurAppVersion(curAppVersion)
	{
		type = CHECK_FOR_UPDATES;
		len = sizeof(PCS_CheckForUpdates);
	}
};

struct PSC_CheckForUpdates : WPacketBase
{
	int nAppVersion;

	PSC_CheckForUpdates(int appVersion) : nAppVersion(appVersion)
	{
		type = CHECK_FOR_UPDATES;
		len = sizeof(PSC_CheckForUpdates);
	}
};

struct PCS_FileRequest : WPacketBase
{
	int nAppId;
	int nAppVersion;
	PCS_FileRequest(int appId, int appVersion) : nAppId(appId), nAppVersion(appVersion)
	{
		type = FILE_REQUEST;
		len = sizeof(PCS_FileRequest);
	}
};

struct PSC_TransferStart : WPacketBase
{
	unsigned int nAppSize;
	bool		 bIsCompressed;
	int			 nUncompressSize;

	PSC_TransferStart()
	{
		type = XFER_START;
		len = sizeof(PSC_TransferStart);
	}
};

struct PSC_TransferData : WPacketBase
{
	unsigned char data[MAX_WPACKET_SIZE_ENCAP];
	PSC_TransferData()
	{
		type = XFER_DATACHUNK;
	}
};

struct PSC_TransferEnd : WPacketBase
{
	unsigned int nAppChecksum;
	PSC_TransferEnd()
	{
		type = XFER_END;
		len = sizeof(PSC_TransferEnd);
	}
};
#pragma endregion 


#pragma pack(pop)
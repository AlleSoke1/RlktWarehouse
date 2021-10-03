using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer.packets
{
    public enum WEPacketType
    {
        NOT_DEFINED,
        HANDSHAKE,
        CHECK_FOR_UPDATES,
        FILE_REQUEST,
        XFER_START,
        XFER_DATACHUNK,
        XFER_END,
    };

    internal class WPacket
    {
        virtual public bool ProcessRequest() { return false; }
    }
}

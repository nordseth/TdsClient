using System;
using System.Collections.Generic;
using System.Text;

namespace TdsClient.Protocol
{
    public enum TdsType
    {
        TDS_BINARY = 0x2D,
        TDS_BIT = 0x32,
        TDS_CHAR = 0x2F,
        TDS_DATETIMEN = 0x6F,
        TDS_DECN = 0x6A,
        TDS_FLT4 = 0x3B,
        TDS_FLT8 = 0x3E,
        TDS_FLTN = 0x6D,
        TDS_IMAGE = 0x22,
        TDS_INT1 = 0x30,
        TDS_INT2 = 0x34,
        TDS_INT4 = 0x38,
        TDS_INT8 = 0xBF,
        TDS_INTN = 0x26,
        TDS_UINT1 = 0x40,
        TDS_UINT2 = 0x41,
        TDS_UINT4 = 0x42,
        TDS_UINT8 = 0x43,
        TDS_UINTN = 0x44,
        TDS_LONGBINARY = 0xE1,
        TDS_LONGCHAR = 0xAF,
        TDS_MONEY = 0x3C,
        TDS_SHORTMONEY = 0x7A,
        TDS_MONEYN = 0x6E,
        TDS_NUMN = 0x6C,
        TDS_TEXT = 0x23,
        TDS_VARBINARY = 0x25,
        TDS_SENSITIVITY = 0x67,
        TDS_BOUNDARY = 0x68,
        TDS_VARCHAR = 0x27,
        TDS_BLOB = 0x24,
        TDS_VOID = 0x1f,
        TDS_DATETIME = 0x3D,
        TDS_SHORTDATE = 0x3A,
    }
}

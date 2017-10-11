using System;
using System.Collections.Generic;
using System.Text;

namespace TdsClient.Protocol
{
    public enum TokenType
    {
        TDS_ALTCONTROL = 0xAF, // (obsolete)
        TDS_ALTFMT = 0xA8, // 2 byte length, describes data type, length and status of COMPUTE data
        TDS_ALTNAME = 0xA7, // 2 byte length, describes number and name of a COMPUTE clause
        TDS_ALTROW = 0xD3, // row data for compute clause. length depends on number of COMPUTE operators and data in each operator (from TDS_ALTFMT)
        TDS_CAPABILITY = 0xE2, // 2 bytes length, list of capabilities
        TDS_COLFMT = 0xA1, // (obsolete)
        TDS_COLFMTOLD = 0x2A, // (obsolete)
        TDS_COLINFO = 0xA5, // 2 byte length, column information for browse mode
        TDS_COLNAME = 0xA0, // (obsolete)
        TDS_CONTROL = 0xAE, // 2 byte length, use controll or format for columns
        TDS_CURCLOSE = 0x80, // 2 byte length, cursor...
        TDS_CURDECLARE = 0x86, // 2 byte length, cursor...
        TDS_CURDECLARE2 = 0x23, // 4 byte length, cursor...
        TDS_CURDELETE = 0x81, // 2 byte length, cursor...
        TDS_CURFETCH = 0x82, // 2 byte length, cursor...
        TDS_CURINFO = 0x83, // 2 byte length, cursor...
        TDS_CUROPEN = 0x84, // 2 byte length, cursor...
        TDS_CURUPDATE = 0x85, // 2 byte length, cursor...
        TDS_DBRPC = 0xE6, // 2 byte length, 
        TDS_DEBUGCMD = 0x60,
        TDS_DONE = 0xFD, // 8 bytes
        TDS_DONEINPROC = 0xFF, // 8 bytes
        TDS_DONEPROC = 0xFE, // 8 bytes
        TDS_DYNAMIC = 0xE7, // 2 byte length, 
        TDS_DYNAMIC2 = 0xA3, // 4 byte length, 
        TDS_EED = 0xE5, // 2 byte length, text message to client
        TDS_ENVCHANGE = 0xE3, // 2 byte length, 
        TDS_ERROR = 0xAA, // (obsolete) 2 byte length, 
        TDS_EVENTNOTICE = 0xA2, // 2 byte length, 
        TDS_INFO = 0xAB, // (obsolete) 2 byte length, 
        TDS_KEY = 0xCA, // key data, length depends on number of keys (from TDS_ROWFMT?)
        TDS_LANGUAGE = 0x21, // 4 byte length, send sql, use TDS_PARAMFMT and TDS_PARAMS
        TDS_LOGINACK = 0xAD, // 2 byte length, 
        TDS_LOGOUT = 0x71, // 1 byte
        TDS_MSG = 0x65, // 1 byte length, 
        TDS_OFFSET = 0x78, // 4 bytes
        TDS_OPTIONCMD = 0xA6, // 2 byte length, 
        TDS_ORDERBY = 0xA9, // 2 byte length, columns in an "order by" clause of select. Number of clauses is the first two bytes, each clause is a one byte index (to a select list in ??)
        TDS_ORDERBY2 = 0x22, // 4 byte length, 
        TDS_PARAMFMT = 0xEC, // 2 byte length,
        TDS_PARAMFMT2 = 0x20, // 4 byte length, 
        TDS_PARAMS = 0xD7, // parameter data, length depends on TDS_PARAMFMT
        TDS_PROCID = 0x7C, // (obsolete)
        TDS_RETURNSTATUS = 0x79, // 4 bytes
        TDS_RETURNVALUE = 0xAC, // (obsolete) 2 byte length,
        TDS_ROW = 0xD1, // row data, length depends on TDS_ROWFMT
        TDS_ROWFMT = 0xEE, // 2 byte length,
        TDS_ROWFMT2 = 0x61, // 4 byte length,
        TDS_RPC = 0xE0, // (obsolete) 2 byte length,
        TDS_TABNAME = 0xA4, // 2 byte length,
    }
}

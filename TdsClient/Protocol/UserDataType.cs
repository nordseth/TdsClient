using System;
using System.Collections.Generic;
using System.Text;

namespace TdsClient.Protocol
{
    public enum UserDataType
    {
        @char = 1, // TDS_VARCHAR, blank pad to the length in the format
        varchar = 2, // TDS_VARCHAR     
        binary = 3,     // TDS_BINARY, null pad to the length in the format
        varbinary = 4, // TDS_BINARY      
        tinyint = 5, // TDS_INTN        
        smallint = 6, // TDS_INTN        
        @int = 7, // TDS_INTN 		
        @float = 8, // TDS_FLTN 		
        numeric = 10, // TDS_NUMERIC     
        money = 11, // TDS_MONEYN      
        datetime = 12, // TDS_DATETIMEN   
        intn = 13, // TDS_INTN        
        floatn = 14, // TDS_FLTN        
        datetimn = 15, // TDS_DATETIMN    
        bit = 16, // TDS_BIT         
        moneyn = 17, // TDS_MONEYN      
        sysname = 18, // TDS_VARCHAR     
        text = 19, // TDS_TEXT        
        image = 20, // TDS_IMAGE       
        smallmoney = 21, // TDS_MONEYN      
        smalldatetime = 22, // TDS_DATETIMN    
        real = 23, // TDS_FLTN        
        nchar = 24, // TDS_VARCHAR     
        nvarchar = 25, // TDS_VARCHAR     
        @decimal = 26, // TDS_NUMERIC 	
        decimaln = 27, // TDS_NUMERIC     
        numericn = 28, // TDS_NUMERIC     
        unichar = 34, // TDS_LONGBINARY, fixed length UTF-16 encoded data
        univarchar = 35, // TDS_LONGBINARY, variable length UTF-16 encoded data
        unitext = 36, // TDS_IMAGE, UTF-16 encoded data
        date = 50, // TDS_DATETIMN, The hh:mm:ss.nnnn information should be ignored
        time = 51, // TDS_DATETIMN, The mm/dd/yyyy information shouldbe ignored
        @ushort = 52, // TDS_INTN        
        @uint = 53, // TDS_INTN        
        @ulong = 54, // TDS_INTN        
        serialization = 55, // TDS_LONGBINARY, serialized java object or instance (i.e. java object)
        serialized_java_class = 56, // TDS_LONGBINARY, serialized java class (i.e. byte code)
        @string = 57, // TDS_LONGCHAR, internally generated varchar strings(e.g.select @@version), not table columns
        unknown = 58, // TDS_INTN, a describe input will return TDS_INT4(as a simple placeholder) for all columns where it does not know the datatype.This usertype indicates that the actual type is unknown.
        smallbinary = 59, // TDS_LONGBINARY, 64K max length binary data(ASA)
        smallchar = 60, // TDS_LONGCHAR, 64K maximum length char data(ASA)
        timestamp = 80, // TDS_BINARY, This has nothing to do with date or time, it is an ASE unique value for use with optimistic concurrency
    }
}

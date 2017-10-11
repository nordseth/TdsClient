# .Net Core database client for Sybase/SAP ASE with Tds protocol

Inspired by and partly based on [jTDS](http://jtds.sourceforge.net/).

## Features
* Intended for use with Dapper.Net or ADONET and Sybase/SAP ASE
* No cursor, data table or browse support
* support for language commands with parameters

## Usage

1. Add nuget
2. Configure a connection string. Example `"Data Source=192.168.0.25;Port=5000;User=sa;Pass=password;db=pubs2"`
3. Create a connectin `new TdsConnection(connectionString, null)` (logging is optional)
4. Use and dispose the connection. See tests for samples

## Missing features and limitations

### Limitations
* Support reading and writing more datatypes
* Support for output params
* Better support for long string and binary datatypes
* More tests

### Features comming later (0.2.0?)
* Better error handling
* Timeouts
* Connection pooling

### Unplaneed features
* EF Core support


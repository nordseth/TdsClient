# .Net Core database client for Sybase/SAP ASE with Tds protocol

Inspired by and partly based on [jTDS](http://jtds.sourceforge.net/).

## Features
* Intended for use with Dapper or ADONET and Sybase/SAP ASE
* No cursor, data table or browse support
* Support for language commands with parameters

## Usage

1. Add [nuget package](https://www.nuget.org/packages/TdsClient)
2. Configure a connection string. Example `"Data Source=192.168.0.25;Port=5000;User=sa;Pass=password;db=pubs2"`
3. Create a connectin `new TdsConnection(connectionString, null)` (logging is optional)
4. Open, use and dispose the connection. See tests for samples

### Connection string format

The connection string is a pair of parameter and value seperated by "=".
The pairs are concatiated, seperated by ";".

Example `parameter1=value1;parameter2=value2`
There are no escape characters, tho a value can contain an "=".
Parameter names are case insensitive. Leading and trailing whitespace is trimmed from parameter name. Only leading whitespace is trimmed from value.

|Parameter | Default | Description  | Alias |
| --- | --- | --- | --- |
| Data Source |  | Network address of database server | Server, Address, ServerHostname, Hostname |
| Port | 5000 | Tcp port |	|
| ClientHostname | MachineName | Sent to server on connect | |
| Username | | | usr, uid, user|
| Password | | | pwd, pass|
| Initial Catalog | | |	Database, DB, Catalog|
| ProcessId|Current process ID | Sent to server on connect | pid|
| AppliactionName| Current process name| Sent to server on connect| Appliaction, app|
| ServerName| set to "Data Source" |Sent to server on connect |	|
| Language| (blank)| Database language | lang |
| Charset| iso_1| char set to use with connection | |

## Missing features and limitations

### Limitations (Comming soon)
* Not all datatypes supported for reading and writing 
* No support for output params
* Support for long string and binary datatypes limited
* Needs more tests

### Features comming later (0.2.0?)
* Connection and command timeout handling
* Connection pooling
* Better error handling

### Not planned features
* EF Core support


Distributed Backup System
=========================

UDP Multicast
-------------

Program written in C# (.NET 4.5.1) using Visual Studio 2013, following the specification at [http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html](http://web.fe.up.pt/~pfs/aulas/sd2014/proj1.html) (with enhancements).

Enhanced protocols specification and rational is described in [protocols.pdf](protocols.pdf).

## Libraries:

- [JsonConfig](https://github.com/Dynalon/JsonConfig)
- [Reactive Extensions (Rx)](http://msdn.microsoft.com/en-us/data/gg577609)
- [C#-SQLite](https://code.google.com/p/csharp-sqlite/)

## Projects

- `DBS`: main library
- `DBSTests`: some unit tests for DBS
- `Peer`: command line interface
- `PeerGUI`: graphical user interface

## Compiling

Open `SDIS_DistributedBackupService.sln` in Visual Studio and compile in Debug or Release mode.

## Running

Default multicast channels:

- **MC**: 225.0.0.10:31000
- **MDB**: 225.0.0.10:31001
- **MDR**: 225.0.0.10:31002

- CLI: Start Peer.exe with the following optional arguments: MCIP MCport MDBIP MDBPort MDRIP MDRPort
    - Available commands:
        - `quit`
        - `backup <filename> <replication degree>`
        - `restore <filename>`
        - `delete <filename>`

- GUI: Start PeerGUI.exe

## Authors

- Duarte Duarte - ei11101
- Ruben Cordeiro - ei11097

Mestrado Integrado em Engenharia Informática e Computação, Sistema Distribuídos  
Faculdade de Engenharia da Universidade do Porto, 2014
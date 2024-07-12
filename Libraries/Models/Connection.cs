using System.Runtime.InteropServices;

using Libraries.Enums;

namespace Libraries.Models;

public class MasterConnection
{
    public string IPv4 { get; set; }
    //public string IPv6 = "0:0:0:0:0:0:0:1";
    //public string URL = "locahlhost";
    public ushort Port { get; set; }

    public MasterConnection(string ipv4 = "127.0.0.1", ushort port = 19186)
    {
        IPv4 = ipv4;
        Port = port;
    }
}

// File Protocoll Connection
public class FPConnection
{
    public string User { get; set; }
    public string Password { get; set; }
    public string URL { get; set; }
    public string Remote_Directory { get; set; }
    public string Connection_String { get; set; }
}

public class FTPConnection : FPConnection
{
    public FTPConnection() { }
    public FTPConnection(string url, string user, string password, string directory = "")
    {
        URL = url;
        Remote_Directory = directory;
        User = user;
        Password = password;

        //Connection_String = $"ftp://{User}:{Password}@{URL}/{Remote_Directory}/";
    }
}

public class SMBConnection : FPConnection
{
    public SMBConnection() { }
    public SMBConnection(string url, string user, string password, string directory = "")
    {
        URL = url;
        Remote_Directory = directory;
        User = user;
        Password = password;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Connection_String = Get_Windows_String();
        }
        else
        {
            Connection_String = Get_Unix_String();
        }
    }

    public string Get_Windows_String()
    {
        return $"\\\\{URL}/{Remote_Directory}/";
    }

    public string Get_Unix_String()
    {
        return $"smb://{User}:{Password}@{URL}/{Remote_Directory}/";
    }
}

public class DBConnection
{
    public string? User { get; set; }
    public string? Password { get; set; }
    public string Path { get; set; }
    public string Database { get; set; }
    public ushort? Port { get; set; }
    public DBMode Mode { get; set; }

    public string Connection_String
    {
        get
        {
            switch (Mode)
            {
                case DBMode.SQLite:
                    return $"Data Source={System.IO.Path.Join(Path, "PRF.db")}";
                case DBMode.MySQL:
                    return $"Server={Path}; Port={Port}; User={User}; Password={Password}; Database={Database};";
                case DBMode.PostgreSQL:
                case DBMode.MSSQL:
                    return $"Data Source={Path}; Port={Port}; User={User}; Password={Password}; Database={Database};";
                default:
                    goto case DBMode.SQLite;
            }
        }
    }

    public DBConnection(DBMode mode, string path, string? user = null, string? password = null)
    {
        Path = path;
        User = user;
        Password = password;
        Mode = mode;
    }
}
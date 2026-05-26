namespace vsHelp.Core.Models;

public class DatabaseConnectionSettings
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string User { get; set; } = "root";
    public string Password { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;

    public string GetConnectionString(bool includeDatabase = true)
    {
        var connString = $"Server={Server};Port={Port};Uid={User};Pwd={Password};AllowUserVariables=True;ConnectionTimeout=60;";
        if (includeDatabase && !string.IsNullOrWhiteSpace(DatabaseName))
        {
            connString += $"Database={DatabaseName};";
        }
        return connString;
    }
}

namespace AppEngine.DataAccess;

public class ConnectionString(string connectionString)
{
    public override string ToString()
    {
        return connectionString;
    }
}
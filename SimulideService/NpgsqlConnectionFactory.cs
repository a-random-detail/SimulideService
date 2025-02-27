using System.Data;
using Npgsql;
using SimulideService.Repositories;

namespace SimulideService;
public class NpgsqlConnectionFactory()
{
    public NpgsqlConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }
}

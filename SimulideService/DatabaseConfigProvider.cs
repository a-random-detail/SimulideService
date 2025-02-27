namespace SimulideService;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message) : base(message) { }
}

public class DatabaseConfig
{
    public string Host { get; init; }
    public int Port { get; init; }
    public string Database { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public bool Pooling { get; init; }
    public int MinPoolSize { get; init; }
    public int MaxPoolSize { get; init; }

    public static DatabaseConfig Load(IConfiguration configuration)
    {
        string GetRequiredValue(string key)
        {
            string? value = Environment.GetEnvironmentVariable(key) ?? configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidConfigurationException($"Missing required configuration: {key}");
            }
            return value;
        }

        int GetIntValue(string key, int defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(key) ?? configuration[key];
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        bool GetBoolValue(string key, bool defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(key) ?? configuration[key];
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        return new DatabaseConfig
        {
            Host = GetRequiredValue("DB_HOST"),
            Port = GetIntValue("DB_PORT", 5432),
            Database = GetRequiredValue("DB_NAME"),
            Username = GetRequiredValue("DB_USER"),
            Password = GetRequiredValue("DB_PASSWORD"),
            Pooling = GetBoolValue("DB_POOLING", true),
            MinPoolSize = GetIntValue("DB_MIN_POOL_SIZE", 1),
            MaxPoolSize = GetIntValue("DB_MAX_POOL_SIZE", 20)
        };
    }

    public string GetConnectionString() =>
        $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};Pooling={Pooling};MinPoolSize={MinPoolSize};MaxPoolSize={MaxPoolSize};";
}
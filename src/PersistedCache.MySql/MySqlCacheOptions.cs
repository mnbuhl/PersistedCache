namespace PersistedCache.MySql
{
    public class MySqlCacheOptions : PersistedCacheOptions
    {
        public MySqlCacheOptions(string connectionString) : base(connectionString)
        {
        }

        public override string TableName => "PersistedCache";
    }
}
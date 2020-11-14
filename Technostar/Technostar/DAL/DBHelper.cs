using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;

using Technostar.DAL.Mappers;

namespace Technostar.DAL
{
    public class DBHelper
    {
        private readonly SqlConnection _connection;

        public DBHelper()
        {
            _connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            _connection.Open();
        }

        public IReadOnlyList<T> GetData<T>(IMapper<T> mapper, SqlCommand command)
        {
            var result = new List<T>();
            command.Connection = _connection;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(mapper.ReadItem(reader));

            return result;
        }
        public T GetItem<T>(IMapper<T> mapper, SqlCommand command)
        {
            command.Connection = _connection;

            using var reader = command.ExecuteReader();
            reader.Read();

            return mapper.ReadItem(reader);
        }

        public void ExecuteNonQuery(SqlCommand command)
        {
            command.Connection = _connection;

            command.ExecuteNonQuery();
        }
        public T ExecuteScalar<T>(SqlCommand command)
        {
            command.Connection = _connection;

            return (T) command.ExecuteScalar();
        }
    }
}

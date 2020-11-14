using System;
using System.Data.SqlClient;

using Technostar.Models;

namespace Technostar.DAL.Mappers
{
    public class RecordMapper : IMapper<Record>
    {
        public Record ReadItem(SqlDataReader dr)
        {
            return new Record
            {
                Id = (int) dr["Id"],
                Content = (string) dr["Content"],
                TimeStamp = (DateTime) dr["TimeStamp"]
            };
        }
    }
}

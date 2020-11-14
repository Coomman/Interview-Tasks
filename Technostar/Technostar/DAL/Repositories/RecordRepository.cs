using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Technostar.Models;
using Technostar.DAL.Mappers;

namespace Technostar.DAL.Repositories
{
    public class RecordRepository
    {
        private readonly DBHelper _dbHelper = new DBHelper();

        public IReadOnlyCollection<Record> GetAllRecords()
        {
            return _dbHelper.GetData(new RecordMapper(), new SqlCommand(Queries.GetAllRecords));
        }

        public Record AddRecord(string content)
        {
            using var command = new SqlCommand(Queries.AddRecord);
            command.Parameters.Add(new SqlParameter("@content", SqlDbType.NVarChar) {Value = content});

            return _dbHelper.GetItem(new RecordMapper(), command);
        }
        public Record UpdateRecord(int recordId, string newContent)
        {
            using var command = new SqlCommand(Queries.UpdateRecord);
            command.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) {Value = recordId});
            command.Parameters.Add(new SqlParameter("@newContent", SqlDbType.NVarChar) {Value = newContent});

            return _dbHelper.GetItem(new RecordMapper(), command);
        }
    }
}

using System.Data.SqlClient;

namespace Technostar.DAL.Mappers
{
    public interface IMapper<out T>
    {
        T ReadItem(SqlDataReader dr);
    }
}

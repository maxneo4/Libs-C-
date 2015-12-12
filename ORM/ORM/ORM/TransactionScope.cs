using System.Data.SqlClient;

namespace ORM
{
    class TransactionScope
    {
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }
        public SqlCommand Command { get; set; }
    }
}

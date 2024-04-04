using MedicalCertificates.Models;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Text;

namespace MedicalCertificates.Services
{
    public static class EnsureCreateDb
    {
        public static void EnsureAndCreate()
        {
            var db = new MedicalCertificatesDbContext();
            if (!db.Database.CanConnect())
            {
                Create();
            }
        }

        public static bool Create()
        {
            string sqlConnectionString = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=master;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";
            FileInfo file = new FileInfo("CreateQuery.sql");
            string script = File.ReadAllText("CreateQuery.sql", Encoding.GetEncoding(1251));

            SqlConnection conn = new SqlConnection(sqlConnectionString);
            Server server = new Server(new ServerConnection(conn));
            server.ConnectionContext.ExecuteNonQuery(script);

            return false;
        }
    }
}
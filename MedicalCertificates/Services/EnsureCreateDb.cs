using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalCertificates.Services
{
    public static class EnsureCreateDb
    {
        public static void EnsureAndCreate()
        {
            var db = new MedCertificatesDbContext();
            if (!db.Database.CanConnect())
            {
                var alert = new AcceptAlert("Внимание!", "База данных «MedicalCertificates» отсутствует на данном сервере.\nДля продолжения работы необходимо создать базу данных. Вы желаете продолжить?");
                if (alert.ShowDialog() == true)
                {
                    var createAlert = new Alert.Alert("Внимание!", "База данных создается...");
                    createAlert.YesButton.IsEnabled = false;
                    createAlert.Show();
                    Create();
                    createAlert.Close();
                }
                else
                    App.Current.Shutdown();
            }
        }

        public static bool Create()
        {
            string sqlConnectionString = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=master;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";

            if (!File.Exists("CreateQuery.sql"))
            {
                var alert = new Alert.Alert("Ошибка!", "Ошибка: Файл создания базы данных отсутствует. Переустановите программу.", Alert.AlertType.Error);
                alert.ShowDialog();

                App.Current.Shutdown();
            }

            string script = File.ReadAllText("CreateQuery.sql", Encoding.GetEncoding(1251));

            using (SqlConnection conn = new SqlConnection(sqlConnectionString))
            {
                Server server = new Server(new ServerConnection(conn));
                server.ConnectionContext.ExecuteNonQuery(script);

                Thread.Sleep(10000);
                return false;
            }
        }
    }
}
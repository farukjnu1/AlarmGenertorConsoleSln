using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmGenertorConsole.Models
{
    public class Fcm_User
    {
        public int nID { get; set; }
        public string strUser { get; set; }
        public string DeviceID { get; set; }
        public DateTime DeviceTime { get; set; }

        // extra property
        public string strTEID { get; set; }

        public List<Fcm_User> Get(string connectionString)
        {
            string q = @"SELECT fu.nID
            , fu.strUser
            , fu.DeviceID
            , fu.DeviceTime
            , cm.strTEID FROM Fcm_User fu 
            JOIN Table_CarManage cm ON fu.strUser = cm.strUser
            WHERE cm.strUser NOT LIKE 'noc%'
            AND cm.strUser NOT LIKE 'admin%'
            AND cm.strUser NOT LIKE 'atlbd%'
            AND cm.strUser NOT LIKE 'faruktest%'";

            var oFcm_Users = new List<Fcm_User>();

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new System.Data.SqlClient.SqlCommand(q, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    //var employeeList = new Classes.EmployeePopulator().CreateList(reader);  
                    //oFcm_Users = new Services.GenericPopulator<Fcm_User>().CreateList(reader);
                    oFcm_Users = new Services.ReflectionPopulator<Fcm_User>().CreateList(reader);

                    reader.Close();
                }
                conn.Close();
            }

            return oFcm_Users;
        }

    }
}

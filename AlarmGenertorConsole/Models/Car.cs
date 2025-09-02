using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmGenertorConsole.Models
{
    public class Car
    {
        public string strTEID { get; set; }
        public string strSmsNoticeTel { get; set; }
        public int nSMSNoticeState { get; set; }
        public int BalanceSms { get; set; }
        public string strCarNum { get; set; }
        public string strSmsNoticeTel2 { get; set; }
        public NoticeType NoticeType { get; set; }

        public static List<Car> Get()
        {
            string GpsConStr = ConfigurationManager.ConnectionStrings["GpsDbConStr"].ConnectionString;

            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader rd;
            con = new SqlConnection(GpsConStr);
            cmd.Connection = con;
            con.Open();
            
            string query = @"SELECT [Table_Alarm].[strTEID]
            , [Table_Alarm].[strSmsNoticeTel]
            ,[Table_Alarm].[nSMSNoticeState]
            ,[Table_Alarm].[BalanceSms]
            ,[Table_Car].[strCarNum]
            ,[Table_Alarm].[strSmsNoticeTel2]
            ,[Table_Alarm].[NoticeType]
            FROM [Table_Alarm] JOIN [Table_Car] ON [Table_Alarm].[strTEID] = [Table_Car].[strTEID] 
            WHERE [Table_Alarm].[nSMSNoticeState] > 0 AND [Table_Car].[strCarNum] NOT LIKE '%off%'";
            cmd.CommandText = query;
            rd = cmd.ExecuteReader();
            List<Car> cars = new List<Car>();
            while (rd.Read())
            {
                if (rd.GetString(1).Length > 10)
                {
                    Car car = new Car();
                    car.strTEID = rd.IsDBNull(0) ? "": rd.GetString(0);
                    car.strSmsNoticeTel = rd.IsDBNull(1) ? "" : rd.GetString(1);
                    car.nSMSNoticeState = rd.IsDBNull(2) ? 0 : rd.GetInt32(2);
                    car.BalanceSms = rd.IsDBNull(3) ? 0 : rd.GetInt32(3);
                    car.strCarNum = rd.IsDBNull(4) ? "" : rd.GetString(4);
                    car.strSmsNoticeTel2 = rd.IsDBNull(5) ? "" : rd.GetString(5);
                    car.NoticeType = rd.IsDBNull(6) ? NoticeType.SMS : (NoticeType)rd.GetInt32(6);
                    cars.Add(car);
                }

            }
            rd.Close();
            cmd.Dispose();
            con.Close();

            return cars;
        }

    }
}

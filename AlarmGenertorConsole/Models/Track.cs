using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AlarmGenertorConsole.Models
{
    public class Track
    {
        public int nID { get; set; }
        public string strTEID { get; set; }
        public int nTime { get; set; }
        public string strTime { get; set; }
        public decimal dbLon { get; set; }
        public decimal dbLat { get; set; }
        public int nDirection { get; set; }
        public int nSpeed { get; set; }
        public int nGSMSignal { get; set; }
        public int nGPSSignal { get; set; }
        public int nFuel { get; set; }
        public int nMileage { get; set; }
        public int nTemp { get; set; }
        public int nCarState { get; set; }
        public int nTEState { get; set; }
        public int nAlarmState { get; set; }
        public string strOther { get; set; }

        public static List<Track> Get(string query)
        {
            string GPSDB = System.Configuration.ConfigurationManager.ConnectionStrings["GPSDB"].ConnectionString;

            var oTracks = new List<Track>();
            SqlDataReader rd = null;
            SqlConnection con = null;
            SqlCommand cmd = null;
            
            con = new SqlConnection(GPSDB);
            cmd = new SqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandText = query;
            try
            {
                rd = cmd.ExecuteReader();
                rd.Read();
                while (rd.Read())
                {
                    var oTrack = new Track();
                    oTrack.nID = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
                    oTrack.strTEID = rd.IsDBNull(1) ? "" : rd.GetString(1);
                    oTrack.nTime = rd.IsDBNull(2) ? 0 : rd.GetInt32(2);
                    oTrack.dbLon = rd.IsDBNull(3) ? 0 : rd.GetDecimal(3);
                    oTrack.dbLat = rd.IsDBNull(4) ? 0 : rd.GetDecimal(4);
                    oTrack.nDirection = rd.IsDBNull(5) ? 0 : rd.GetInt16(5);
                    oTrack.nSpeed = rd.IsDBNull(6) ? 0 : rd.GetByte(6);
                    oTrack.nGSMSignal = rd.IsDBNull(7) ? 0 : rd.GetByte(7);
                    oTrack.nGPSSignal = rd.IsDBNull(8) ? 0 : rd.GetByte(8);
                    oTrack.nFuel = rd.IsDBNull(9) ? 0 : rd.GetInt32(9);
                    oTrack.nMileage = rd.IsDBNull(10) ? 0 : rd.GetInt32(10);
                    oTrack.nTemp = rd.IsDBNull(11) ? 0 : rd.GetInt16(11);
                    oTrack.nCarState = rd.IsDBNull(12) ? 0 : rd.GetInt32(12);
                    oTrack.nTEState = rd.IsDBNull(13) ? 0 : rd.GetInt32(13);
                    oTrack.nAlarmState = rd.IsDBNull(14) ? 0 : rd.GetInt32(14);
                    oTrack.strOther = rd.IsDBNull(15) ? "" : rd.GetString(15);
                    oTrack.strTime = rd.IsDBNull(16) ? "" : rd.GetValue(16).ToString();
                    oTracks.Add(oTrack);
                }
            }
            catch
            {
                return oTracks;
            }
            finally
            {
                if (rd == null)
                { }
                else if (!rd.IsClosed)
                {
                    rd.Close();
                }
                cmd.Dispose();
                con.Close();
            }

            return oTracks;
        }


        public static int Set(string query)
        {
            string GPSDB = System.Configuration.ConfigurationManager.ConnectionStrings["GPSDB"].ConnectionString;
            int nAffected = 0;
            SqlConnection con = null;
            SqlCommand cmd = null;

            con = new SqlConnection(GPSDB);
            cmd = new SqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandText = query;
            try
            {
                nAffected = cmd.ExecuteNonQuery();
            }
            catch { }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            return nAffected;
        }

    }
}
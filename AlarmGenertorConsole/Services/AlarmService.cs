using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using AlarmGenertorConsole.Models;
using System.Data.SqlClient;
//
using System.Data;
using System.Net;
using System.Xml;



namespace AlarmGenertorConsole.Services
{
    public class AlarmService
    {
        // declare database connection
        static string GPSDB = ConfigurationManager.ConnectionStrings["GPSDB"].ConnectionString;
        //string SmsDbConStr = ConfigurationManager.ConnectionStrings["SmsDbConStr"].ConnectionString;
        static string SMSServer = ConfigurationManager.ConnectionStrings["SMSServer"].ConnectionString;

        // Alam SMS
        static string smsSetting = ConfigurationManager.AppSettings["smsSetting"];
        static SendSMS alarmSms;

        public AlarmService()
        {
            if (smsSetting == "1")
            {
                alarmSms = SendSMS.Limited;
            }
            else if (smsSetting == "2")
            {
                alarmSms = SendSMS.Unlimited;
            }
        }

        public static async void Play()
        {
            // get user who are getting sms
            var listCar = Car.Get();

            // fcm user
            var oFcm_User = new Fcm_User();
            var oFcm_Users = oFcm_User.Get(GPSDB);

            // query in Table_TrackLast
            foreach (Car oCar in listCar)
            {
                var oTrackLast = Track.Get(@"SELECT [nID]
                    ,[strTEID]
                    ,[nTime]
                    ,[dbLon]
                    ,[dbLat]
                    ,[nDirection]
                    ,[nSpeed]
                    ,[nGSMSignal]
                    ,[nGPSSignal]
                    ,[nFuel]
                    ,[nMileage]
                    ,[nTemp]
                    ,[nCarState]
                    ,[nTEState]
                    ,[nAlarmState]
                    ,[strOther]
                    ,FORMAT(DATEADD(SS,[nTime],'1970-01-01 06:00:00'), 'h:mm tt yyyy-MM-dd') strTime
                FROM [Table_TrackLast] WHERE [strTEID] = '" + oCar.strTEID + "'").FirstOrDefault();
                var oTrackLastTemp = Track.Get(@"SELECT [nID]
                    ,[strTEID]
                    ,[nTime]
                    ,[dbLon]
                    ,[dbLat]
                    ,[nDirection]
                    ,[nSpeed]
                    ,[nGSMSignal]
                    ,[nGPSSignal]
                    ,[nFuel]
                    ,[nMileage]
                    ,[nTemp]
                    ,[nCarState]
                    ,[nTEState]
                    ,[nAlarmState]
                    ,[strOther]
                    ,FORMAT(DATEADD(SS,[nTime],'1970-01-01 06:00:00'), 'h:mm tt yyyy-MM-dd') strTime
                FROM [index4_TrackLast] WHERE [strTEID] = '" + oCar.strTEID + "'").FirstOrDefault();
                if (oTrackLastTemp == null)
                {
                    var nTrack = Track.Set(@"INSERT INTO [dbo].[index4_TrackLast]
                       ([strTEID]
                       ,[nTime]
                       ,[dbLon]
                       ,[dbLat]
                       ,[nDirection]
                       ,[nSpeed]
                       ,[nGSMSignal]
                       ,[nGPSSignal]
                       ,[nFuel]
                       ,[nMileage]
                       ,[nTemp]
                       ,[nCarState]
                       ,[nTEState]
                       ,[nAlarmState]
                       ,[strOther])
                        VALUES
                       ('" + oTrackLast.strTEID + @"'
                       ,'" + oTrackLast.nTime + @"'
                       ,'" + oTrackLast.dbLon + @"'
                       ,'" + oTrackLast.dbLat + @"'
                       ,'" + oTrackLast.nDirection + @"'
                       ,'" + oTrackLast.nSpeed + @"'
                       ,'" + oTrackLast.nGSMSignal + @"'
                       ,'" + oTrackLast.nGPSSignal + @"'
                       ,'" + oTrackLast.nFuel + @"'
                       ,'" + oTrackLast.nMileage + @"'
                       ,'" + oTrackLast.nTemp + @"'
                       ,'" + oTrackLast.nCarState + @"'
                       ,'" + oTrackLast.nTEState + @"'
                       ,'" + oTrackLast.nAlarmState + @"'
                       ,'" + oTrackLast.strOther + @"')");
                    //
                    oTrackLastTemp = Track.Get(@"SELECT [nID]
                    ,[strTEID]
                    ,[nTime]
                    ,[dbLon]
                    ,[dbLat]
                    ,[nDirection]
                    ,[nSpeed]
                    ,[nGSMSignal]
                    ,[nGPSSignal]
                    ,[nFuel]
                    ,[nMileage]
                    ,[nTemp]
                    ,[nCarState]
                    ,[nTEState]
                    ,[nAlarmState]
                    ,[strOther]
                    ,FORMAT(DATEADD(SS,[nTime],'1970-01-01 06:00:00'), 'h:mm tt yyyy-MM-dd') strTime
                    FROM [index4_TrackLast] WHERE [strTEID] = '" + oCar.strTEID + "'").FirstOrDefault();
                }
                if (oTrackLast != null)
                {
                    //int nSMSNoticeState = oCar.nSMSNoticeState;
                    
                    // user requirement: fench alarm
                    if (GetBit(oCar.nSMSNoticeState, 1))
                    {

                    }

                    // user requirement: door open = 3 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 3))
                    {

                    }

                    // user requirement: main power off = 2 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 2))
                    {
                        // check: Main Power Off 
                        if (GetBit(oTrackLast.nAlarmState, 3) && !GetBit(oTrackLastTemp.nAlarmState, 3))
                        {
                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = "MAIN POWER OFF";
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " is removed at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                if (alarmSms == SendSMS.Limited) { }
                                else // unlimited Alarm SMS
                                {
                                    if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                    {
                                        SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    }
                                    else
                                    {
                                        SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                        SendSms(oCar.strSmsNoticeTel2, bodyMsg);
                                    }
                                }

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;   
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                        , oCar.strTEID, fcmID.strUser);
                                }

                                var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");

                            }
                        }
                        else if (!GetBit(oTrackLast.nAlarmState, 3))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }
                    }
                    
                    // user requirement: ac on = 4 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 4))
                    {
                        // check: AC on is true
                        if (GetBit(oTrackLast.nCarState, 3) && GetBit(oTrackLastTemp.nCarState, 3))
                        {
                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = "AC on";
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " is removed at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                }
                                else
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    SendSms(oCar.strSmsNoticeTel2, bodyMsg);
                                }

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                        , oCar.strTEID, fcmID.strUser);
                                }

                                var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");

                            }
                        }
                        
                        // check: AC on is false in Table_TrackLast
                        if (!GetBit(oTrackLast.nCarState, 3))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }
                    }
                    
                    // user requirement: engine on = 5 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 5))
                    {
                        // check: Engine on is true
                        if (GetBit(oTrackLast.nCarState, 7) && !GetBit(oTrackLastTemp.nCarState, 7))
                        {
                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = "Engine on";
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " is removed at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                }
                                else
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    SendSms(oCar.strSmsNoticeTel2, bodyMsg);
                                }

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                        , oCar.strTEID, fcmID.strUser);
                                }

                                var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                            }
                        }
                        
                        // check: Engine on is false in Table_TrackLast & flag on
                        if (!GetBit(oTrackLast.nCarState, 7))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }
                    }
                    
                    // user requirement: engine off = 3 is false in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 3))
                    {
                        // check: Engine on is true in Table_TrackLast & flag on
                        if (!GetBit(oTrackLast.nCarState, 7) && GetBit(oTrackLastTemp.nCarState, 7))
                        {
                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = "Engine off";
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " is removed at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                }
                                else
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    SendSms(oCar.strSmsNoticeTel2, bodyMsg);
                                }

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                        , oCar.strTEID, fcmID.strUser);
                                }

                                var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                            }

                        }
                        //
                        
                        // check: Engine off is false in Table_TrackLast & flag on
                        if (GetBit(oTrackLast.nCarState, 7))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nCarState] = '" + oTrackLast.nCarState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }
                    }
                    
                    // user requirement: over speed = 6 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 6))
                    {
                        // check: Over speed is true in Table_TrackLast & flag on
                        if (GetBit(oTrackLast.nAlarmState, 6) && !GetBit(oTrackLastTemp.nAlarmState, 6))
                        {
                            // check engine status
                            string engineOnOff = "Engine off";
                            if (GetBit(oTrackLast.nCarState, 7)) // if true engine on
                            {
                                engineOnOff = "Engine on";
                            }

                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = engineOnOff;
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " speed over " + TrackLastByStrTEID(oCar.strTEID, 6) + " km/h at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                if (alarmSms == SendSMS.Limited) { }
                                else // unlimited Alarm SMS
                                {
                                    if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                    {
                                        SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    }
                                    else
                                    {
                                        SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                        SendSms(oCar.strSmsNoticeTel2, bodyMsg);
                                    }
                                }

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;   
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                        , oCar.strTEID, fcmID.strUser);
                                }
                            }

                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");

                        }
                        
                        // check: Over speed is false in Table_TrackLast & flag on
                        if (!GetBit(oTrackLast.nAlarmState, 6))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }

                    }

                    // user requirement: emergency sos = 7 in Table_Alarm
                    if (GetBit(oCar.nSMSNoticeState, 7))
                    {
                        // check: Emergency(SOS) is true in Table_TrackLast & flag on
                        if (GetBit(oTrackLast.nAlarmState, 7) && !GetBit(oTrackLastTemp.nAlarmState, 7))
                        {
                            var geoLocation = await GetGeoLocation(oTrackLast.dbLat.ToString(), oTrackLast.dbLon.ToString());
                            string shortMsg = "Emergeny alarm!";
                            string bodyMsg = "Alarm! " + oTrackLast.strTime + ", Car: " + oCar.strCarNum + ", " + shortMsg + " speed over " + TrackLastByStrTEID(oCar.strTEID, 6) + " km/h at: " + geoLocation;

                            if (oCar.NoticeType == NoticeType.SMS || oCar.NoticeType == NoticeType.FCM || oCar.NoticeType == NoticeType.SMS_FCM)
                            {
                                //BalanceUpdate balanceUpdate = UpdateSmsBalance(Table_Alarm.Flag_Sos, oCar.strTEID, BalanceCondition.MoreThanZero);
                                //if (balanceUpdate == BalanceUpdate.Success) { }

                                if (string.IsNullOrWhiteSpace(oCar.strSmsNoticeTel2))
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                }
                                else
                                {
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                    SendSms(oCar.strSmsNoticeTel, bodyMsg);
                                }

                                // if Balance SMS is 0  // need to modify
                                //balanceUpdate = UpdateSmsBalance(Table_Alarm.Flag_Sos, oCar.strTEID, BalanceCondition.ZeroBalance);
                                //if (balanceUpdate == BalanceUpdate.Success)
                                //{
                                //    SendSms(oCar.strSmsNoticeTel, "Dear client, your SMS balance is 0. for recharge SMS contact with Akash Billing Center.");
                                //}

                                var fcmIDs = oFcm_Users.Where(fu => fu.strTEID == oCar.strTEID).ToList();
                                foreach (var fcmID in fcmIDs)
                                {
                                    //bodyMsg = "" + shortMsg + " at: " + geoLocation;
                                    string str = await SendToFirebase(fcmID.DeviceID, shortMsg, oCar.strCarNum, shortMsg, oCar.strCarNum, bodyMsg
                                                                , oCar.strTEID, fcmID.strTEID);
                                }

                                var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                            }

                        }

                        
                        // check: Emergency(SOS) is false in Table_TrackLast
                        if (!GetBit(oTrackLast.nAlarmState, 7))
                        {
                            var nTrack = Track.Set(@"UPDATE [dbo].[index4_TrackLast]
                                SET = [nAlarmState] = '" + oTrackLast.nAlarmState + "' WHERE strTEID = '" + oTrackLast.strTEID + "'");
                        }

                    }
                }
                
            }

        }

        static bool GetBit(Int32 b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        static bool CheckFlag(string strTEID, Table_Alarm table_Alarm)
        {
            bool bFlag = false;
            int index = Convert.ToInt32(table_Alarm);
            string q = "SELECT [strTEID],[strSmsNoticeTel],[nSMSNoticeState],[BalanceSms],[Flag_MainPowerOff],[Flag_OverSpeed],[Flag_GeoFence],[Flag_Sos],[Flag_EngineOn],[Flag_EngineOff],[Flag_AcOn],[Flag_AcOff] FROM [Table_Alarm] WHERE [strTEID] = '" + strTEID + "'";
            SqlConnection con3 = new SqlConnection();
            SqlCommand cmd3 = new SqlCommand();
            SqlDataReader reader3;
            con3 = new SqlConnection(GPSDB);
            cmd3.Connection = con3;
            con3.Open();
            cmd3.CommandText = q;
            reader3 = cmd3.ExecuteReader();
            reader3.Read();
            if (reader3.HasRows)
            {
                bFlag = reader3.GetBoolean(index);
            }
            reader3.Close();
            cmd3.Dispose();
            con3.Close();

            return bFlag;
        }

        private static async Task<string> GetGeoLocation(string Latitude, string Longitude)
        {
            string myAddress0 = string.Empty;
            string myAddress1 = string.Empty;
            string result = string.Empty;
            string urlAkash = string.Empty;
            string urlOpenstreet = string.Empty;

            //WebClient wc = new WebClient();
            //wc.Encoding = System.Text.Encoding.UTF8;
            WebClient wc = new WebClientWithTimeout();
            wc.Encoding = Encoding.UTF8;

            try
            {
                urlAkash = "http://map.akash.vip/nominatim/reverse?format=xml&lat=" + Latitude + "&lon=" + Longitude + "&addressdetails=1";
                result = await wc.DownloadStringTaskAsync(urlAkash);
            }
            catch
            {
                try
                {
                    urlOpenstreet = "https://nominatim.openstreetmap.org/reverse?format=xml&lat=" + Latitude + "&lon=" + Longitude + "&addressdetails=1";
                    result = await wc.DownloadStringTaskAsync(urlOpenstreet);
                }
                catch
                {
                    result = "Bangladesh";
                    //MessageBox.Show("error!! 363", "Map");
                }
            }

            var address = ""; // return variable
            //parse data from XML doc
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                //Get and display the last item node.
                XmlElement root = xmlDoc.DocumentElement;
                XmlNodeList nodeList;
                nodeList = root.GetElementsByTagName("result");
                address = nodeList.Item(0).InnerXml;
                if (nodeList.Count > 0)
                {
                    address = nodeList.Item(0).InnerXml;
                }
            }
            catch
            {
                address = "Bangladesh";
                //MessageBox.Show("error!! 383", "Map");
            }

            return address;
        }

        private static bool SendSms(string mobile, string message)
        {
            // send sms via SMSServer.MessageOut
            SqlConnection con2 = new SqlConnection();
            SqlCommand cmd2 = new SqlCommand();
            con2 = new SqlConnection(SMSServer);
            cmd2.Connection = con2;
            con2.Open();
            cmd2.CommandText = "INSERT INTO [MessageOut](MessageTo, MessageFrom, MessageText) VALUES(@MessageTo, @MessageFrom, @MessageText)";
            cmd2.Parameters.Add(new SqlParameter("MessageTo", mobile));
            cmd2.Parameters.Add(new SqlParameter("MessageFrom", "alertserve"));
            cmd2.Parameters.Add(new SqlParameter("MessageText", message));
            cmd2.ExecuteNonQuery();
            cmd2.Dispose();
            con2.Close();

            return false;
        }

        private static int UpdateFlag(Table_Alarm columnName, string strTEID, FlagSMS flagSms)
        {
            int nBalanceSms = 0;
            string query = "";
            bool flag = Convert.ToBoolean(flagSms);
            query = "UPDATE Table_Alarm SET " + columnName + " = @param WHERE [strTEID] = @strTEID";

            SqlConnection con2 = new SqlConnection();
            SqlCommand cmd2 = new SqlCommand();
            con2 = new SqlConnection(GPSDB);
            cmd2.Connection = con2;
            con2.Open();
            cmd2.CommandText = query;
            cmd2.Parameters.Add(new SqlParameter("param", flag));
            cmd2.Parameters.Add(new SqlParameter("strTEID", strTEID));
            nBalanceSms = cmd2.ExecuteNonQuery();
            cmd2.Dispose();
            con2.Close();

            return nBalanceSms;
        }

        private BalanceUpdate UpdateSmsBalance(Table_Alarm columnName, string strTEID, BalanceCondition balanceCondition)
        {
            BalanceUpdate balanceUpdate = BalanceUpdate.Fail;
            int nBalanceStatus;
            string query = "";
            if (balanceCondition == BalanceCondition.MoreThanZero)
            {
                query = "UPDATE Table_Alarm SET [BalanceSms] = [BalanceSms] - 1 WHERE [strTEID] = @strTEID  AND BalanceSms > 0";
            }
            else if (balanceCondition == BalanceCondition.ZeroBalance)
            {
                query = "UPDATE Table_Alarm SET [BalanceSms] = [BalanceSms] - 1 WHERE [strTEID] = @strTEID  AND BalanceSms = 0";
            }

            SqlConnection con2 = new SqlConnection();
            SqlCommand cmd2 = new SqlCommand();
            con2 = new SqlConnection(GPSDB);
            cmd2.Connection = con2;
            con2.Open();
            cmd2.CommandText = query;
            cmd2.Parameters.Add(new SqlParameter("strTEID", strTEID));
            nBalanceStatus = cmd2.ExecuteNonQuery();
            cmd2.Dispose();
            con2.Close();

            if (nBalanceStatus > 0)
            {
                balanceUpdate = BalanceUpdate.Success;
            }

            return balanceUpdate;
        }

        //SendNotificationFromFirebaseCloud
        static async Task<String> SendToFirebase(string to
            , string ShortDesc
            , string IncidentNo
            , string Description
            , string title
            , string text
            , string strTEID
            , string strUser)
        {
            //cowntgz1BPo:APA91bHQsYVpwRW-lsAg9kvsa7C9f9hcBCCQv7XKcTD4h50wWVp8MaoQyIt5NRyde3SGf1k9N0OsaBSkpcTcQOyqu98KLRO0xnSSmMkYIbN7zZkc4J67EWHad6hMdYnHEyMKlXOeEUo4
            var result = "-1";
            var webAddr = "https://fcm.googleapis.com/fcm/send";

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "key=AAAAiFkQFfU:APA91bHBu4XNp8udWzDIxj25i1Jv2iQ2iPEWEN1O_zuRa_ZvIkXGQfMU_n1oqU0oHgXyxngEVFiquTu0xQr9U9IqDHQsKb1AHYChZ8yf4MSFTXeD0Sjd3pmRUaBjG2GnbxoPR7mj1N3R");
                httpWebRequest.Method = "POST";
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string strNJson = @"{
                    ""to"": """ + to + @""",
                    ""data"": {
                        ""ShortDesc"": """ + ShortDesc + @""",
                        ""IncidentNo"": """ + IncidentNo + @""",
                        ""Description"": """ + Description + @"""
                          },
                          ""notification"": {
                                        ""title"": """ + title + @""",
                            ""text"": """ + text + @""",
                        ""sound"":""default""
                          }
                                }";
                    streamWriter.Write(strNJson);
                    streamWriter.Flush();
                }

                //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var httpResponse = await httpWebRequest.GetResponseAsync();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    result = await streamReader.ReadToEndAsync();
                }

                // add a log in database
                SendFcmMsgOut(to, ShortDesc, IncidentNo, Description, title, text, strTEID, strUser);

            }
            catch
            {
                //MessageBox.Show("error!! 626", "Fireabase");
            }
            return result;
        }

        static bool SendFcmMsgOut(string to
            , string ShortDesc
            , string IncidentNo
            , string Description
            , string title
            , string text
            , string strTEID
            , string strUser)
        {
            string insert = @"INSERT INTO [Fcm_UserMsgOut]
                       ([strTEID]
                       ,[strUser]
                       ,[Message]
                       ,[MessageTime])
                 VALUES
                       ('" + strTEID + @"'
                       ,'" + strUser + @"'
                       ,N'Car: " + title + @" " + text + @"'
                       ,'" + DateTime.Now + "')";
            var db = new Services.MsSqlService();
            var isSaved = db.Set("GpsDbConStr", insert);
            return isSaved;
        }

        private static string TrackLastByStrTEID(string strTEID, int index)
        {
            string result = "N/A";
            SqlConnection con3 = new SqlConnection();
            SqlCommand cmd3 = new SqlCommand();
            SqlDataReader reader3;
            con3 = new SqlConnection(GPSDB);
            cmd3.Connection = con3;
            con3.Open();
            cmd3.CommandText = "SELECT [nID],[strTEID],[nTime],[dbLon],[dbLat],[nDirection],[nSpeed],[nGSMSignal],[nGPSSignal],[nFuel],[nMileage],[nTemp],[nCarState],[nTEState],[nAlarmState] FROM [Table_TrackLast]"
                + " WHERE [strTEID] = '" + strTEID + "'";
            reader3 = cmd3.ExecuteReader();
            reader3.Read();
            if (reader3.HasRows)
            {
                result = reader3.GetValue(index).ToString();
            }
            reader3.Close();
            cmd3.Dispose();
            con3.Close();

            return result;
        }

    }
}

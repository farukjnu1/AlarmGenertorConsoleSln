using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmGenertorConsole.Models
{
    public class Fcm_UserMsgOut
    {
        public Int64  nID { get; set; }
        public string strTEID { get; set; }
        public string strUser { get; set; }
        public string Message { get; set; }
        public DateTime MessageTime { get; set; }

    }

}

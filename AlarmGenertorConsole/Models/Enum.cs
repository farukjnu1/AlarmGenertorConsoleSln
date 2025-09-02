using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmGenertorConsole.Models
{
    public class Enum
    {

    }

    public enum FlagSMS 
    {
        Off = 0,
        On = 1
    }

    public enum SendSMS 
    {
        Limited = 1,
        Unlimited = 2
    }

    public enum Table_Alarm
    {
        strTEID = 0,
        strSmsNoticeTel = 1,
        nSMSNoticeState = 2,
        BalanceSms = 3,
        Flag_MainPowerOff = 4,
        Flag_OverSpeed = 5,
        Flag_GeoFence = 6,
        Flag_Sos = 7,
        Flag_EngineOn = 8,
        Flag_EngineOff = 9,
        Flag_AcOn = 10,
        Flag_AcOff = 11
    }

    public enum BalanceCondition
    {
        ZeroBalance = 0,
        MoreThanZero = 1
    }

    public enum BalanceUpdate 
    {
        Success = 1,
        Fail = 2
    }

    public enum NoticeType
    {
        SMS = 1,
        FCM = 2,
        SMS_FCM = 3
    }

}

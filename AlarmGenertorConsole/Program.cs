using AlarmGenertorConsole.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//
using System.Configuration;
using AlarmGenertorConsole.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Xml;


namespace AlarmGenertorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a Timer object that knows to call our TimerCallback
            // method once every 2000 milliseconds.
            Timer t = new Timer(TimerCallback, null, 0, 2000);
            // Wait for the user to hit <Enter>
            Console.ReadLine();
        }

        private static void TimerCallback(Object o)
        {
            // Display the date/time when this method got called.
            //Console.WriteLine("In TimerCallback: " + DateTime.Now);

            AlarmService.Play();

            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }

    }
}
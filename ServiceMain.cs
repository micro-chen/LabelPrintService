using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using NetworkStatusService;

namespace LabelPrintService
{
    public partial class PrintService : ServiceBase
    {
        private Timer TTTimer;

        public PrintService()
        {
            InitializeComponent();

            if (!System.Diagnostics.EventLog.SourceExists("PrintServiceLogSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("PrintServiceLogSource", "PrintServiceLog");
            }

            eventLog1.Source = "PrintServiceLogSource";
            eventLog1.Log = "PrintServiceLog";

            this.TTTimer = new Timer();

            this.TTTimer.Interval = 5000;
            this.TTTimer.Elapsed += new ElapsedEventHandler(TTTimer_Elapsed);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Print Service Started");
            this.TTTimer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Print Service Stopped");
            this.TTTimer.Stop();
        }

        void TTTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //eventLog1.WriteEntry("Tick Generated");
            
        }

        /// <summary>
        /// Event handler used to capture availability changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        static void DoAvailabilityChanged(
            object sender, NetworkStatusChangedArgs e)
        {
            ReportAvailability();
        }


        /// <summary>
        /// Report the current network availability.
        /// </summary>

        private static void ReportAvailability()
        {
            if (NetworkStatus.IsAvailable)
            {
                eventLog1.WriteEntry("Network is available");
            }
            else
            {
                eventLog1.WriteEntry("Network is not available");
            }
        }
    }
}

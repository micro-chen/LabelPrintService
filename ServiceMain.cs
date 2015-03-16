using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using SysTimers = System.Timers;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing.Printing;
using NetworkStatusService;
using ZPLEncodingLibrary;
using RawPrinterLibrary;

namespace LabelPrintService
{
    public partial class PrintService : ServiceBase
    {
        private SysTimers.Timer tTimer;
        private int eventId;

        public PrintService()
        {
            InitializeComponent();
            eventId = 1;

            if (!System.Diagnostics.EventLog.SourceExists("LabelPrintServiceSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("LabelPrintServiceSource", "LabelPrintServiceLog");
            }

            eventLog1.Source = "LabelPrintServiceSource";
            eventLog1.Log = "LabelPrintServiceLog";

            this.tTimer = new SysTimers.Timer();

            this.tTimer.Interval = 10000;
            this.tTimer.Elapsed += new SysTimers.ElapsedEventHandler(this.OnTimer);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Label Print Service is Started");
            this.tTimer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Label Print Service is Stopped");
            this.tTimer.Stop();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Label Print Service is trigger", EventLogEntryType.Information, 10001);

            StringBuilder sBuffer = new StringBuilder();

            try
            {
                LabelPrintDataSetTableAdapters.LabelPrintJobTableAdapter jta =
                    new LabelPrintDataSetTableAdapters.LabelPrintJobTableAdapter();
                LabelPrintDataSet.LabelPrintJobDataTable dt = jta.GetPrintJob();

                //Queue<ProductLabel> qLabel = new Queue<ProductLabel>();

                foreach (LabelPrintDataSet.LabelPrintJobRow row in dt)
                {
                    eventLog1.WriteEntry("Line 1: " + row.Line1 + "\nLine 2: " + row.Line2 + "\nQuantity: " + row.Quantity,
                        EventLogEntryType.Information, 11000 + eventId++);
                    ProductLabel tLabel = new ProductLabel();
                    tLabel.ProductCode = row.Line1;
                    tLabel.ProductName = row.Line2;
                    tLabel.Quantity = row.Quantity;
                    //qLabel.Enqueue(tLabel);
                    sBuffer.Append(tLabel.getRawByte());

                    jta.DeletePrintJob(row.PrintId);
                }

                /*
                foreach (ProductLabel tLabel in qLabel)
                {   
                    eventLog1.WriteEntry("Raw: \n" + tLabel.getRawByte() + "\n");
                }

                qLabel.Clear();
                */

                // Allow the user to select a printer.
                /*PrintDialog pd = new PrintDialog();
                pd.PrinterSettings = new PrinterSettings();
                if (DialogResult.OK == pd.ShowDialog(this))
                {
                    // Send a printer-specific to the printer.
                    RawPrinterHelper.SendStringToPrinter(pd.PrinterSettings.PrinterName, sBuffer);
                }*/
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    eventLog1.WriteEntry("Printer Name: " + printerName, EventLogEntryType.Information, 12000 + eventId++);
                    if (printerName.Contains("Z4M") && sBuffer.Length > 0)
                    {
                        // Send a printer-specific to the printer.
                        if (RawPrinterHelper.SendStringToPrinter(printerName, sBuffer.ToString()))
                        {
                            eventLog1.WriteEntry("Sent to " + printerName, EventLogEntryType.Information, 12100 + eventId++);
                        }      

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message, EventLogEntryType.Error, 90001);
            }
            finally
            {
                sBuffer.Clear();
            }
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

        }
    }
}

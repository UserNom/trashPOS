using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.PointOfService;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace SelfHostedWebScanner
{
    public class Printer
    {
        
        int timeout = 500;
        public  PosPrinter printer{get;private set;}
		public DeviceInfo deviceInfo { get; private set; }

		private IHubContext _context
		{
			get
			{
				return GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			}
		}

        


        public Printer(DeviceInfo dInfo)
        {
            //this._context = hubContext;

            PosExplorer posExplorer = new PosExplorer();


			deviceInfo = dInfo;
			/*posExplorer.GetDevice(DeviceType.PosPrinter, "Zebra QL 320");*/
            printer = (PosPrinter)posExplorer.CreateInstance(deviceInfo);
			try
			{
				printer.Open();
			}
			catch (PosControlException e) {
				//e.
				throw (e);
			}
            
            _context.Clients.All.broadcastMessage("Printer", "state: " + printer.State);
            //Thread.Sleep(timeout);

        }

        /// <summary designerfunction="true">
        /// I believe this function prints text. Try it and see... 
        /// </summary>
        /// <param name="text">That's the text you want to print.</param>
        /// 
        public void printText(String text)
        {
            lock (printer)
            {
                printer.Claim(1000);
                Thread.Sleep(timeout);
                printer.DeviceEnabled = true;
                printer.AsyncMode = false;
                printer.MapMode = MapMode.Dots;
                Thread.Sleep(timeout);
                printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine);
				printer.DeviceEnabled = false;
				printer.Release();
            }
        }

		public void printReceipt(String text, String orderNo)
		{
			lock (printer)
			{
				// https://msdn.microsoft.com/en-us/library/microsoft.pointofservice.posprinter.aspx
				// See JrnLineChars? https://msdn.microsoft.com/en-US/library/microsoft.pointofservice.posprinter_members%28v=winembedded.11%29.aspx
				string ESC = System.Text.ASCIIEncoding.ASCII.GetString(new byte[] { 27 });
				printer.Claim(1000);
				Thread.Sleep(timeout);
				printer.DeviceEnabled = true;
				printer.AsyncMode = false;
				printer.MapMode = MapMode.Dots;
				Thread.Sleep(timeout);
				printer.TransactionPrint(PrinterStation.Receipt, PrinterTransactionControl.Transaction);
				if (printer.CapRecBitmap)
				{
					printer.PrintBitmap(PrinterStation.Receipt, "C:\\Users\\SysUser\\Desktop\\w3.bmp", PosPrinter.PrinterBitmapAsIs, PosPrinter.PrinterBitmapCenter);
				}
				printer.PrintNormal(PrinterStation.Receipt, ESC+"4C"+ " Order "+orderNo + Environment.NewLine);
				printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine);
				if (printer.CapRecBarCode)
				{
					printer.PrintBarCode(PrinterStation.Receipt, orderNo, BarCodeSymbology.Ean128, 100, 100, PosPrinter.PrinterBarCodeCenter, BarCodeTextPosition.Below);
				}
				printer.PrintNormal(PrinterStation.Receipt, "\r\n\r\n\r\n\r\n\r\n");
				printer.TransactionPrint(PrinterStation.Receipt, PrinterTransactionControl.Normal);

				printer.DeviceEnabled = false;
				printer.Release();
			}
		}

        public void printBarcode(String text)
        {
            lock (printer)
            {
                printer.Claim(1000);
                Thread.Sleep(timeout);
                printer.DeviceEnabled = true;
                printer.AsyncMode = false;
                printer.MapMode = MapMode.Dots;
                Thread.Sleep(timeout);
                printer.PrintBarCode(PrinterStation.Receipt, text, BarCodeSymbology.Ean128, 100, 100, PosPrinter.PrinterBarCodeCenter, BarCodeTextPosition.Below);
				printer.DeviceEnabled = false;
				printer.Release();
            }
        }

        public void printLogo()
        {
            lock (printer)
            {
                printer.Claim(1000);
                Thread.Sleep(timeout);
                printer.DeviceEnabled = true;
                printer.AsyncMode = false;
                printer.MapMode = MapMode.Dots;
                Thread.Sleep(timeout);
                printer.PrintBitmap(PrinterStation.Receipt, "C:\\Users\\SysUser\\Desktop\\w3.bmp", PosPrinter.PrinterBitmapAsIs, PosPrinter.PrinterBitmapCenter);
				printer.DeviceEnabled = false;
				printer.Release();
            }
        }


        public void getStatus(string connectionId)
        {
            _context.Clients.Client(connectionId).broadcastMessage("Printer", "state: " + printer.State);
        }

		public void close()
		{
			try
			{
				printer.DeviceEnabled = false;
			}
			catch(PosControlException e) {
				Console.WriteLine("Error when setting printer.DeviceEnabled = false: " + e.Message);
			}
			try
			{
				printer.Release();
			}
			catch (PosControlException e) {
				Console.WriteLine("Error when releasing printer: " + e.Message);
			}
			try
			{
				printer.Close();
			}
			catch (PosControlException e){
				Console.WriteLine("Error when closing printer: " + e.Message);
			}
			finally
			{
				printer = null;
			}
		}
    }
}

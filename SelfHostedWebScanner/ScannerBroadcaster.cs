using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.PointOfService;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace SelfHostedWebScanner
{
	public class ScannerBroadcaster
	{


		public Scanner scanner { get; private set; }
		public DeviceInfo deviceInfo { get; private set; }

		private IHubContext _context
		{
			get
			{
				return GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			}
		}



		public void getStatus(string connectionId)
		{
			_context.Clients.Client(connectionId).broadcastMessage("Scanner", "status: " + scanner.State);
		}

		public ScannerBroadcaster(DeviceInfo dInfo)
		{

			

			PosExplorer posExplorer = new PosExplorer();
			deviceInfo = dInfo;
			try
			{
				scanner = (Scanner)posExplorer.CreateInstance(deviceInfo);

				scanner.Open();
				scanner.Claim(0);
				scanner.DeviceEnabled = true;
				scanner.DataEventEnabled = true;
				scanner.DecodeData = true;
				scanner.DataEvent += new DataEventHandler(onDataEventHandler);
				//You need this or data events are disabled on a bad read (whatever that is) 
				//and the scanner does nothing until you reopen it.
				// https://msdn.microsoft.com/en-US/library/microsoft.pointofservice.scanner%28v=winembedded.4%29.aspx
				scanner.ErrorEvent += (x, e) => {scanner.DataEventEnabled = true; };

			}
			catch(PosControlException e)
			{
				throw e;
			}
			_context.Clients.All.broadcastMessage("Scanner", "status: " + scanner.State);


		}


		private void onDataEventHandler(object sender, DataEventArgs e)
		{
			// http://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-server#callfromoutsidehub
			String label = System.Text.Encoding.Default.GetString(((Scanner)sender).ScanDataLabel);
			_context.Clients.All.broadcastMessage("Scanner", "Read label: " + label);
			_context.Clients.All.barcodeRead(label);
			((Scanner)sender).DataEventEnabled = true;

		}

		public void close()
		{
			try
			{
				scanner.Release();
				scanner.Close();
			}
			catch(PosControlException e){
				Console.WriteLine("Error releasing scanner: "+e.Message);
			}
			try
			{
				scanner.Close();
			}
			catch(PosControlException e){
				Console.WriteLine("Error closing scanner: "+e.Message);
			}
			finally
			{
				scanner = null;
			}

		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.PointOfService;
using System.Threading.Tasks;

namespace SelfHostedWebScanner
{
	public class PosHub : Hub
	{
		//singleton...
		//init the handlers (posManager.onDeviceAddedHandlers...) once only...
		static PosManager posManager = PosManager.Instance;
		
		public PosHub(){
			

			//Make sure we are only subscribed once or it will fire more than once.
			posManager.DeviceConnected -= onDeviceConnected;
			posManager.DeviceConnected += onDeviceConnected;

			posManager.DeviceDisconnected -= onDeviceDisconnected;
			posManager.DeviceDisconnected += onDeviceDisconnected;

			posManager.PosError -= onPosError;
			posManager.PosError += onPosError;

			
		}

		public DeviceCollection getScanners()
		{
			return posManager.scanners;
		}

		//Using DeviceInfoWrapper because you can't deserialise to DeviceInfo
		public void claimScanner(DeviceInfoWrapper i)
		{
			if (i == null)
			{
				posManager.releaseScanner();
				return;
			}
			DeviceInfo device=i.GetMatchingDeviceInfoFromCollection(posManager.scanners);
			if (device == null)
			{
				//error, do something
			}
			else
			{
				posManager.claimScanner(device);
			}
			
		}

		public DeviceCollection getPrinters()
		{
			return posManager.printers;
		}

		public void claimPrinter(DeviceInfoWrapper i)
		{
			if (i == null)
			{
				posManager.releasePrinter();
				return;
			}
			DeviceInfo device = i.GetMatchingDeviceInfoFromCollection(posManager.printers);
			if (device == null)
			{
				//error, do something
			}
			else
			{
				posManager.claimPrinter(device);
			}
			
		}

		public void GetStatus()
		{
			Clients.All.updatePrinterList(posManager.printers, false);
			Clients.All.updateScannerList(posManager.scanners, false);
			if (posManager.selectedPrinter != null)
				posManager.selectedPrinter.getStatus(Context.ConnectionId);
			else
				Clients.Caller.broadcastMessage("Printer", "No printer selected");

			if (posManager.selectedScanner != null)
				posManager.selectedScanner.getStatus(Context.ConnectionId);
			else
				Clients.Caller.broadcastMessage("Scanner", "No scanner selected");
		}

		public void Print(string message)
		{
			if (posManager.selectedPrinter!=null) 
				posManager.selectedPrinter.printText(message);
		}

		public void PrintReceipt(string receipt, string orderNo)
		{
			if (posManager.selectedPrinter != null)
				posManager.selectedPrinter.printReceipt(receipt, orderNo);
		}

		public override Task OnConnected()
		{
			GetStatus();
			return base.OnConnected();
		}

		public override Task OnReconnected()
		{
			GetStatus();
			return base.OnReconnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			posManager.releaseDevices();
			return base.OnDisconnected(stopCalled);
		}


		public static void onDeviceConnected(DeviceInfo deviceInfo)
		{
			IHubContext ctx = GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			if (deviceInfo.Type == DeviceType.PosPrinter)
			{
				ctx.Clients.All.broadcastMessage("Printer", "New printer found");
				ctx.Clients.All.updatePrinterList(posManager.printers, false); 
			}
			if (deviceInfo.Type == DeviceType.Scanner)
			{
				ctx.Clients.All.broadcastMessage("Scanner", "New scanner found");
				ctx.Clients.All.updateScannerList(posManager.scanners, false); 
			}
		}

		public static void onDeviceDisconnected(DeviceInfo deviceInfo, Boolean wasClaimed)
		{
			IHubContext ctx = GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			if (deviceInfo.Type == DeviceType.PosPrinter)
			{
				ctx.Clients.All.broadcastMessage("Printer", "A printer was removed");
				ctx.Clients.All.updatePrinterList(posManager.printers, wasClaimed); 
			}
			if (deviceInfo.Type == DeviceType.Scanner)
			{
				ctx.Clients.All.broadcastMessage("Scanner", "A scanner was removed");
				ctx.Clients.All.updateScannerList(posManager.scanners, wasClaimed);
			}
		}

		public static void onPosError(String m)
		{
			IHubContext ctx = GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			ctx.Clients.All.broadcastAlert(m);
		}

	}
}

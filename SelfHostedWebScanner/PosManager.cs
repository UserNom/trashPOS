using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PointOfService;
using Microsoft.AspNet.SignalR;

namespace SelfHostedWebScanner
{
	class PosManager
	{
		PosExplorer posExplorer;
		public DeviceCollection printers, scanners;

		public Printer selectedPrinter { get; private set;}
		public ScannerBroadcaster selectedScanner { get; private set; }
		
		private static PosManager _instance;
		public static PosManager Instance { 
			get { 
				if(_instance==null)
					_instance=new PosManager();
				return _instance;
			}
		}

		private PosManager()
		{
			posExplorer = new PosExplorer();
			posExplorer.DeviceAddedEvent += new DeviceChangedEventHandler(onDeviceAdded);
			posExplorer.DeviceRemovedEvent += new DeviceChangedEventHandler(onDeviceRemoved);

			printers = posExplorer.GetDevices(DeviceType.PosPrinter);
			scanners = posExplorer.GetDevices(DeviceType.Scanner);
		}

		public void refreshDevices()
		{
			printers = posExplorer.GetDevices(DeviceType.PosPrinter);
			scanners = posExplorer.GetDevices(DeviceType.Scanner);
		}


		public void claimPrinter(DeviceInfo info)
		{
			releasePrinter();
			
			if (info == null || info.Type != DeviceType.PosPrinter)
			{
				//something went wrong... throw an exception?
				throw new Exception("Unable to claim selected printer.");
			}
			try
			{
				selectedPrinter=new Printer(info);
			}
			catch (PosControlException e)
			{
				Console.WriteLine("PosManager.claimPrinter failed: "+e.Message);
				OnPosError("Unable to connect to printer: " + e.Message);
			}
		}

		public void claimScanner(DeviceInfo info)
		{
			releaseScanner();
			
			if (info == null || info.Type!=DeviceType.Scanner)
			{
				//something went wrong... throw an exception?
				throw new Exception("Unable to claim selected scanner.");
			}
			try
			{
				selectedScanner = new ScannerBroadcaster(info);
			}
			catch (PosControlException e)
			{
				Console.WriteLine("PosManager.claimScanner failed: "+e.Message);
				OnPosError("Unable to connect to scanner: "+e.Message);
			}
		}

		public void releaseDevices(){
			releasePrinter();
			releaseScanner();
		}

		public void releaseScanner()
		{
			if (selectedScanner != null)
			{
				try
				{
					selectedScanner.close();
				}
				catch (PosControlException e) { }
				finally
				{
					selectedScanner = null;
				}
			}
		}

		public void releasePrinter()
		{
			if (selectedPrinter != null)
			{
				try
				{
					selectedPrinter.close();
				}
				catch (PosControlException e) { }
				finally
				{
					selectedPrinter = null;
				}
				
			}
		}

		void onDeviceAdded(object sender, DeviceChangedEventArgs e)
		{
			refreshDevices();
			OnDeviceConnected(e.Device);
		}

		void onDeviceRemoved(object sender, DeviceChangedEventArgs e)
		{
			refreshDevices();
			Boolean wasClaimed = false;
			if (selectedScanner!=null && e.Device.IsDeviceInfoOf(selectedScanner.scanner))
			{
				releaseScanner();
				wasClaimed = true;
			}
			if (selectedPrinter!=null && e.Device.IsDeviceInfoOf(selectedPrinter.printer))
			{
				releasePrinter();
				wasClaimed = true;
			}
			refreshDevices();

			OnDeviceDisconnected(e.Device, wasClaimed);
		}


		//Device connected/disconnected events for the hub or other classes to listen to.
		public event DeviceDisconnectedEventHandler DeviceDisconnected;
		protected virtual void OnDeviceDisconnected(DeviceInfo d, Boolean wasClaimed)
		{
			if(DeviceDisconnected!=null)
				DeviceDisconnected(d,wasClaimed);
		}
		public delegate void DeviceDisconnectedEventHandler(DeviceInfo info, Boolean wasClaimed);

		
		public event DeviceConnectedEventHandler DeviceConnected;
		protected virtual void OnDeviceConnected(DeviceInfo d)
		{
			if (DeviceConnected != null)
				DeviceConnected(d);
		}
		public delegate void DeviceConnectedEventHandler(DeviceInfo info);


		public event PosErrorEventHandler PosError;
		protected virtual void OnPosError(String message)
		{
			PosError(message);
		}
		public delegate void PosErrorEventHandler(String message);
		
	}
}

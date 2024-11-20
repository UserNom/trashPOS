using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.PointOfService;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace SelfHostedWebScanner
{
	class PinPadDevice
	{
		public PinPad pinpad { get; private set; }
		public DeviceInfo deviceInfo { get; private set; }

		private IHubContext _context
		{
			get
			{
				return GlobalHost.ConnectionManager.GetHubContext<PosHub>();
			}
		}

		public PinPadDevice(DeviceInfo dInfo)
		{
			PosExplorer posExplorer = new PosExplorer();
			deviceInfo = dInfo;
			pinpad = (PinPad)posExplorer.CreateInstance(deviceInfo);
			try
			{
				pinpad.Open();
			}
			catch (PosControlException e)
			{
				throw (e);
			}

			_context.Clients.All.broadcastMessage("PinPad", "state: " + pinpad.State);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PointOfService;

namespace SelfHostedWebScanner
{
	

	public  class DeviceInfoWrapper
	{

		public  DeviceCompatibilities Compatibility { get; set; }

		public  string Description { get; set; }

		public  string HardwareDescription { get; set; }

		public  string HardwareId { get; set; }

		public  string HardwarePath { get; set; }

		public  bool IsDefault { get; set; }

		public  string[] LogicalNames { get; set; }

		public  string ManufacturerName { get; set; }

		public  string ServiceObjectName { get; set; }

		public  FakeVersion ServiceObjectVersion { get; set; }

		public  string Type { get; set; }

		public  FakeVersion UposVersion { get; set; }

		/// <summary>
		/// Gets the Boolean value that indicates wether a DeviceInfo instance applies to the specified service object
		/// </summary>
		/// <param name="serviceObject"></param>
		/// <returns></returns>
		public Boolean IsDeviceInfoOf(PosCommon serviceObject)
		{
			return getDeviceInfo().IsDeviceInfoOf(serviceObject);
		}

		/// <summary>
		/// Gets the matching DeviceInfo instance from the specified DeviceCollection.
		/// Returns null if none is found.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public DeviceInfo GetMatchingDeviceInfoFromCollection(DeviceCollection c)
		{
			DeviceInfo i=null;
			PosExplorer posExplorer=new PosExplorer();
			foreach(DeviceInfo dev in c){
				PosCommon tempInstance = (PosCommon)posExplorer.CreateInstance(dev);
				if (IsDeviceInfoOf(tempInstance)){
					i=dev;
					break;
				}
			}
			return i;
		}

		private DeviceInfo getDeviceInfo() { 
			if(concreteDeviceInfo==null){
				concreteDeviceInfo=new ConcreteDeviceInfo(this);
			}
			return concreteDeviceInfo;
		}

		private ConcreteDeviceInfo concreteDeviceInfo;

		public class FakeVersion
		{
			public int Build { get; set; }
			public int Major { get; set; }
			public int Minor { get; set; }
			public int Revision { get; set; }
			public Version getVersion()
			{
				if (v == null)
				{
					v = new Version(Major, Minor, Build, Revision);
				}
				return v;
			}
			private Version v;
		}

		private class ConcreteDeviceInfo : DeviceInfo
		{
			public ConcreteDeviceInfo(DeviceInfoWrapper s)
			{
				this.s = s;
			}
			DeviceInfoWrapper s;

			public override DeviceCompatibilities Compatibility
			{
				get { return s.Compatibility; }
			}

			public override string Description
			{
				get { return s.Description; }
			}

			public override string HardwareDescription
			{
				get { return s.HardwareDescription; }
			}

			public override string HardwareId
			{
				get { return s.HardwareId; }
			}

			public override string HardwarePath
			{
				get { return s.HardwarePath; }
			}

			public override bool IsDefault
			{
				get { return s.IsDefault; }
			}

			public override string[] LogicalNames
			{
				get { return s.LogicalNames; }
			}

			public override string ManufacturerName
			{
				get { return s.ManufacturerName; }
			}

			public override string ServiceObjectName
			{
				get { return s.ServiceObjectName; }
			}

			public override Version ServiceObjectVersion
			{
				get { return s.ServiceObjectVersion.getVersion(); }
			}

			public override string Type
			{
				get { return s.Type; }
			}

			public override Version UposVersion
			{
				get { return s.UposVersion.getVersion(); }
			}


		}
	}

	
	
}

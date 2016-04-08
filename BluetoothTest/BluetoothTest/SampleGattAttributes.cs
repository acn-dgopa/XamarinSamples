
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BluetoothTest
{

	/**
 	* This class includes a small subset of standard GATT attributes for demonstration purposes.
 	*/

			
	public class SampleGattAttributes
	{
		public static String HEART_RATE_MEASUREMENT = "00002a37-0000-1000-8000-00805f9b34fb";
		public static String CLIENT_CHARACTERISTIC_CONFIG = "00002902-0000-1000-8000-00805f9b34fb";
		public static String HEALTH_THERMO_MEASUREMENT ="00002a1c-0000-1000-8000-00805f9b34fb";

		private static Dictionary <String, String> Attributes = new Dictionary <String, String> ()
		{
			// Sample Services.
			{"0000180d-0000-1000-8000-00805f9b34fb", "Heart Rate Service"},
			{"0000180a-0000-1000-8000-00805f9b34fb", "Device Information Service"},
			{"00001800-0000-1000-8000-00805f9b34fb", "Blue Gecko"},
			{"00001803-0000-1000-8000-00805f9b34fb", "Link Loss"},
			{"00001802-0000-1000-8000-00805f9b34fb", "Immediate Alert"},
			{"00001804-0000-1000-8000-00805f9b34fb", "Power"},
			{"00001809-0000-1000-8000-00805f9b34fb", "Health Thermometer Service"},

			// Sample Characteristics.
			{HEART_RATE_MEASUREMENT, "Heart Rate Measurement"},
			{"00002a29-0000-1000-8000-00805f9b34fb", "Manufacturer Name String"},
			{"00002a24-0000-1000-8000-00805f9b34fb", "Model Number"},
			{"00002a23-0000-1000-8000-00805f9b34fb", "System ID"},
			{"00002a00-0000-1000-8000-00805f9b34fb", "Device Name"},
			{"00002a01-0000-1000-8000-00805f9b34fb", "Appearance"},
			{"00002a06-0000-1000-8000-00805f9b34fb", "Alert Level"},
			{"00002a07-0000-1000-8000-00805f9b34fb", "Power Level"},
			{HEALTH_THERMO_MEASUREMENT, "Health Thermometer Measurement"},
		};

		public static String Lookup (String key, String defaultName) 
		{
			String name = Attributes [key];
			return name == null ? defaultName : name;
		}
	}
}


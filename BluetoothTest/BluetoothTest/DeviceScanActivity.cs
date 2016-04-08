
using System;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System.Collections.Generic;
using System.Threading;

namespace BluetoothTest
{
	[Activity (Label = "BLE sample", MainLauncher=true)]			
	public class DeviceScanActivity : ListActivity, BluetoothAdapter.ILeScanCallback
	{
		LeDeviceListAdapter leDeviceListAdapter;
		BluetoothAdapter bluetoothAdapter;
		bool scanning;
		Handler handler;

		static readonly int REQUEST_ENABLE_BT = 1;
		// Stops scanning after 10 seconds.
		static readonly long SCAN_PERIOD = 10000;

		protected override void OnCreate (Bundle savedInstanceState)
		{

			base.OnCreate (savedInstanceState);

			Window.RequestFeature(WindowFeatures.ActionBar);

			ActionBar.SetTitle (Resource.String.title_devices);

			handler = new Handler ();

			// Use this check to determine whether BLE is supported on the device.  Then you can
			// selectively disable BLE-related features.
			if (!PackageManager.HasSystemFeature (Android.Content.PM.PackageManager.FeatureBluetoothLe)) 
			{
				Toast.MakeText (this, Resource.String.ble_not_supported, ToastLength.Short).Show ();
				Finish ();
			}


			// Initializes a Bluetooth adapter.  For API level 18 and above, get a reference to
			// BluetoothAdapter through BluetoothManager.
			BluetoothManager bluetoothManager = (BluetoothManager) GetSystemService (Context.BluetoothService);
			bluetoothAdapter = bluetoothManager.Adapter;

			// Checks if Bluetooth is supported on the device.
			if (bluetoothAdapter == null) 
			{
				Toast.MakeText (this, Resource.String.error_bluetooth_not_supported, ToastLength.Short).Show();
				Finish();
				return;
			}
		}


		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.main, menu);
			if (!scanning) {
				menu.FindItem (Resource.Id.menu_stop).SetVisible (false);
				menu.FindItem (Resource.Id.menu_scan).SetVisible (true);
				menu.FindItem (Resource.Id.menu_refresh).SetActionView (null);
			} else {
				menu.FindItem (Resource.Id.menu_stop).SetVisible (true);
				menu.FindItem (Resource.Id.menu_scan).SetVisible (false);
				menu.FindItem (Resource.Id.menu_refresh).SetActionView (
					Resource.Layout.actionbar_indeterminate_progress);
			}
			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_scan:
				leDeviceListAdapter.Clear ();
				ScanLeDevice (true);
				break;

			case Resource.Id.menu_stop:
				ScanLeDevice (false);
				break;
			}
			return true;
		}


		protected override void OnResume ()
		{
			base.OnResume ();

			// Ensures Bluetooth is enabled on the device.  If Bluetooth is not currently enabled,
			// fire an intent to display a dialog asking the user to grant permission to enable it.
			if (!bluetoothAdapter.IsEnabled) {
				if (!bluetoothAdapter.IsEnabled) {
					Intent enableBtIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult (enableBtIntent, REQUEST_ENABLE_BT);
				}
			}

			// Initializes list view adapter.
			leDeviceListAdapter = new LeDeviceListAdapter (this);
			ListAdapter = leDeviceListAdapter;
			ScanLeDevice (true);
		}


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			// User chose not to enable Bluetooth.
			if (requestCode == REQUEST_ENABLE_BT && resultCode == Result.Canceled) {
				Finish ();
				return;
			}

			base.OnActivityResult (requestCode, resultCode, data);
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			ScanLeDevice (false);
			leDeviceListAdapter.Clear ();
		}


		protected override void OnListItemClick (ListView l, View v, int position, long id)
		{
			BluetoothDevice device = leDeviceListAdapter.GetDevice (position);
			if (device == null) 
				return;

			Intent intent = new Intent (this, typeof (DeviceControlActivity));
			intent.PutExtra (DeviceControlActivity.EXTRAS_DEVICE_NAME, device.Name);
			intent.PutExtra (DeviceControlActivity.EXTRAS_DEVICE_ADDRESS, device.Address);
			if (scanning) {
				bluetoothAdapter.StopLeScan (this);
				scanning = false;
			}
			StartActivity (intent);
		}


		void ScanLeDevice (bool enable)
		{
			if (enable) {
				// Stops scanning after a pre-defined scan period.
				handler.PostDelayed (new Action (delegate {
					scanning = false;
					bluetoothAdapter.StopLeScan (this);
					InvalidateOptionsMenu ();
				}), SCAN_PERIOD);

				scanning = true;
				bluetoothAdapter.StartLeScan (this);
			} else {
				scanning = false;
				bluetoothAdapter.StopLeScan (this);
			}
			InvalidateOptionsMenu();
		}


		// Device scan callback.
		public void OnLeScan (BluetoothDevice device, int rssi, byte[] scanRecord)
		{
			RunOnUiThread (new Action (delegate {
				leDeviceListAdapter.AddDevice (device);
				leDeviceListAdapter.NotifyDataSetChanged();
			}));
		}

	}


	// Adapter for holding devices found through scanning.
	class LeDeviceListAdapter : BaseAdapter 
	{
		List <BluetoothDevice> mLeDevices;
		LayoutInflater mInflator;
		Context context;

		public LeDeviceListAdapter (Context c) 
		{
			context = c;
			mLeDevices = new List <BluetoothDevice> ();
			mInflator = LayoutInflater.From (context);

		}

		public void AddDevice (BluetoothDevice device) 
		{
			if (!mLeDevices.Contains (device)) {
				mLeDevices.Add (device);
			}
		}

		public BluetoothDevice GetDevice (int position) 
		{
			return mLeDevices [position];
		}

		public void Clear ()
		{
			mLeDevices.Clear ();
		}

		public override int Count {
			get {
				return mLeDevices.Count;
			}
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return mLeDevices [position];
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			ViewHolder viewHolder;

			// General ListView optimization code.
			if (convertView == null) {
				convertView = mInflator.Inflate (Resource.Layout.listitem_device, null);
				viewHolder = new ViewHolder ();
				viewHolder.DeviceAddress = convertView.FindViewById <TextView>  (Resource.Id.device_address);
				viewHolder.DeviceName = convertView.FindViewById <TextView> (Resource.Id.device_name);
				convertView.Tag = viewHolder;
			} else {
				viewHolder = (ViewHolder) convertView.Tag;
			}

			BluetoothDevice device = mLeDevices [position];
			String deviceName = device.Name;
			if (deviceName != null && deviceName.Length > 0)
				viewHolder.DeviceName.Text = deviceName;
			else
				viewHolder.DeviceName.SetText (Resource.String.unknown_device);

			viewHolder.DeviceAddress.Text = device.Address;

			return convertView;
		}

		class ViewHolder : Java.Lang.Object
		{
			public TextView DeviceName { get; set; }
			public TextView DeviceAddress { get; set; }
		}
	}
}


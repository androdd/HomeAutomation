namespace AdSoft.Fez.Hardware.Storage
{
    using System;

    using GHIElectronics.NETMF.IO;
    using GHIElectronics.NETMF.USBHost;

    using Microsoft.SPOT;
    using Microsoft.SPOT.IO;

    public class UsbStick : StorageBase
    {
        private string _root;
        private VolumeInfo _volume;

        protected override string Root
        {
            get { return _root; }
        }

        public UsbStick()
        {
            RemovableMedia.Insert += RemovableMediaInsert;
            RemovableMedia.Eject += RemovableMediaEject;
            USBHostController.DeviceConnectedEvent += DeviceConnected;
            
            // Start monitoring for USB device status
            StartDeviceMonitoring();
        }
        
        private void StartDeviceMonitoring()
        {
            // Check if we have any USB mass storage devices connected
            var devices = USBHostController.GetDevices();
            foreach (var device in devices)
            {
                if (device.TYPE == USBH_DeviceType.MassStorage)
                {
                    DeviceConnected(device);
                    break;
                }
            }
        }

        protected override void InitStorage()
        {
        }

        protected override void Flush()
        {
            if (_volume != null)
            {
                _volume.FlushAll();
            }
        }
        
        public void AttemptReconnection()
        {
            Debug.Print("USB - Attempting reconnection...");
            
            // Unmount current storage
            Unmount();
            
            // Try to find and reconnect to USB devices
            StartDeviceMonitoring();
        }

        private void DeviceConnected(USBH_Device device)
        {
            if (device.TYPE != USBH_DeviceType.MassStorage)
            {
                return;
            }

            try
            {
                if (Storage != null)
                {
                    // Clean up existing storage
                    try
                    {
                        Storage.UnmountFileSystem();
                        Storage.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("USB - Error cleaning up existing storage: " + ex.Message);
                    }
                }

                Storage = new PersistentStorage(device);
                Storage.MountFileSystem();
                
                Debug.Print("USB - Storage mounted successfully");
            }
            catch (Exception ex)
            {
                Debug.Print("USB - Failed to mount USB stick: " + ex.Message);
                IsLoaded = false;

                RaiseStatusChanged(Status.Error);
            }
        }

        private void RemovableMediaInsert(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name != "USB")
            {
                return;
            }

            Debug.Print("USB - Media inserted");
            if(Storage != null)
            {
                _volume = e.Volume;
                _root = _volume.RootDirectory;
                IsLoaded = true;
                
                Debug.Print("USB - Storage available at: " + _root);
            }
            else
            {
                Debug.Print("USB - Storage not mounted, media insert ignored");
                IsLoaded = false;
            }

            RaiseStatusChanged(IsLoaded ? Status.Available : Status.Unavailable);
        }

        private void RemovableMediaEject(object sender, MediaEventArgs e)
        {
            Debug.Print("USB - Media ejected");
            RaiseStatusChanged(Status.Unavailable);
            Unmount();
        }
    }
}
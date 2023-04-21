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
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
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

        private void DeviceConnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.MassStorage)
            {
                try
                {
                    Storage = new PersistentStorage(device);
                    Storage.MountFileSystem();
                }
                catch (Exception ex)
                {
                    Debug.Print("USB - Failed to mount USB stick: " + ex.Message);
                    IsLoaded = false;

                    RaiseStatusChanged(Status.Error);
                }
            }
        }

        private void RemovableMediaInsert(object sender, MediaEventArgs e)
        {
            _volume = e.Volume;
            _root = _volume.RootDirectory;
            IsLoaded = true;
            RaiseStatusChanged(Status.Available);
        }

        private void RemovableMediaEject(object sender, MediaEventArgs e)
        {
            RaiseStatusChanged(Status.Unavailable);
            Unmount();
        }
    }
}
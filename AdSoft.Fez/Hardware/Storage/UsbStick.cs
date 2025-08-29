namespace AdSoft.Fez.Hardware.Storage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using GHIElectronics.NETMF.IO;
    using GHIElectronics.NETMF.USBHost;

    using Microsoft.SPOT;
    using Microsoft.SPOT.IO;

    using Math = System.Math;

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
        }

        protected override void InitStorage()
        {
            if (IsLoaded)
            {
                return;
            }


            USBH_Device[] devices = USBHostController.GetDevices();

            foreach (var device in devices)
            {
                if (device.TYPE != USBH_DeviceType.MassStorage)
                {
                    continue;
                }

                Debug.Print("InitStorage");
                DeviceConnected(device);
                break;
            }

            for (int i = 0; i < 3; i++)
            {
                // Exponential back-off: 1s, 2s, 4s, 8s, etc. with a cap at 10 seconds
                int delayMs = Math.Min(1000 * (1 << i), 10000);
                Thread.Sleep(delayMs);

                if (IsLoaded)
                {
                    return;
                }
            }
        }

        protected override void Flush()
        {
            if (_volume != null)
            {
                _volume.FlushAll();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void DeviceConnected(USBH_Device device)
        {
            if (device.TYPE != USBH_DeviceType.MassStorage)
            {
                return;
            }

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

        private void RemovableMediaInsert(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name != "USB")
            {
                return;
            }

            //Debug.Print("RemovableMediaInsert");
            if(Storage != null)
            {
                _volume = e.Volume;
                _root = _volume.RootDirectory;
                IsLoaded = true;
            }

            RaiseStatusChanged(IsLoaded ? Status.Available : Status.Unavailable);
        }

        private void RemovableMediaEject(object sender, MediaEventArgs e)
        {
            //Debug.Print("RemovableMediaEject");
            RaiseStatusChanged(Status.Unavailable);
            Unmount();
        }
    }
}
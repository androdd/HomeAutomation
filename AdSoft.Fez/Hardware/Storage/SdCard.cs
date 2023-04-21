namespace AdSoft.Fez.Hardware.Storage
{
    using System;

    using GHIElectronics.NETMF.IO;

    using Microsoft.SPOT;

    public class SdCard: StorageBase
    {
        protected override string Root
        {
            get { return "\\SD"; }
        }

        protected override void InitStorage()
        {
            if (!PersistentStorage.DetectSDCard())
            {
                Debug.Print("SD - card missing");

                Unmount();

                RaiseStatusChanged(Status.Unavailable);

                return;
            }

            if (Storage != null)
            {
                return;
            }

            try
            {
                Storage = new PersistentStorage("SD");
                Storage.MountFileSystem();
                IsLoaded = true;
                RaiseStatusChanged(Status.Available);
            }
            catch (Exception ex)
            {
                Debug.Print("SD - Failed to mount SD card: " + ex.Message);
                IsLoaded = false;

                RaiseStatusChanged(Status.Error);
            }
        }

        protected override void Flush()
        {
        }
    }
}

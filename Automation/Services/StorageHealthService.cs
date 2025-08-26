namespace HomeAutomation.Services
{
    using System;
    using System.Threading;

    using AdSoft.Fez.Hardware.Storage;

    using HomeAutomation.Tools;

    using Microsoft.SPOT;

    public class StorageHealthService
    {
        private readonly IStorage _storage;
        private readonly Log _log;
        private readonly RealTimer _realTimer;
        private bool _isMonitoring;

        public StorageHealthService(IStorage storage, Log log, RealTimer realTimer)
        {
            _storage = storage;
            _log = log;
            _realTimer = realTimer;
        }

        public void StartMonitoring()
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            
            // Check storage health every 5 minutes
            _realTimer.TryScheduleRunAt(DateTime.Now.AddMinutes(5), CheckStorageHealth, "Storage Health Check");
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
        }

        private void CheckStorageHealth(TimerState state)
        {
            if (!_isMonitoring)
                return;

            try
            {
                if (_storage != null && !_storage.IsLoaded)
                {
                    Debug.Print("Storage not available, attempting reconnection...");
                    
                    // Try to reconnect USB storage
                    if (_storage is AdSoft.Fez.Hardware.Storage.UsbStick usbStick)
                    {
                        usbStick.AttemptReconnection();
                        
                        // Wait a bit for reconnection
                        Thread.Sleep(2000);
                        
                        // Try a test write
                        if (_storage.TryAppend("health_check.txt", "Health check at " + DateTime.Now.ToString() + "\r\n"))
                        {
                            _log.Write("Storage health check passed after reconnection");
                        }
                        else
                        {
                            Debug.Print("Storage health check failed after reconnection attempt");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Storage health check exception: " + ex.Message);
            }

            // Schedule next check
            if (_isMonitoring)
            {
                _realTimer.TryScheduleRunAt(DateTime.Now.AddMinutes(5), CheckStorageHealth, "Storage Health Check");
            }
        }
    }
}

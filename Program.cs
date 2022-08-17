using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using System.IO;
using System.Collections;
using GHIElectronics.NETMF.Hardware;

namespace HomeAutomation
{
    using System;

    public class Program
    {
        static OutputPort[] relays;

        static InputPort pressureSensor;

        static ArrayList pressureData;

        public static void Main()
        {
            //RealTimeClock.SetTime(new DateTime(2022, 8, 17, 14, 48, 0));


            pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            pressureData = new ArrayList();

            Timer pressureSensorTimer = new Timer(new TimerCallback(pressureSensorTimer_Execute), null, 1000, 1000);

            return;

            PersistentStorage sdPS = new PersistentStorage("SD");

            sdPS.MountFileSystem();

            Debug.Print("Getting files and folders:");

            if (VolumeInfo.GetVolumes()[0].IsFormatted)
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                string[] files = Directory.GetFiles(rootDirectory);
                string[] folders = Directory.GetDirectories(rootDirectory);
                Debug.Print("Files available on " + rootDirectory + ":");
                for (int i = 0; i < files.Length; i++)
                    Debug.Print(files[i]);

                Debug.Print("Folders available on " + rootDirectory + ":");
                for (int i = 0; i < folders.Length; i++) Debug.Print(folders[i]);
            }
            else
            {
                Debug.Print("Storage is not formatted. Format on PC with                              FAT32/FAT16 first.");
            }

            // Unmount            
            sdPS.UnmountFileSystem();
        }

        public static void MainOld()
        {
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);

            bool relayTrue = false;
            relays = new OutputPort[8];
            Cpu.Pin[] diPins = new Cpu.Pin[8] {
                (Cpu.Pin)FEZ_Pin.Digital.Di0,
                (Cpu.Pin)FEZ_Pin.Digital.Di1,
                (Cpu.Pin)FEZ_Pin.Digital.Di2,
                (Cpu.Pin)FEZ_Pin.Digital.Di3,
                (Cpu.Pin)FEZ_Pin.Digital.Di4,
                (Cpu.Pin)FEZ_Pin.Digital.Di5,
                (Cpu.Pin)FEZ_Pin.Digital.Di6,
                (Cpu.Pin)FEZ_Pin.Digital.Di7
            };
            
            for (int i = 0; i < relays.Length; i++)
            {
                relays[i] = new OutputPort(diPins[i], !relayTrue);
            }

            LegoRemote remote = new LegoRemote((Cpu.Pin)FEZ_Pin.Interrupt.Di11);
            remote.OnLegoButtonPress += remote_OnLegoButtonPress;

            InputPort pressureSensor = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false, Port.ResistorMode.PullUp);

            led.Write(true);
            Thread.Sleep(600);
            led.Write(false);
            Thread.Sleep(600);

            for (int i = 0; i < relays.Length; i++)
            {
                relays[i].Write(relayTrue);
                Thread.Sleep(600);
                relays[i].Write(!relayTrue);
                Thread.Sleep(600);
            }

            while (true)
            {
                var hasPressure = pressureSensor.Read();

                relays[5].Write(hasPressure);

                Thread.Sleep(1000);
            }
                  
            PersistentStorage sdPS = new PersistentStorage("SD");

            sdPS.MountFileSystem();

            Debug.Print("Getting files and folders:");

            if (VolumeInfo.GetVolumes()[0].IsFormatted)
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                string[] files = Directory.GetFiles(rootDirectory);
                string[] folders = Directory.GetDirectories(rootDirectory);
                Debug.Print("Files available on " + rootDirectory + ":");
                for (int i = 0; i < files.Length; i++)
                    Debug.Print(files[i]);

                Debug.Print("Folders available on " + rootDirectory + ":");
                for (int i = 0; i < folders.Length; i++) Debug.Print(folders[i]);
            }
            else
            {
                Debug.Print("Storage is not formatted. Format on PC with                              FAT32/FAT16 first.");
            }

            // Unmount            
            sdPS.UnmountFileSystem();
        }

        static void remote_OnLegoButtonPress(Message msg)
        {
            if (msg.CommandA == Command.ComboDirectForward)
            {
                relays[4].Write(false);
            }

            if (msg.CommandA == Command.ComboDirectBackward)
            {
                relays[4].Write(true);
            }
        }

        static void pressureSensorTimer_Execute(object state)
        {
            if (pressureData.Count > 10)
            {
                pressureData.Clear();
            }

            var hasPressure = pressureSensor.Read();

            pressureData.Add(new WaterData { Timestamp = RealTimeClock.GetTime(), Pressure = hasPressure ? 1 : 0 });
        }
    }
}

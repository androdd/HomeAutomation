namespace HomeAutomation.Hardware
{
    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.NecRemote;
    using AdSoft.Fez.Hardware.Storage;

    using GHIElectronics.NETMF.FEZ;

    public class HardwareManager
    {
        public IStorage ExternalStorage { get; private set; }
        public RelaysArray RelaysArray { get; private set; }
        public IPressureSensor PressureSensor { get; private set; }
        public IPumpStateSensor PumpStateSensor { get; private set; }
        public NecRemote NecRemote { get; private set; }
        public FlowRateSensor FlowRateSensor { get; private set; }
        public Lcd2004 Screen { get; private set; }
        public ScreenPowerButton ScreenPowerButton { get; private set; }
        public Led MbLed { get; private set; }

        public int SouthMainValveRelayId { get; private set; }
        public int NorthMainValveRelayId { get; private set; }
        public int WateringPumpTurnOffRelayId { get; private set; }
        public int AutoTurnOffPumpRelayId { get; private set; }
        public int LightsRelayId { get; private set; }

        public HardwareManager(IStorage externalStorage)
        {
            ExternalStorage = externalStorage;

            RelaysArray = new RelaysArray(new[]
            {
                FEZ_Pin.Digital.Di0, // South Main Valve
                FEZ_Pin.Digital.Di1, // North Main Valve
                FEZ_Pin.Digital.Di4, // South Valve 1 - Configured
                FEZ_Pin.Digital.Di5, // South Valve 2 - Configured
                FEZ_Pin.Digital.Di6, // South Valve 3 - Configured
                FEZ_Pin.Digital.Di7, // Watering Pump Auto Turn Off
                FEZ_Pin.Digital.Di8, // House Pump Auto Turn Off
                FEZ_Pin.Digital.Di9  // Lights
            });
            PressureSensor = new PressureSensor80(FEZ_Pin.AnalogIn.An1);
            PumpStateSensor = new PumpStateSensor(FEZ_Pin.Digital.An0);
            NecRemote = new NecRemote(FEZ_Pin.Interrupt.Di11);
            FlowRateSensor = new FlowRateSensor(FEZ_Pin.Interrupt.Di12);
            Screen = new Lcd2004(0x27);
            ScreenPowerButton = new ScreenPowerButton(FEZ_Pin.Interrupt.An3, Screen);
            MbLed = new Led(FEZ_Pin.Digital.LED);
            
            SouthMainValveRelayId = 0;
            NorthMainValveRelayId = 1;
            WateringPumpTurnOffRelayId = 5;
            AutoTurnOffPumpRelayId = 6;
            LightsRelayId = 7;
            
#if TEST_AUTO_TURN_OFF_SERVICE
            _log.Write("TEST_AUTO_TURN_OFF_SERVICE enabled. PumpStateSensor and PressureSensor are controlled manually through mocks and remote.");
            PumpStateSensor = new PumpStateSensorMock();
            PressureSensor = new PressureSensorMock();
#endif
            
#if TEST_TUNE_PRESSURE
            _log.Write("TEST_TUNE_PRESSURE enabled. PressureSensor are controlled manually through mocks and remote.");
            PressureSensor = new PressureSensorMock();
#endif
        }
        
        public void Setup()
        {
            RelaysArray.Init();
            PressureSensor.Init();
            PumpStateSensor.Init();
            NecRemote.Init();
            FlowRateSensor.Init();
            ScreenPowerButton.Init();
            MbLed.Init();
        }
    }
}
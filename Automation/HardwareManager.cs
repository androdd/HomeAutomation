namespace HomeAutomation
{
    using AdSoft.Fez.Hardware;
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.Lcd2004;
    using AdSoft.Fez.Hardware.LegoRemote;
    using AdSoft.Fez.Hardware.SdCard;

    using GHIElectronics.NETMF.FEZ;

    using HomeAutomation.Hardware.Mocks;
    using HomeAutomation.Tools;

    public class HardwareManager
    {
        private readonly Log _log;
        
        public SdCard SdCard { get; private set; }
        public RelaysArray RelaysArray { get; private set; }
        public IPressureSensor PressureSensor { get; private set; }
        public IPumpStateSensor PumpStateSensor { get; private set; }
        public LegoRemote LegoRemote { get; private set; }
        public WaterFlowSensor WaterFlowSensor { get; private set; }
        public Lcd2004 Screen { get; private set; }
        public ScreenPowerButton ScreenPowerButton { get; private set; }
        public Led MbLed { get; private set; }

        public int LightsRelayId { get; private set; }
        public int AutoTurnOffPumpRelayId { get; private set; }

        public HardwareManager(Log log)
        {
            _log = log;

            SdCard = new SdCard();
            RelaysArray = new RelaysArray(new[]
            {
                FEZ_Pin.Digital.Di0,
                FEZ_Pin.Digital.Di1,
                FEZ_Pin.Digital.Di4,
                FEZ_Pin.Digital.Di5,
                FEZ_Pin.Digital.Di6,
                FEZ_Pin.Digital.Di7,
                FEZ_Pin.Digital.Di8,
                FEZ_Pin.Digital.Di9
            });
            PressureSensor = new PressureSensor80(FEZ_Pin.AnalogIn.An1);
            PumpStateSensor = new PumpStateSensor(FEZ_Pin.Digital.An0);
            LegoRemote = new LegoRemote(FEZ_Pin.Interrupt.Di11);
            WaterFlowSensor = new WaterFlowSensor(FEZ_Pin.Interrupt.Di12);
            Screen = new Lcd2004(0x27);
            ScreenPowerButton = new ScreenPowerButton(FEZ_Pin.Digital.Di13, Screen);
            MbLed = new Led(FEZ_Pin.Digital.LED);

            LightsRelayId = 7;
            AutoTurnOffPumpRelayId = 5;
            
#if TEST_AUTO_TURN_OFF_SERVICE
            _log.Write("TEST_AUTO_TURN_OFF_SERVICE enabled. PumpStateSensor and PressureSensor are controlled manually through mocks and remote.");
            PumpStateSensor = new PumpStateSensorMock();
            PressureSensor = new PressureSensorMock();
#endif
        }
        
        public void Setup()
        {
            RelaysArray.Init();
            PressureSensor.Init();
            PumpStateSensor.Init();
            LegoRemote.Init();
            WaterFlowSensor.Init();
            ScreenPowerButton.Init();
            MbLed.Init();
        }
    }
}
namespace HomeAutomation.Hardware.Mocks
{
    using HomeAutomation.Hardware.Interfaces;
    using HomeAutomation.Hardware.LegoRemote;
    using HomeAutomation.Models;
    using HomeAutomation.Tools;

    internal class AutoTurnOffPumpServiceTestRemoteCommandService : IRemoteCommandsService
    {
        private readonly Log _log;
        private readonly LegoRemote _legoRemote;
        private readonly PumpStateSensorMock _pumpStateSensor;
        private readonly PressureSensorMock _pressureSensor;

        public AutoTurnOffPumpServiceTestRemoteCommandService(
            Log log,
            LegoRemote legoRemote,
            IPumpStateSensor pumpStateSensor,
            IPressureSensor pressureSensor)
        {
            _log = log;
            _legoRemote = legoRemote;
            _pumpStateSensor = (PumpStateSensorMock)pumpStateSensor;
            _pressureSensor = (PressureSensorMock)pressureSensor;
        }

        public void Init()
        {
            _legoRemote.OnLegoButtonPress += LegoRemoteOnOnLegoButtonPress;
        }

        private void LegoRemoteOnOnLegoButtonPress(Message msg)
        {
            if (msg.CommandA == Command.ComboDirectForward)
            {
                _pumpStateSensor.IsWorking = true;
                _log.Write("Mock PumpStateSensor is set to working.");
            }
            else if (msg.CommandA == Command.ComboDirectBackward)
            {
                _pumpStateSensor.IsWorking = false;
                _log.Write("Mock PumpStateSensor is set to NOT working.");
            }

            if (msg.CommandB == Command.ComboDirectForward)
            {
                _pressureSensor.Pressure = 2;
                _log.Write("Mock PressureSensor is set to Pressure 2bar.");
            }
            else if (msg.CommandB == Command.ComboDirectBackward)
            {
                _pressureSensor.Pressure = 0;
                _log.Write("Mock PressureSensor is set to Pressure 0bar.");
            }
        }
    }
}
namespace HomeAutomation.Services
{
    using AdSoft.Fez.Hardware.Interfaces;
    using AdSoft.Fez.Hardware.LegoRemote;

    using HomeAutomation.Services.Interfaces;

    internal class RemoteCommandsService : IRemoteCommandsService
    {
        private readonly LegoRemote _legoRemote;
        private readonly LightsService _lightsService;

        public RemoteCommandsService(LegoRemote legoRemote, LightsService lightsService)
        {
            _legoRemote = legoRemote;
            _lightsService = lightsService;
        }

        public void Init()
        {
            _legoRemote.OnLegoButtonPress += remote_OnLegoButtonPress;
        }

        private void remote_OnLegoButtonPress(Message msg)
        {
            if (msg.Channel == 1)
            {
                if (msg.CommandA == Command.ComboDirectForward)
                {
                    _lightsService.SetLights(true, "Manual ");
                }
                else if (msg.CommandA == Command.ComboDirectBackward)
                {
                    _lightsService.SetLights(false, "Manual ");
                }    
            }
        }
    }
}

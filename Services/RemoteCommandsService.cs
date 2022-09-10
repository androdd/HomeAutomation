namespace HomeAutomation.Services
{
    using GHIElectronics.NETMF.FEZ;

    using HomeAutomation.Hardware;
    using HomeAutomation.Models;

    using Microsoft.SPOT.Hardware;

    internal class RemoteCommandsService
    {
        private readonly LightsService _lightsService;

        public RemoteCommandsService(LightsService lightsService)
        {
            _lightsService = lightsService;
        }

        public void Init()
        {
            LegoRemote remote = new LegoRemote((Cpu.Pin)FEZ_Pin.Interrupt.Di11);
            remote.OnLegoButtonPress += remote_OnLegoButtonPress;
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

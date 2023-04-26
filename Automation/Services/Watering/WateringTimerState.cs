namespace HomeAutomation.Services.Watering
{
    using HomeAutomation.Tools;

    public class WateringTimerState : TimerState
    {
        public int RelayId { get; set; }

        public bool TurnMainOnOff { get; set; }
    }
}
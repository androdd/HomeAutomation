namespace AdSoft.Fez.Hardware.LegoRemote
{
    public enum Escape
    {
        Mode = 0,
        ComboPwm = 1
    }

    public enum Address
    {
        Default = 0,
        Extra = 1
    }

    public enum Mode
    {
        Extended = 0,
        ComboDirect = 1,
        SinglePinContinuous = 2,
        SinglePinTimeout = 3,
        SingleOutput = 4
    }

    public enum Command
    {
        Na = -1,

        ComboDirectFloat = 0,
        ComboDirectForward = 1,
        ComboDirectBackward = 2,
        ComboDirectBrake = 3,

        SingleOutputFloat = 0,
        SingleOutputPwmForwardStep1 = 1,
        SingleOutputPwmForwardStep2 = 2,
        SingleOutputPwmForwardStep3 = 3,
        SingleOutputPwmForwardStep4 = 4,
        SingleOutputPwmForwardStep5 = 5,
        SingleOutputPwmForwardStep6 = 6,
        SingleOutputPwmForwardStep7 = 7,
        SingleOutputBrake = 8,
        SingleOutputPwmBackwardStep7 = 9,
        SingleOutputPwmBackwardStep6 = 10,
        SingleOutputPwmBackwardStep5 = 11,
        SingleOutputPwmBackwardStep4 = 12,
        SingleOutputPwmBackwardStep3 = 13,
        SingleOutputPwmBackwardStep2 = 14,
        SingleOutputPwmBackwardStep1 = 15,

        SingleOutputClear = 0,
        SingleOutputSetClear = 1,
        SingleOutputClearSet = 2,
        SingleOutputSet = 3,
        SingleOutputIncrementPwm = 4,
        SingleOutputDecrementPwm = 5,
        SingleOutputFullForward = 6,
        SingleOutputFullBackward = 7,
        SingleOutputToggleFull = 8
    }

    public enum SingleOutputMode
    {
        Na = -1,
        Pwm = 0,
        Cst = 1
    }
}

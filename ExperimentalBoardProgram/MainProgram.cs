namespace ExperimentalBoardProgram
{
    using System.Diagnostics;
    using System.Threading;

    using AdSoft.Fez.Hardware;

    using GHIElectronics.NETMF.FEZ;

    public static class MainProgram
    {
        public static void Start()
        {
            Debugger.Break();

            RelaysArray relaysArray = new RelaysArray(new[] { FEZ_Pin.Digital.Di0, FEZ_Pin.Digital.Di1 });

            relaysArray.Init();

            relaysArray.Set(0, true);
            relaysArray.Set(1, true);
            Thread.Sleep(1000);
            relaysArray.Set(0, false);
            relaysArray.Set(1, false);
        }
    }
}

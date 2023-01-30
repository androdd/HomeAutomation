namespace ExperimentalBoard
{
    using System.Threading;

    using GHIElectronics.NETMF.FEZ;

    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;

    public class Program
    {
        private I2CDevice.Configuration ConfigI2CLcd;
        private I2CDevice BusI2C;
        private ushort i2c_Add_7bits = 0x3A;

        public static void Main()
        {
            using (var led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false))
            {
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    led.Write(true);
                    Thread.Sleep(400);
                    led.Write(false);
                }
            }


            I2CDevice.Configuration con = new I2CDevice.Configuration(0x27, 100);
            I2CDevice MyI2C = new I2CDevice(con);
            //create transactions (we need 2 in this example)
            I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[1];



            byte[] RegisterNum = { 0 };
            xActions[0] = I2CDevice.CreateWriteTransaction(RegisterNum);
            var result  = MyI2C.Execute(xActions, 1000);

            Debug.Print("Result:" + result);

            Thread.Sleep(500);

            RegisterNum = new byte[1] { 0x08 }; //0000 1000
            xActions[0] = I2CDevice.CreateWriteTransaction(RegisterNum);
            result = MyI2C.Execute(xActions, 1000);


            Debug.Print("Result:" + result);
        }
    }
}

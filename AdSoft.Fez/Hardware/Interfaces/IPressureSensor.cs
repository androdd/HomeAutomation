namespace AdSoft.Fez.Hardware.Interfaces
{
    public interface IPressureSensor
    {
        double Voltage { get; }

        double Pressure
        {
            get;

            // Formula: P = (V - b) / a
            // Sensor                       b       a
            // 80 psi Experimental (air)    0.48    0.756833333
            // 80 psi Factory               0.5     0.727272727
            // 150 psi Experimental (air)   0.54    0.396766667
            // 150 psi Factory              0.5     0.388349515
        }

        void Init();
    }
}
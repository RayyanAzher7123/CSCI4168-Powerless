using UnityEngine;

public class Battery
{
    private int batteryLife = 1000;
    private bool isOn;

    public int getBatteryLife()
    {
        return batteryLife;
    }

    public void decrementTime()
    {
        batteryLife -= 1;
    }
}

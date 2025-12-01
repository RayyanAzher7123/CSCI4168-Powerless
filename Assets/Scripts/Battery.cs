using UnityEngine;

public class Battery
{
    private int startBatteryLife = 10000;
    private int batteryLife = 10000;
    private bool isOn;

    public int getBatteryLife()
    {
        return batteryLife;
    }

    public void decrementTime()
    {
        batteryLife -= 1;
    }

    public bool getIsLowBattery()
    {
        return batteryLife < startBatteryLife / 10;
    }
}

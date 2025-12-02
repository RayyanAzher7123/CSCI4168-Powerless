using UnityEngine;

public class Battery
{
    private int startBatteryLife = 3000;
    private int batteryLife = 3000;
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

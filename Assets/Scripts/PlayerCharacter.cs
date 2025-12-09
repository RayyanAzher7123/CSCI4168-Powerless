using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI key_tm;
    [SerializeField] private TextMeshProUGUI battery_tm;
    [SerializeField] private TextMeshProUGUI batteryLife_tm;
    [SerializeField] private TextMeshProUGUI electricGear_tm;

    private float keys;
    public List<Battery> batteries;
    private int batteryIndex;
    private int electricalGear;

    void Start()
    {
        batteryIndex = 0;
        electricalGear = 0;
        batteries = new  List<Battery>();
    }
    
    void Update()
    {
        //key_tm.text = "Keys: " + keys;
        //battery_tm.text = "Batteries: " + batteries.Count;
        //electricGear_tm.text = "Electric Gear: " + electricalGear;

        //if (batteries.Count > 0)
        //{
            //batteryLife_tm.text = "Battery Life: " + batteries[batteryIndex].getBatteryLife();
        //} else 
        //{
            //batteryLife_tm.text = "Battery Life: ---";
        //}
    }

    public void addKey()
    {
        keys++;
    }
    
    public void addGear()
    {
        electricalGear++;
    }

    public void removeKey()
    {
        keys--;
    }

    public float getKeys()
    {
        return keys;
    }
    
    public float getGear()
    {
        return electricalGear;
    }

    public void addBattery()
    {
        batteries.Add(new Battery());
    }

    public Battery getBattery()
    {
        if (batteries.Count > 0)
        {
            return batteries[batteryIndex];
        }
        
        return null;
    }
    
    public bool getIsLowBattery()
    {
        if (batteries.Count > 0)
        {
            return batteries[batteryIndex].getIsLowBattery();
        }

        return false;
    }
    
    public void reloadFlashlight()
    {
        if (batteries.Count <= 0)
        {
            return;
        }

        if (batteries.Count == 1)
        {
            if (batteries[batteryIndex].getBatteryLife() <= 0)
            {
                batteries.RemoveAt(batteryIndex);
                batteryIndex = 0;
            }
        } 
        else if (batteries[batteryIndex].getBatteryLife() <= 0)
        {
            batteries.RemoveAt(batteryIndex);
            batteryIndex = (batteryIndex + 1) % batteries.Count;
        }
        else
        {
            batteryIndex = (batteryIndex + 1) % batteries.Count;
        }
    }
}

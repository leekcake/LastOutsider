using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePanel : MonoBehaviour
{
    public Text Money, Food, Electric, Time;

    // Update is called once per frame
    void Update()
    {
        var resource = DataManager.Instance.Resource;

        Money.text = $"{resource.Money}/+{resource.MoneyRecoveryAmount}";
        Food.text = $"{resource.Food}/+{resource.FoodRecoveryAmount}";
        Electric.text = $"{resource.Electric}/+{resource.ElectricRecoveryAmount}";
        Time.text = $"{resource.Time}/+{resource.ElectricRecoveryAmount}";
    }
}

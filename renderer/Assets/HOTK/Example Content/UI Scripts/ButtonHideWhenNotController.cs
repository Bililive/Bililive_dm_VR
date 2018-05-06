using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonHideWhenNotController : MonoBehaviour
{
    public Dropdown LinkedDropdown;

    public Button Button
    {
        get { return _button ?? (_button = GetComponent<Button>()); }
    }

    private Button _button;

    public void SetDropdownState(string val)
    {
        if (LinkedDropdown == null) return;
        var dev =
            (MountDevice)
                Enum.Parse(typeof (MountDevice), LinkedDropdown.options[LinkedDropdown.value].text);
        switch (dev)
        {
            case MountDevice.LeftController:
            case MountDevice.RightController:
                Button.interactable = true;
            break;
            case MountDevice.Screen:
            case MountDevice.World:
            default:
                Button.interactable = false;
                break;
        }
    }
}

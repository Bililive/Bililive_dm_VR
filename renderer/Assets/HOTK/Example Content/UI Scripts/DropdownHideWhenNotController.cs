using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Dropdown))]
public class DropdownHideWhenNotController : MonoBehaviour
{
    public Dropdown LinkedDropdown;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

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
                Dropdown.interactable = true;
            break;
            case MountDevice.Screen:
            case MountDevice.World:
            default:
                Dropdown.interactable = false;
                break;
        }
    }
}

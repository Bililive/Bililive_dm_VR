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
            (HOTK_Overlay.AttachmentDevice)
                Enum.Parse(typeof (HOTK_Overlay.AttachmentDevice), LinkedDropdown.options[LinkedDropdown.value].text);
        switch (dev)
        {
            case HOTK_Overlay.AttachmentDevice.左手柄:
            case HOTK_Overlay.AttachmentDevice.右手柄:
                Dropdown.interactable = true;
            break;
            case HOTK_Overlay.AttachmentDevice.显示器:
            case HOTK_Overlay.AttachmentDevice.世界:
            default:
                Dropdown.interactable = false;
                break;
        }
    }
}

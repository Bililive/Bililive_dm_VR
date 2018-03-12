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
            (HOTK_Overlay.AttachmentDevice)
                Enum.Parse(typeof (HOTK_Overlay.AttachmentDevice), LinkedDropdown.options[LinkedDropdown.value].text);
        switch (dev)
        {
            case HOTK_Overlay.AttachmentDevice.左手柄:
            case HOTK_Overlay.AttachmentDevice.右手柄:
                Button.interactable = true;
            break;
            case HOTK_Overlay.AttachmentDevice.显示器:
            case HOTK_Overlay.AttachmentDevice.世界:
            default:
                Button.interactable = false;
                break;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

[RequireComponent(typeof(Dropdown))]
public class DropdownMatchEnumOptions : MonoBehaviour
{
    public HOTK_Overlay Overlay;

    public EnumSelection EnumOptions;

    public Dropdown Dropdown
    {
        get { return _dropdown ?? (_dropdown = GetComponent<Dropdown>()); }
    }

    private Dropdown _dropdown;

    public void OnEnable()
    {
        Dropdown.ClearOptions();
        var strings = new List<string>();
        switch (EnumOptions)
        {
            case EnumSelection.AttachmentDevice:
                UpdateDeviceDropdown();
                HOTK_TrackedDeviceManager.OnControllerIndicesUpdated += UpdateDeviceDropdown;
                break;
            case EnumSelection.AttachmentPoint:
                strings.AddRange(from object e in Enum.GetValues(typeof(HOTK_Overlay.AttachmentPoint)) select e.ToString());
                Dropdown.AddOptions(strings);
                Dropdown.value = strings.IndexOf(Overlay.AnchorPoint.ToString());
                break;
            case EnumSelection.AnimationType:
                strings.AddRange(from object e in Enum.GetValues(typeof(HOTK_Overlay.AnimationType)) select e.ToString());
                Dropdown.AddOptions(strings);
                Dropdown.value = strings.IndexOf(Overlay.AnimateOnGaze.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool SetToRightController = false;
    private bool SetToLeftController = false;

    public void SwapControllers()
    {
        if (HOTK_TrackedDeviceManager.Instance.LeftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && HOTK_TrackedDeviceManager.Instance.RightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) return; // If both controllers are found, don't swap selected name
        if (Dropdown.options[Dropdown.value].text == HOTK_Overlay.AttachmentDevice.LeftController.ToString()) SetToRightController = true;
        else if (Dropdown.options[Dropdown.value].text == HOTK_Overlay.AttachmentDevice.RightController.ToString()) SetToLeftController = true;
    }

    private void UpdateDeviceDropdown()
    {
        var strings = new List<string>
        {
            HOTK_Overlay.AttachmentDevice.World.ToString(), HOTK_Overlay.AttachmentDevice.Screen.ToString()
        };
        if (HOTK_TrackedDeviceManager.Instance.LeftIndex != OpenVR.k_unTrackedDeviceIndexInvalid) strings.Add(HOTK_Overlay.AttachmentDevice.LeftController.ToString());
        if (HOTK_TrackedDeviceManager.Instance.RightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) strings.Add(HOTK_Overlay.AttachmentDevice.RightController.ToString());
        Dropdown.ClearOptions();
        Dropdown.AddOptions(strings);
        if (SetToRightController) Dropdown.value = strings.IndexOf(HOTK_Overlay.AttachmentDevice.RightController.ToString());
        else if (SetToLeftController) Dropdown.value = strings.IndexOf(HOTK_Overlay.AttachmentDevice.LeftController.ToString());
        else Dropdown.value = strings.IndexOf(Overlay.AnchorDevice.ToString());
        SetToRightController = false;
        SetToLeftController = false;
    }

    public void SetDropdownState(string val)
    {
        switch (EnumOptions)
        {
            case EnumSelection.AttachmentDevice:
                Overlay.AnchorDevice = (HOTK_Overlay.AttachmentDevice) Enum.Parse(typeof (HOTK_Overlay.AttachmentDevice), Dropdown.options[Dropdown.value].text);
                break;
            case EnumSelection.AttachmentPoint:
                Overlay.AnchorPoint = (HOTK_Overlay.AttachmentPoint) Enum.Parse(typeof (HOTK_Overlay.AttachmentPoint), Dropdown.options[Dropdown.value].text);
                break;
            case EnumSelection.AnimationType:
                Overlay.AnimateOnGaze = (HOTK_Overlay.AnimationType) Enum.Parse(typeof (HOTK_Overlay.AnimationType), Dropdown.options[Dropdown.value].text);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetToOption(string text)
    {
        for (var i = 0; i < Dropdown.options.Count; i ++)
        {
            if (Dropdown.options[i].text != text) continue;
            Dropdown.value = i;
            break;
        }
    }

    public enum EnumSelection
    {
        AttachmentDevice,
        AttachmentPoint,
        AnimationType
    }
}

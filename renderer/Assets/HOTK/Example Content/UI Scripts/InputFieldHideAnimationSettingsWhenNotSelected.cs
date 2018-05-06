using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldHideAnimationSettingsWhenNotSelected : MonoBehaviour
{
    public Dropdown Dropdown;
    public SettingValue Setting;

    public InputField InputField
    {
        get { return _inputField ?? (_inputField = GetComponent<InputField>()); }
    }

    private InputField _inputField;
    public void OnValueChanges()
	{
	    if (Dropdown == null) return;
        var anim = (AnimationType)Enum.Parse(typeof(AnimationType), Dropdown.options[Dropdown.value].text);
        if (Setting == SettingValue.Alpha)
        {
            if (anim != AnimationType.Alpha && anim != AnimationType.AlphaAndScale)
            {
                InputField.interactable = false;
            }
            else
            {
                InputField.interactable = true;
            }
        }
        else if(Setting == SettingValue.Scale)
        {
            if (anim != AnimationType.Scale && anim != AnimationType.AlphaAndScale)
            {
                InputField.interactable = false;
            }
            else
            {
                InputField.interactable = true;
            }
        }

    }

    public enum SettingValue
    {
        Alpha,
        Scale,
    }
}

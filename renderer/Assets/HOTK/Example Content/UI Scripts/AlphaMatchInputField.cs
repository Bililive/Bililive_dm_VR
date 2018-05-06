using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class AlphaMatchInputField : MonoBehaviour
{
    public HOTK_Overlay Overlay;
    public InputValue Value;
    
    public InputField InputField
    {
        get { return _inputField ?? (_inputField = GetComponent<InputField>()); }
    }

    private InputField _inputField;

    public void OnEnable()
    {
        if (Overlay == null) return;
        switch (Value)
        {
            case InputValue.AlphaStart:
                InputField.text = Overlay.Alpha.ToString();
                break;
            case InputValue.AlphaEnd:
                InputField.text = Overlay.AnimationAlpha.ToString();
                break;
            case InputValue.AlphaSpeed:
                InputField.text = Overlay.AnimationAlphaSpeed.ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnAlphaChanged()
    {
        float f;
        if (!float.TryParse(InputField.text, out f)) return;
        if (Overlay == null) return;
        switch (Value)
        {
            case InputValue.AlphaStart:
                Overlay.Alpha = f;
                break;
            case InputValue.AlphaEnd:
                Overlay.AnimationAlpha = f;
                break;
            case InputValue.AlphaSpeed:
                Overlay.AnimationAlphaSpeed = f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public enum InputValue
    {
        AlphaStart,
        AlphaEnd,
        AlphaSpeed
    }
}

using System;
using UnityEngine;
using Valve.VR;

public class HOTK_TrackedDeviceManager : MonoBehaviour
{
    public static Action<ETrackedControllerRole, uint> OnControllerIndexChanged; // Called any time a specific controller changes index
    public static Action OnControllerIndicesUpdated; // Called only when both controllers have been checked/assigned or are swapped

    public static HOTK_TrackedDeviceManager Instance
    {
        get { return _instance ?? (_instance = new GameObject("HOTK_TrackedDeviceManager", typeof(HOTK_TrackedDeviceManager)) { hideFlags = HideFlags.HideInHierarchy }.GetComponent<HOTK_TrackedDeviceManager>()); }
    }

    public uint LeftIndex
    {
        get
        {
            return _leftIndex;
        }
    }
    public uint RightIndex
    {
        get
        {
            return _rightIndex;
        }
    }
    public uint HMDIndex
    {
        get
        {
            return _hmdIndex;
        }
    }

    private static HOTK_TrackedDeviceManager _instance;

    private uint _leftIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _rightIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    private uint _hmdIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

    public void Awake()
    {
        FindHMD();
        FindControllers();
    }

    public void Update()
    {
        FindHMD();
        FindControllers();
        UpdatePoses();
    }

    private readonly TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    private readonly TrackedDevicePose_t[] _gamePoses = new TrackedDevicePose_t[0];

    /// <summary>
    /// Grab the last poses from the compositor and push them to the event listeners.
    /// This method should be disabled if SteamVR_Render.cs is being used, because it is also called there.
    /// </summary>
    private void UpdatePoses()
    {
        var compositor = OpenVR.Compositor;
        if (compositor == null) return;
        compositor.GetLastPoses(_poses, _gamePoses);
        SteamVR_Utils.Event.Send("new_poses", _poses);
        SteamVR_Utils.Event.Send("new_poses_applied");
    }

    /// <summary>
    /// Attempt to find the HMD.
    /// </summary>
    public void FindHMD()
    {
        var system = OpenVR.System;
        if (system == null)
        {
            LogError("未找到 VR 系统。");
            // LogError("OpenVR System not found.");
            return;
        }

        if (_hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid &&
            system.GetTrackedDeviceClass(_hmdIndex) == ETrackedDeviceClass.HMD)
        {
            // Assume we as still connected to the HMD..
            return;
        }

        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.HMD) continue;
            _hmdIndex = i;
            break;
        }
        if (_hmdIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            // Log("Found HMD ( Device: {0} )", _hmdIndex);
            Log("找到头戴显示器 ( 设备: {0} )", _hmdIndex);
        }
    }

    public void ResetControllerFindAttemptCount()
    {
        _noControllersCount = 0;
    }

    private uint _noControllersCount;

    /// <summary>
    /// Attempt to find both controllers.
    /// </summary>
    public void FindControllers()
    {
        var system = OpenVR.System;
        if (system == null)
        {
            LogError("未找到 VR 系统。");
            // LogError("OpenVR System not found.");
            return;
        }
        if (_noControllersCount >= 10)
        {
            return;
        }

        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_leftIndex) == ETrackedDeviceClass.Controller &&
            _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid && system.GetTrackedDeviceClass(_rightIndex) == ETrackedDeviceClass.Controller)
        {
            // Assume we are still connected to the controllers..
            return;
        }

        if (_noControllersCount == 0) Log("正在查找手柄.."); //Log("Searching for Controllers..");
        _leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
        CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
        _rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
        CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
        CallControllersUpdated();

        if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) // Both controllers are assigned!
        {
            Log("找到手柄 ( 设备: {0} ): 右", _rightIndex);
            Log("找到手柄 ( 设备: {0} ): 左", _leftIndex);
            // Log("Found Controller ( Device: {0} ): Right", _rightIndex);
            // Log("Found Controller ( Device: {0} ): Left", _leftIndex);
        }
        else if (_leftIndex != OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid) // Left controller is assigned but right is missing
        {
            Log("找到手柄 ( 设备: {0} ): 左", _leftIndex);
            //Log("Found Controller ( Device: {0} ): Left", _leftIndex);
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _leftIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                _rightIndex = i;
                CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
                Log("找到手柄 ( 设备: {0} ): 右", _rightIndex);
                // Log("Found Controller ( Device: {0} ): Right", _rightIndex);
                break;
            }
            CallControllersUpdated();
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid) // Right controller is assigned but left is missing
        {
            Log("找到手柄 ( 设备: {0} ): 右", _rightIndex);
            // Log("Found Controller ( Device: {0} ): Right", _rightIndex);
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (i == _rightIndex || system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue;
                }
                _leftIndex = i;
                CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
                Log("找到手柄 ( 设备: {0} ): 左", _leftIndex);
                // Log("Found Controller ( Device: {0} ): Left", _leftIndex);
                break;
            }
            CallControllersUpdated();
        }
        else if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid && _rightIndex == OpenVR.k_unTrackedDeviceIndexInvalid) // Both controllers are unassigned
        {
            if (_noControllersCount == 0) LogWarning("SteamVR 报告没有已分配的手柄..!搜索中..");
            // LogWarning("SteamVR Reports No Assigned Controllers..! Searching..");
            var foundUnassigned = 0;
            var slots = new uint[2];
            // Sort through all the devices until we find two controllers
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (system.GetTrackedDeviceClass(i) != ETrackedDeviceClass.Controller)
                {
                    continue; // This device isn't a controller, skip it
                }
                switch (system.GetControllerRoleForTrackedDeviceIndex(i))
                {
                    case ETrackedControllerRole.LeftHand:
                        _leftIndex = i;
                        Log("找到手柄 ( 设备: {0} ): 左", _leftIndex);
                        // Log("Found Controller ( Device: {0} ): Left", _leftIndex);
                        CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
                        break;
                    case ETrackedControllerRole.RightHand:
                        _rightIndex = i;
                        Log("找到手柄 ( 设备: {0} ): 右", _rightIndex);
                        // Log("Found Controller ( Device: {0} ): Right", _rightIndex);
                        CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
                        break;
                    case ETrackedControllerRole.Invalid:
                        Log("找到手柄 ( 设备: {0} ): 未分配", i);
                        // Log("Found Controller ( Device: {0} ): Unassigned", i);
                        if (foundUnassigned <= 1)
                            slots[foundUnassigned++] = i;
                        break;
                }

                if (foundUnassigned == 2)
                {
                    break; // We have two controllers, stop searching
                }
            }
            switch (foundUnassigned)
            {
                case 2:
                    LogWarning("找到两个未分配左右的手柄! 随机分配中!");
                    // LogWarning("Found Two Unassigned Controllers! Randomly Assigning!");
                    _rightIndex = slots[0];
                    CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
                    _leftIndex = slots[1];
                    CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
                    break;
                case 1:
                    if (_leftIndex == OpenVR.k_unTrackedDeviceIndexInvalid &&
                       _rightIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
                    {
                        LogWarning("找到了一个未分配的手柄，但是右手柄已经分配了！未分配的手柄分配为左手柄！");
                        // LogWarning("Only Found One Unassigned Controller, and Right was already assigned! Assigning To Left!");
                        _leftIndex = slots[0];
                        CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
                        _noControllersCount = 10;
                    }
                    else
                    {
                        LogWarning("找到了一个未分配的手柄! 分配为右手柄!");
                        // LogWarning("Only Found One Unassigned Controller! Assigning To Right!");
                        _rightIndex = slots[0];
                        CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
                        _noControllersCount = 10;
                    }
                    break;
                case 0:
                    if (_noControllersCount == 0) LogWarning("找不到任何未分配的手柄！");
                    // LogWarning("Couldn't Find Any Unassigned Controllers!");
                    _noControllersCount++;
                    if (_noControllersCount >= 10)
                    {
                        LogError("未找到手柄！");
                        LogError("请连接手柄，然后重启！"); //TODO: 不需要重启就可以识别新连接的手柄
                        //LogError("Controllers not found!");
                        //LogError("Please connect the controllers and restart!");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CallControllersUpdated();
        }
    }

    private void CallIndexChanged(ETrackedControllerRole role, uint index)
    {
        if (OnControllerIndexChanged != null)
            OnControllerIndexChanged(role, index);
    }
    private void CallControllersUpdated()
    {
        if (OnControllerIndicesUpdated != null)
            OnControllerIndicesUpdated();
    }

    public void SwapControllers()
    {
        var t = _leftIndex;
        _leftIndex = _rightIndex;
        CallIndexChanged(ETrackedControllerRole.LeftHand, _leftIndex);
        _rightIndex = t;
        CallIndexChanged(ETrackedControllerRole.RightHand, _rightIndex);
        CallControllersUpdated();
    }

    /// <summary>
    /// This is just used to quickly enable/disable Log messages.
    /// </summary>
    /// <param name="text"></param>
    void Log(string text, params object[] vars)
    {
        Logger4UIScripts.Log.Invoke(vars == null ? text : string.Format(text, vars), Logger4UIScripts.LogColor.Blue);
    }

    /// <summary>
    /// This is just used to quickly enable/disable LogWarning messages.
    /// </summary>
    /// <param name="text"></param>
    void LogWarning(string text, params object[] vars)
    {
        Logger4UIScripts.Log.Invoke(vars == null ? text : string.Format(text, vars), Logger4UIScripts.LogColor.Yellow);
    }

    /// <summary>
    /// This is just used to quickly enable/disable LogError messages.
    /// </summary>
    /// <param name="text"></param>
    void LogError(string text, params object[] vars)
    {
        Logger4UIScripts.Log.Invoke(vars == null ? text : string.Format(text, vars), Logger4UIScripts.LogColor.Red);
    }
}

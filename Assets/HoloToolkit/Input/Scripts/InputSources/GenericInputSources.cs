﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// This class manages the Input Sources recognized by <see cref="Input.GetJoystickNames"/>.
    /// <para> <remarks>The Windows Mixed Reality Spatial Controllers are handed in the <see cref="InteractionInputSources"/></remarks>.</para>
    /// </summary>
    public class GenericInputSources : Singleton<GenericInputSources>
    {
        protected const string XboxController = "Xbox Controller";
        protected const string XboxOneForWindows = "Xbox One For Windows";
        protected const string XboxBluetoothGamePad = "Xbox Bluetooth Gamepad";
        protected const string XboxWirelessController = "Xbox Wireless Controller";

        protected const string MotionControllerLeft = "Spatial Controller - Left";
        protected const string MotionControllerRight = "Spatial Controller - Right";

        protected const string OpenVRControllerLeft = "OpenVR Controller - Left";
        protected const string OpenVRControllerRight = "OpenVR Controller - Right";

        protected const string OculusRemote = "Oculus Remote";
        protected const string OculusTouchLeft = "Oculus Touch - Left";
        protected const string OculusTouchRight = "Oculus Touch - Right";

        [SerializeField]
        [Tooltip("Time in seconds to determine if an Input Device has been connected or disconnected")]
        protected float DeviceRefreshInterval = 3.0f;
        protected string[] LastDeviceList;

        protected BaseInputMapping<XboxControllerInputType> XboxControllerMapping;
        protected BaseInputMapping<ViveControllerInputType> ViveControllerMapping;
        protected BaseInputMapping<OculusControllerInputType> OculusControllerMapping;

        protected readonly HashSet<GenericInputSource> InputSources = new HashSet<GenericInputSource>();

        private float deviceRefreshTimer;

        protected virtual void Update()
        {
            deviceRefreshTimer += Time.unscaledDeltaTime;

            if (deviceRefreshTimer >= DeviceRefreshInterval)
            {
                deviceRefreshTimer = 0.0f;
                RefreshDevices();
            }
        }

        protected virtual void RefreshDevices()
        {
            var joystickNames = Input.GetJoystickNames();

            if (joystickNames.Length <= 0) { return; }

            if (LastDeviceList != null && joystickNames.Length == LastDeviceList.Length)
            {
                for (int i = 0; i < LastDeviceList.Length; i++)
                {
                    if (joystickNames[i].Equals(LastDeviceList[i])) { continue; }

                    foreach (var inputSource in InputSources)
                    {
                        if (inputSource.Name.Equals(joystickNames[i]))
                        {
                            InputManager.Instance.RaiseSourceLost(inputSource);
                            InputSources.Remove(inputSource);
                        }
                    }
                }
            }

            for (var i = 0; i < joystickNames.Length; i++)
            {
                if (joystickNames[i].Contains(MotionControllerLeft) ||
                    joystickNames[i].Contains(MotionControllerRight))
                {
                    // If we don't have any matching joystick types, continue.
                    // Skip any WMR motion controllers connected.  They're handled in the InteractionInputSources.
                    continue;
                }

                if (joystickNames[i].Contains(XboxController) ||
                    joystickNames[i].Contains(XboxOneForWindows) ||
                    joystickNames[i].Contains(XboxBluetoothGamePad) ||
                    joystickNames[i].Contains(XboxWirelessController))
                {
                    var inputSource = new GenericInputSource(InputManager.GenerateNewSourceId(), joystickNames[i]);
                    InputSources.Add(inputSource);
                    InputManager.Instance.RaiseSourceDetected(inputSource);
                }
                else if (joystickNames[i].Contains(OculusRemote) ||
                         joystickNames[i].Contains(OculusTouchLeft) ||
                         joystickNames[i].Contains(OculusTouchRight) ||
                         joystickNames[i].Contains(OpenVRControllerLeft) ||
                         joystickNames[i].Contains(OpenVRControllerRight))
                {
                    var inputSource = new GenericInputPointingSource(InputManager.GenerateNewSourceId(), joystickNames[i]);
                    InputSources.Add(inputSource);
                    InputManager.Instance.RaiseSourceDetected(inputSource);
                }
                else
                {
                    Debug.LogWarningFormat("Unimplemented Controller Type: {0}", joystickNames[i]);
                }
            }

            LastDeviceList = joystickNames;
        }
    }
}

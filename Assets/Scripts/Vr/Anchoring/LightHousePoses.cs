using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Anchoring
{
    public class LightHousePoses : MonoBehaviour
    {
        [SerializeField] private SteamVR_RenderModel[] renderers = default;

        private readonly List<InputDevice> inputDevices = new List<InputDevice>();
        //private TrackedDevicePose_t[] devices = new TrackedDevicePose_t[0];
        

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].index = (SteamVR_TrackedObject.EIndex)(i + 1);
        }
#endif


        private void Update()
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.TrackingReference, inputDevices);
            
            //OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0, devices);
            for (int i = 0; i < renderers.Length; i++)
                UpdatePose(i);
        }


        private void UpdatePose(int index)
        {
            var renderer = renderers[index];
            if (renderer == null) throw new Exception("Requires renderer");

            if (index < inputDevices.Count && index >= 0)
            {
               
                inputDevices[index].TryGetFeatureValue(CommonUsages.devicePosition, out var lightHousePosition);
                inputDevices[index].TryGetFeatureValue(CommonUsages.deviceRotation, out var lightHouseRotation);

                renderer.transform.localPosition = lightHousePosition;//devices[index].mDeviceToAbsoluteTracking.GetPosition();
                renderer.transform.localRotation = lightHouseRotation;// devices[index].mDeviceToAbsoluteTracking.GetRotation();

                renderer.gameObject.SetActive(true);

                renderer.name = inputDevices[index].serialNumber;
            }
            else
            {
                renderer.gameObject.SetActive(false);
            }
        }
    }
}
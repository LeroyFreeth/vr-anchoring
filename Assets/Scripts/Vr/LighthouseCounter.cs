using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class LighthouseCounter : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        List<InputDevice> trackedInputDevices = new();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.TrackingReference,
            trackedInputDevices);

        meshRenderer.enabled = trackedInputDevices.Count == 4;
    }
}

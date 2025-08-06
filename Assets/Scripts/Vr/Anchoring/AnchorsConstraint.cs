using System;
using System.Linq;
using Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Anchoring
{
    public class AnchorsConstraint : MonoBehaviour
    {
        [Header("Saving")]
        [SerializeField] private string constraintDataStr = "anchors_constraint_data";

        [Header("Settings")]
        [SerializeField] private Transform[] playerRoots;
        [SerializeField] private AnchorSource[] anchorsSourceData;
        [SerializeField] private AnchorsConstraintData anchorsConstraintData = new();
        [SerializeField] private AnchorsConstraintData prevAnchorsConstraintData;

        private JsonFileHelper<AnchorsConstraintData> _anchorDataSaver;

        public AnchorSource[] AnchorSources => anchorsSourceData;

        private void Start()
        {
            _anchorDataSaver = new JsonFileHelper<AnchorsConstraintData>(constraintDataStr);
            LoadDataFromFile();
        }

        private void Update()
        {
            anchorsConstraintData.anchors = GetLatestAnchorData();
            if (anchorsConstraintData.anchors.Length == 0) return;

            if (!anchorsConstraintData.anchors.Any(x => x.isRotationDriver)
                && anchorsConstraintData.anchors != null
                && anchorsConstraintData.anchors.Length > 0)
            {
                var a = anchorsConstraintData.anchors[0];
                a.isRotationDriver = true;
                anchorsConstraintData.anchors[0] = a;
            }

            var avgCenter = GetAnchorsAvg(anchorsConstraintData.anchors);
            var anchorsRotation = GetAnchorsRotation(anchorsConstraintData.anchors);

            if (prevAnchorsConstraintData == null || prevAnchorsConstraintData.anchors.Length == 0)
                prevAnchorsConstraintData = anchorsConstraintData.Clone();

            if (!AreAnchorDatasSame(prevAnchorsConstraintData.anchors, anchorsConstraintData.anchors))
            {
                var oldAnchorsRotation = GetAnchorsRotation(prevAnchorsConstraintData.anchors);
                var anchorsRotDiff = Quaternion.Inverse(anchorsRotation) * oldAnchorsRotation;
                anchorsConstraintData.anchorOffsets.rotation *= anchorsRotDiff;

                var oldCenter = GetAnchorsAvg(prevAnchorsConstraintData.anchors);
                var oldFullRotation =
                    Quaternion.Inverse(oldAnchorsRotation * prevAnchorsConstraintData.anchorOffsets.rotation);
                var newFullRotation =
                    Quaternion.Inverse(anchorsRotation * anchorsConstraintData.anchorOffsets.rotation);

                var anchorsAvgDiff = newFullRotation * avgCenter - oldFullRotation * oldCenter;

                anchorsConstraintData.anchorOffsets.position += anchorsAvgDiff;
            }

            var rotation = Quaternion.Inverse(anchorsRotation * anchorsConstraintData.anchorOffsets.rotation);
            var position = rotation * -avgCenter + anchorsConstraintData.anchorOffsets.position;
            foreach (var root in playerRoots)
            {
                root.localPosition = position;
                root.localRotation = rotation;
            }

            prevAnchorsConstraintData = anchorsConstraintData.Clone();
            SaveDataToFile();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Copy Data"))
                SaveDataToClipboard();

            if (GUILayout.Button("Paste Data"))
                LoadDataFromClipboard();

            if (GUILayout.Button("Save Data"))
                SaveDataToFile();

            if (GUILayout.Button("Load Data"))
                LoadDataFromFile();

            if (GUILayout.Button("Reset"))
            {
                anchorsConstraintData = new AnchorsConstraintData();
                prevAnchorsConstraintData = null;
            }
        }

        private void OnDrawGizmos()
        { 
            // Debug gizmo to show the offset based on missing/disabled anchors
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(anchorsConstraintData.anchorOffsets.position, .5f);
            // Debug gizmo showing the center of the anchors
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(GetAnchorsAvg(anchorsConstraintData.anchors), .5f);
        }

        private AnchorData[] GetLatestAnchorData()
        {
            return anchorsSourceData
                .Where(x => x.active && (!x.position.Equals(Vector3.zero) || !x.rotation.Equals(Quaternion.identity)))
                .Select(a => new AnchorData
                {
                    id = a.id,
                    position = a.position,
                    rotation = Quaternion.Euler(0, a.rotation.eulerAngles.y, 0)
                })
                .ToArray();
        }

        private Vector3 GetAnchorsAvg(AnchorData[] anchors)
        {
            if (anchors.Length == 0) return Vector3.zero;

            var avg = new Vector3();
            var count = 0;
            foreach (var a in anchors)
            {
                avg += a.position;
                count++;
            }

            avg /= count;
            return avg;
        }

        private Quaternion GetAnchorsRotation(AnchorData[] anchors)
        {
            if (anchors.Length == 0) return Quaternion.identity;

            var driver = anchors.First(x => x.isRotationDriver);
            return Quaternion.Euler(0, driver.rotation.eulerAngles.y, 0);
        }

        private bool AreAnchorDatasSame(AnchorData[] arr1, AnchorData[] arr2)
        {
            if (arr1.Length == 0 || arr2.Length == 0) return false;
            var rotDriver1 = arr1.FirstOrDefault(x => x.isRotationDriver).id;
            var rotDriver2 = arr2.FirstOrDefault(x => x.isRotationDriver).id;

            if (rotDriver1 != null && rotDriver2 != null)
            {
                if (rotDriver1 != rotDriver2)
                    return false;
            }

            return arr1.Length == arr2.Length &&
                   arr1.All(a => arr2.Any(x => x.id == a.id));
        }

        private void SaveDataToClipboard()
        {
            var json = JsonUtility.ToJson(anchorsConstraintData, true);
            GUIUtility.systemCopyBuffer = json;
        }

        private void LoadDataFromClipboard()
        {
            var savedData = JsonUtility.FromJson<AnchorsConstraintData>(GUIUtility.systemCopyBuffer);
            LoadExternalConstraintData(savedData);
        }

        public void SaveDataToFile()
        {
            _anchorDataSaver.Save(anchorsConstraintData);
        }

        public void LoadDataFromFile()
        {
            _anchorDataSaver.Load(out var savedData);
            LoadExternalConstraintData(savedData);
        }

        private void LoadExternalConstraintData(AnchorsConstraintData constraintData)
        {
            anchorsConstraintData = constraintData;
            prevAnchorsConstraintData = constraintData.Clone();
        }

        [Serializable]
        public class AnchorsConstraintData
        {
            public AnchorOffsets anchorOffsets = new()
            {
                position = Vector3.zero,
                rotation = Quaternion.identity
            };

            public AnchorData[] anchors;

            public AnchorsConstraintData Clone() =>
                new()
                {
                    anchorOffsets = new AnchorOffsets
                    {
                        position = anchorOffsets.position,
                        rotation = anchorOffsets.rotation
                    },
                    anchors = (AnchorData[])anchors.Clone()
                };
        }

        [Serializable]
        public class AnchorOffsets
        {
            public Vector3 position;
            public Quaternion rotation = Quaternion.identity;

            public AnchorOffsets Clone() =>
                new()
                {
                    position = position,
                    rotation = rotation
                };
        }

        [Serializable]
        public struct AnchorData
        {
            public string id;
            public Vector3 position;
            public Quaternion rotation;
            public bool isRotationDriver;
        }
    }
}
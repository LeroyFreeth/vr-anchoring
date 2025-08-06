using System.Linq;
using UnityEngine;

namespace VR
{
    public class CenterPositionForObjects : MonoBehaviour
    {
        [SerializeField] private Transform[] _transforms;
        [SerializeField] private GameObject _centerObject;
    

        private void Update()
        {
            Bounds bounds = GeometryUtility.CalculateBounds(_transforms.Select(x => x.position).Where(x => transform.GetChild(0).gameObject.activeInHierarchy).ToArray(), Matrix4x4.identity);
            _centerObject.transform.position = bounds.center;
        }
    }
}

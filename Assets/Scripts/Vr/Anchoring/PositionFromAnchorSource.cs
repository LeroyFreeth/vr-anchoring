using Anchoring;
using UnityEngine;

namespace Mauritshuis.Scripts.FileManagement
{
    public class PositionFromAnchorSource : MonoBehaviour
    {
        [SerializeField] private AnchorsConstraint[] anchorsConstraints;
        [SerializeField] private int index;

        public void Update()
        {
            foreach (var constraint in anchorsConstraints)
            {
                if (constraint.isActiveAndEnabled)
                {
                    transform.localPosition = constraint.AnchorSources[index].position;
                    transform.localRotation = constraint.AnchorSources[index].rotation;
                }
            }
        }
    }
}
using Anchoring;
using UnityEngine;
using UnityEngine.UI;

namespace Mauritshuis.Scripts.FileManagement
{
    public class LighthouseGUIDebug : MonoBehaviour
    {
        [SerializeField] private AnchorsConstraint[] anchorsConstraints;
        [SerializeField] private Image[] images;

        private void Update()
        {
            foreach (var constraint in anchorsConstraints)
            {
                if (constraint.isActiveAndEnabled)
                {
                    for (var i = 0; i < constraint.AnchorSources.Length; i++)
                        images[i].color = constraint.AnchorSources[i].active ? Color.green : Color.red;
                }
            }
        }
    }
}
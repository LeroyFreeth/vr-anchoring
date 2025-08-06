using UnityEngine;

namespace Anchoring
{
    public class AnchorSource : MonoBehaviour
    {
        public string id => this.name;
        public Vector3 position => transform.position;
        public Quaternion rotation => transform.rotation;
        public bool active => this.enabled && this.gameObject.activeInHierarchy;

        private MeshRenderer meshRenderer;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnEnable()
        {
            if (meshRenderer != null) meshRenderer.enabled = true;
        }


        private void OnDisable()
        {
            if (meshRenderer != null) meshRenderer.enabled = false;
        }
    }
}
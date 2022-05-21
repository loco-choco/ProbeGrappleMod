using UnityEngine;
namespace PGM
{
    public class HookAnchor : MonoBehaviour
    {
        private OWRigidbody hookBody;

        public ProbeGrapleMod HookManager { get; set; }

        private float launchTime;

        private Vector3 hookScale = Vector3.one * 0.4f;

        private void Start()
        {
            hookBody = gameObject.GetRequiredComponent<OWRigidbody>();
            
            launchTime = Time.time;
            
            transform.localScale = hookScale;
        }
        
        private void AttachToObject(GameObject hitObject, Vector3 hitNormal)
        {

            hookBody.transform.parent = hitObject.transform;

            hookBody.MakeKinematic();

            Debug.Log($"Object size (locally) :{hitObject.transform.localScale}");
            transform.localScale = CorrectScale( hitObject.transform.localScale, hookScale);

            foreach (Collider collider in hookBody.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = true;
            }

            hookBody.transform.rotation = Quaternion.FromToRotation(hookBody.transform.forward, -hitNormal) * hookBody.transform.rotation;

            HookManager.IsGrappling = true;
            
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.tag == "Player")
            {
                return;
            }

            bool flag = collision.collider.attachedRigidbody != null && hookBody != null;
            if (flag)
            {
                GameObject gameObject = OWPhysics.GetOtherCollider(this, collision).gameObject;
                AttachToObject(gameObject, collision.contacts[0].normal);
            }
        }
        private Vector3 CorrectScale(Vector3 from, Vector3 to)
        {
            return new Vector3(to.x / from.x, to.y / from.y, to.z / from.z);
        }
    }
}

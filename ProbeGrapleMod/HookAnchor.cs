using UnityEngine;
namespace PGM
{
    public class HookAnchor : MonoBehaviour
    {
        private OWRigidbody hookBody;

        public ProbeGrapleMod HookManager { get; set; }

        private float launchTime;

        

        private void Start()
        {
            hookBody = gameObject.GetRequiredComponent<OWRigidbody>();
            
            launchTime = Time.time;
            
            transform.localScale = new Vector3(0.4f,0.4f,0.4f);
        }
        
        private void AttachToObject(GameObject hitObject, Vector3 hitNormal)
        {

            hookBody.transform.parent = hitObject.transform;

            hookBody.MakeKinematic();

            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

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
    }
}

using UnityEngine;

public class JumpCube : MonoBehaviour
{
    public float JumpForce = 100f;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            Rigidbody rigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if(rigidbody != null)
            {
                rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            }
        }
    }
}

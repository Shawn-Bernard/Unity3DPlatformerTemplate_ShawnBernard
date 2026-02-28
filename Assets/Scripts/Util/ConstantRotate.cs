using Unity.Mathematics;
using UnityEngine;

public class ConstantRotate : MonoBehaviour
{
    public bool randomSpin;
    public float randomMaxDirection = 160;
    public float randomMinDirection = 140;
    Rigidbody rb;
    bool hasRb = false;
    public Vector3 RotationToAdd = new Vector3(0.0f, 0.0f, 0.0f);
    void Awake() {
        hasRb = TryGetComponent(out rb);
        if (randomSpin)
        {
            RotationToAdd = new Vector3(0.0f, 0.0f, 0.0f);
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                RotationToAdd.y = Mathf.Abs(UnityEngine.Random.Range(randomMaxDirection, randomMinDirection));
            }
            else
            {
                RotationToAdd.y = -Mathf.Abs(UnityEngine.Random.Range(randomMaxDirection, randomMinDirection));
            }
        }
    }
    
    void FixedUpdate() {
        if (hasRb) {
            rb.MoveRotation(Quaternion.Euler(rb.transform.rotation.eulerAngles + (RotationToAdd  * Time.deltaTime)));
        } else {
            transform.Rotate(RotationToAdd * Time.deltaTime);
        }
    }
}

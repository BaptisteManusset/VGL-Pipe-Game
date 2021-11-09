using UnityEngine;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour {
    private bool isUsed = false;

    private Rigidbody rb;

    public Pipe GetItem() {
        if (isUsed) return null;
        isUsed = true;
        rb.isKinematic = true;
        transform.position += Vector3.down * 100;
        return GetComponent<Pipe>();
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().AddForceAtPosition(Random.insideUnitSphere, transform.position + Random.insideUnitSphere);
    }
}
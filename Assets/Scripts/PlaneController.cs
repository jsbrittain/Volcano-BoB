using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    private Vector3 pos0;

    // Start is called before the first frame update
    void Start()
    {
        pos0 = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Animate sea going in/out
        Vector3 pos = pos0;
        pos.y = pos0.y + 0.1f * Mathf.Sin(Mathf.PI * 2f * 0.5f * Time.time);
        transform.position = pos;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            Destroy(other.gameObject);
        }
    }
}

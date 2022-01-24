using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslanderController : MonoBehaviour
{

    private float speedWalk = 75f;
    Vector3 offset = new Vector3(0f, 0f, 0f);
    private float propKillDrop = -1f;
    private new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if fallen off map
        if (transform.position.y < propKillDrop)
        {
            Destroy(gameObject);
            return;
        }

        // Movement
        offset.x = offset.x * 0.5f + 0.5f * Random.Range(-0.2f, 0.2f);
        offset.z = offset.z * 0.5f + 0.5f * Random.Range(-0.2f, 0.2f);
        rigidbody.MovePosition(transform.position + speedWalk * Time.deltaTime * offset);
    }
}

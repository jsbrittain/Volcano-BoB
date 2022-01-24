using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropController : MonoBehaviour
{

    // Specifies if consumable (i.e. goat) or non-consumable (i.e. person)
    /*enum PropClass { Consumable, Person };
    [SerializeField]
    PropClass propclass = PropClass.Consumable;*/


    // Other vars
    private bool propBeingCarried = false;

    private float YOffsetNormal = 0f;
    public float YOffsetCarried = 1.5f;
    public float XOffsetCarried = 0.0f;
    public float carryRotation = 90.0f;
    private Vector3 playerPos;

    public int appeaseAmount = 10;

    [SerializeField]
    private float speedWalk = 150f;

    private Vector3 offset = new Vector3(0f, 0f, 0f);
    private new Rigidbody rigidbody;

    [SerializeField]
    private Material[] matAnimation;
    [SerializeField]
    private float walkAnimationGap = 0.2f;
    private int lastAnimFrame = 0;
    private float lastAnimChange;
    private new Renderer renderer;
    private Color localColor = Color.white;
    private Vector3 localScale;

    [SerializeField]
    private AudioClip audioPickup;
    [SerializeField]
    private AudioClip audioSacrifice;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        renderer = gameObject.GetComponentInChildren<Renderer>();
        localScale = transform.localScale;
        lastAnimChange = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if ( propBeingCarried )
        {
            // Update prop to follow player
            Vector3 pos = transform.position;
            pos.x = playerPos.x + XOffsetCarried;
            pos.y = playerPos.y + YOffsetCarried;
            pos.z = playerPos.z;
            transform.position = pos;
        }

        // Animate
        if ((Time.time - lastAnimChange) > walkAnimationGap)
        {
            lastAnimChange = Time.time;
            renderer.material = matAnimation[lastAnimFrame++];
            renderer.material.color = localColor;
            if (lastAnimFrame >= matAnimation.Length)
                lastAnimFrame = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!propBeingCarried)
        {
            // Prop is walking around
            offset.x = offset.x * 0.9f + 0.1f * Random.Range(-0.2f, 0.2f);
            offset.z = offset.z * 0.9f + 0.1f * Random.Range(-0.2f, 0.2f);
            rigidbody.MovePosition(transform.position + speedWalk * Time.deltaTime * offset);
            if (offset.x > 0) {
                localScale.x = -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }
            else
            {
                localScale.x = Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }
        }
    }

    public bool isBeingCarried()
    {
        return propBeingCarried;
    }

    public void setBeingCarried(bool beingCarried)
    {
        this.propBeingCarried = beingCarried;
    }

    public bool AttemptPickup()
    {
        if (!propBeingCarried)
        {
            propBeingCarried = true;
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, carryRotation);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DropProp()
    {
        propBeingCarried = false;
        gameObject.transform.rotation = Quaternion.identity;

        Vector3 pos = transform.position;
        pos.x = playerPos.x + 0.5f;
        pos.y = YOffsetNormal;
        pos.z = playerPos.z + 0.5f;
        transform.position = pos;
    }

    public void updatePlayerPosition( Vector3 playerPos )
    {
        // Update prop about current player position - needed when being carried
        this.playerPos = playerPos;
    }

    public void setColor(Color color)
    {
        localColor = color;
    }

    public AudioClip getAudioClipPickup()
    {
        return audioPickup;
    }

    public AudioClip getAudioClipSacrifice()
    {
        return audioSacrifice;
    }
}

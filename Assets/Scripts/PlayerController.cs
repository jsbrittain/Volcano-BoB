using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private Material matPlayerStand;
    [SerializeField]
    private Material[] matPlayerWalkLeft;
    [SerializeField]
    private Material[] matPlayerWalkRight;
    [SerializeField]
    private Material matPlayerLiftStand;
    [SerializeField]
    private Material[] matPlayerLiftMove;
    [SerializeField]
    private float walkAnimationGap = 0.2f;
    private int animType = 0;
    [SerializeField]
    private GameObject playerImage;
    private int lastAnimFrame = 0;
    private float lastAnimChange;
    private float standingVelocity = 0.001f;
    private new Renderer renderer;

    private int health;
    private int maxHealth = 100;
    private float speedWalk = 8f;

    private new Rigidbody rigidbody;
    private bool GrabButtonUp = true;
    private PropController propController = null;
    private bool inSacrificeArea = false;
    private float pickupRange = 2.0f;
    private float pickupRangeBoat = 3.0f;
    private float YOffsetCarrying = 1.8f;

    [SerializeField]
    private GameObject gameControllerGO;
    private GameController gameController;
    private float propKillDrop = -2.5f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        gameController = gameControllerGO.GetComponent<GameController>();
        health = maxHealth;
        renderer =  gameObject.GetComponentInChildren<Renderer>();
        lastAnimChange = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if fallen off map
        if (transform.position.y < propKillDrop)
        {
            Debug.Log("Player fell off island");
            gameController.playerDied();
            return;
        }

        // Grab/drop
        if (Input.GetKey(KeyCode.Space) && (GrabButtonUp))
        {
            if (carryingProp())
            {
                AttemptSacrificeOrDropProp();
            }
            else
            {
                AttemptPickupProp();
            }
            GrabButtonUp = false;
        }
        if (!Input.GetKey(KeyCode.Space))
        {
            GrabButtonUp = true;
        }

        // Update prop to follow movement
        if (propController != null )
        {
            propController.updatePlayerPosition(transform.position + Vector3.up*YOffsetCarrying);
        }

        // Animate movement
        if ((Time.time - lastAnimChange) > walkAnimationGap)
        {
            lastAnimChange = Time.time;
            float horzInput = Input.GetAxis("Horizontal");
            float vertInput = Input.GetAxis("Vertical");
            if (carryingProp())
            {
                // Animate when carrying something
                if (( Mathf.Abs(horzInput) < standingVelocity ) && (Mathf.Abs(vertInput) < standingVelocity))
                {
                    // Standing still
                    if (animType != 7) lastAnimFrame = 0; animType = 7;
                    renderer.material = matPlayerLiftStand;
                    lastAnimFrame = 0;
                } else
                {
                    // Moving
                    if (animType != 2) lastAnimFrame = 0; animType = 2;
                    renderer.material = matPlayerLiftMove[lastAnimFrame++];
                    if (lastAnimFrame >= matPlayerLiftMove.Length)
                        lastAnimFrame = 0;
                }
            }
            else
            {
                // Animate when not carrying anything
                if (horzInput > standingVelocity)
                {
                    // Right
                    if (animType != 3) lastAnimFrame = 0; animType = 3;
                    renderer.material = matPlayerWalkRight[lastAnimFrame++];
                    if (lastAnimFrame >= matPlayerWalkRight.Length)
                        lastAnimFrame = 0;
                }
                else if (horzInput < -standingVelocity)
                {
                    // Left
                    if (animType != 4) lastAnimFrame = 0; animType = 4;
                    renderer.material = matPlayerWalkLeft[lastAnimFrame++];
                    if (lastAnimFrame >= matPlayerWalkLeft.Length)
                        lastAnimFrame = 0;
                }
                else
                {
                    if (vertInput > standingVelocity)
                    {
                        // Left
                        if (animType != 5) lastAnimFrame = 0; animType = 5;
                        renderer.material = matPlayerWalkLeft[lastAnimFrame++];
                        if (lastAnimFrame >= matPlayerWalkLeft.Length)
                            lastAnimFrame = 0;
                    }
                    else if (vertInput < -standingVelocity)
                    {
                        // Right
                        if (animType != 6) lastAnimFrame = 0; animType = 6;
                        renderer.material = matPlayerWalkRight[lastAnimFrame++];
                        if (lastAnimFrame >= matPlayerWalkRight.Length)
                            lastAnimFrame = 0;
                    }
                    else
                    {
                        // Stand
                        if (animType != 7) lastAnimFrame = 0; animType = 7;
                        renderer.material = matPlayerStand;
                        lastAnimFrame = 0;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 userInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        rigidbody.MovePosition(transform.position + userInput * Time.deltaTime * speedWalk);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SacrificeArea") )
        {
            inSacrificeArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SacrificeArea"))
        {
            inSacrificeArea = false;
        }
    }

    private bool carryingProp()
    {
        return propController != null;
    }

    private void AttemptSacrificeOrDropProp()
    {
        // If within sacrifice zone, sacrifice, else drop
        if (inSacrificeArea)
        {
            gameController.SacrificeProp( propController );
            gameController.audioPlay(propController.getAudioClipSacrifice());
        }
        else
        {
            DropProp();
        }
        return;
    }

    private void DropProp()
    {
        if ( propController != null)
        {
            propController.DropProp();
            propController = null;
        }
        return;
    }


    private void AttemptPickupProp()
    {
        // Check if nex to boat, and if volcano is appeased
        GameObject boat = gameController.getBoat();
        float dst = Vector3.Distance(transform.position, boat.transform.position);
        if ( dst < pickupRangeBoat )
        {
            // Check if volcano appeased
            if ( gameController.isVolcanoAppeased() )
            {
                // End level
                gameController.PassLevel();
                return;
            }
        }

        // Find closest prop - is it within pickup range?
        GameObject closestProp = null;
        float currentDistance = Mathf.Infinity;
        // Get gameobjects
        List<GameObject> propGOs = gameController.getPropsList();
        foreach (GameObject prop in propGOs)
        {
            dst = Vector3.Distance(transform.position, prop.transform.position);
            if ((dst < pickupRange) && (dst < currentDistance))
            {
                closestProp = prop;
                currentDistance = dst;
            }
        }

        // Check if no props in range
        if (closestProp == null)
            return;

        // Pickup closest prop in range
        propController = (PropController) closestProp.GetComponent("PropController");
        if (!propController.AttemptPickup())
            propController = null;
        else
            gameController.audioPlay( propController.getAudioClipPickup() );

        return;
    }

}

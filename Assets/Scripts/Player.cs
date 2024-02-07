using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField] InputActionReference movementInput;
    [SerializeField] InputActionReference jumpInput;
    [SerializeField] InputActionReference grappleInput;
    [SerializeField] InputActionReference reelInInput;



    [Header("Grapple Line")]
    public LineRenderer grappleLine;


    [Header("Grapple")]
    public float maxGrappleDis;
    public float grappleCoolDown;
    public float grappleSpringDis;
    public float maxSpeed;
    public float springForce;
    public float grappleForce;
    public float grappleSpeed;


    public bool canGrapple;
    private bool readyToGrapple;
    private bool isGrappled;

    private float grappleDis;
    private float grappleForceMult;
    private float springForceMult;

    //public LayerMask ignoreRaycast;

    private RaycastHit hit;

    public Transform grappleLook;

    public GameObject hitPointVisual;

    private Vector3 grapplePoint;

    

    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool isGrounded;

    public Transform orientation;

    float horizontalInput;
    float verticleInput;

    Vector3 moveDirection;


    public GameObject player;
    public Camera playerCamera;
    private Rigidbody playerRigidbody;

    



    private void Start()
    {
        grappleLine.enabled = false;
        hitPointVisual.SetActive(false);
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        readyToJump = true;
        readyToGrapple = true;
        isGrappled = false;
        
    }

    private void Update()
    {
        

        
        canGrapple = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxGrappleDis);

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        MyInput();
        SpeedControl();

        if (isGrounded)
        {
            playerRigidbody.drag = groundDrag;
        }else
        {
            playerRigidbody.drag = 0f;
        }

        if(isGrappled)
        {

            

            hitPointVisual.transform.position = Vector3.MoveTowards(hitPointVisual.transform.position, grapplePoint, grappleSpeed * Time.deltaTime);

            grappleDis = Vector3.Distance(transform.position, grapplePoint);

            grappleLine.enabled = true;

            grappleLine.SetPosition(0, transform.position);
            grappleLine.SetPosition(1, hitPointVisual.transform.position);

            
            

        

            if (grappleDis >= grappleSpringDis + 5f) // checks if player is farther away than max distance with wiggle room
            {
                springForceMult = grappleDis;
                springForceMult = Mathf.Clamp(springForceMult, 1f, 5f);
             
                Vector3 grappleDir = grapplePoint - transform.position; // finds grapple angle

                playerRigidbody.AddForce(grappleDir.normalized * springForceMult, ForceMode.VelocityChange); //applies a springforce * playerRigidbody.velocity.magnitude  * -(playerRigidbody.mass * Physics.gravity.y)
             
            }

            if (grappleDis >= maxGrappleDis + 30f || playerRigidbody.velocity.magnitude >= maxSpeed) // if above a certain speed or distance stop grappling
            {
                DisableGrapple();
            }

            

        }
    }


    private void FixedUpdate()
    {
        Movement();
    }


    private void Movement()
    {
        // calc move dir
        moveDirection = orientation.forward * verticleInput + orientation.right * horizontalInput;

        //on ground
        if (isGrounded)
        {
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);

        }

        else if (!isGrounded)
        {
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10 * airMultiplier, ForceMode.Force);

        }
    }

    private void MyInput()
    {
        

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticleInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCoolDown);
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            DisableGrapple();
        }

        if(Input.GetKey(KeyCode.Mouse1) && isGrappled)
        {
            Grapple();
        }

        if (Input.GetKey(KeyCode.Mouse0) && canGrapple && readyToGrapple && hit.transform != null)
        {
            grappleForceMult = 0f;
            grappleSpringDis = maxGrappleDis; // reset grapple spring distance

            hitPointVisual.transform.position = transform.position; // teleport the grappling hook to raycast hit point
            
            hitPointVisual.SetActive(true);
            readyToGrapple = false;
            grapplePoint = hit.point;

            grappleDis = Vector3.Distance(transform.position, grapplePoint);
            isGrappled = true;

            grappleSpringDis = grappleDis;

            Invoke(nameof(ResetGrapple), grappleCoolDown);
        }
        
    }


    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);


        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

        playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Grapple()
    {
        

        if (grappleSpringDis >= grappleDis) // checks if max distance is larger than current distance from grapple 
        {
            grappleSpringDis = grappleDis;   // Sets the max distance that player can be away from grapple point to current Distance from point 

        }

        grappleForceMult += Time.deltaTime;

        grappleForceMult = Mathf.Clamp(grappleForceMult, 1f, 10f);

        Vector3 grappleDir = grapplePoint - transform.position; // find grapple angle
        playerRigidbody.AddForce(grappleDir.normalized * grappleForce * grappleForceMult, ForceMode.Force);
    

    }

    private void DisableGrapple()
    {
        hitPointVisual.SetActive(false);
        isGrappled = false;
        grappleLine.enabled = false;
    }

    private void ResetGrapple()
    {
        readyToGrapple = true;
    }
}

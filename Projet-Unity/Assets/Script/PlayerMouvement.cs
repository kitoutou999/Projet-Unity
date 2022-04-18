using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    [Header("Mouvement")]
    private float moveSpeed;
    public float walkSpeed;
    public float backSpeed;
    public float strafeSpeed;




    public float sprintSpeed;

    public float groundDrag;

    [Header("Saut")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("S'accroupir")]
    public float crouchSpeed;
    public CapsuleCollider playerCollider ;

    [Header("Keybinds")]
    public KeyCode walkKey = KeyCode.Z;
    public KeyCode leftKey = KeyCode.Q;
    public KeyCode backKey = KeyCode.S;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    bool grounded;

    [Header("Verif Angle Pente")]
    public float maxPenteAngle;
    private RaycastHit penteHit;
    private bool sortiePente;
    

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        idle,
        air
    }

    private Animator anim;
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        state = MovementState.walking;
        moveSpeed = walkSpeed;
        
        rb.freezeRotation = true;

        readyToJump = true;

        
        
    }
    RaycastHit hit;
    private bool uncrouch;
    private void Update()
    {
        // unsneak check
        if (Physics.Raycast(transform.position, transform.up,out hit,1.8f))
        {
            Debug.Log("Le Raycast(top) touche un objt");
            uncrouch = false;

        }
        else{
            uncrouch =true;
        }


        if (Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround))
        {
            Debug.Log("touche sol");
            grounded = true;

        }
        else{
            grounded =false;
        }
        // ground check
        //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            anim.SetBool("Ground",true);
            rb.drag = groundDrag;
        }
        else
        {
            anim.SetBool("Ground",false);
            rb.drag = 0;
        }
            
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Si le joueur press espace et est au sol et est pret a sauter alors il saute avec un cooldown definie
        if(Input.GetKey(jumpKey) && readyToJump && grounded )
        {
            readyToJump = false;
            anim.SetBool("Jump",true);
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        
        // Fin de l'accroupie qui nous retrecie
        
        if (!Input.GetKeyDown(crouchKey) && uncrouch)
        {
            playerCollider.height = 2;
            playerCollider.center = new Vector3(0.03360176f,0.8391673f,0.1618079f);
            anim.SetBool("CrouchIdle",false);
        }
    }
    
    
    void StateHandler()
    {


        if (grounded)
        {   
            
            //Animation de marche a droite
            if(Input.GetKey(rightKey) && !Input.GetKey(leftKey))
                anim.SetBool("StrafeRight",true);
            else
                anim.SetBool("StrafeRight",false);

            //Animation de marche a gauche
            if(Input.GetKey(leftKey) && !Input.GetKey(rightKey))
                anim.SetBool("StrafeLeft",true);
            else
                anim.SetBool("StrafeLeft",false);

            //Animation de marche en avant
            if (Input.GetKey(walkKey) && !Input.GetKey(backKey))
            {
                //Animation de sprint
                if (Input.GetKey(sprintKey) && !Input.GetKey(rightKey) && !Input.GetKey(leftKey) && !Input.GetKey(crouchKey))
                {
                    anim.SetBool("Sprint",true);
                    state = MovementState.sprinting;
                    moveSpeed = sprintSpeed;
                }
                else
                {
                    state = MovementState.walking;
                    moveSpeed = walkSpeed;
                    anim.SetBool("RunForward",true);
                    anim.SetBool("Sprint",false);
                }
                
            }
            else
            {
                anim.SetBool("RunForward",false);
                anim.SetBool("Sprint",false);
            }
                
            //Animation de recule
            if (Input.GetKey(backKey) && !Input.GetKey(walkKey))
            {
                state = MovementState.walking;
                moveSpeed = backSpeed;
                anim.SetBool("RunBackward",true);
            }
            else{
                anim.SetBool("RunBackward",false);
            }
            
            if(!Input.GetKey(crouchKey) && uncrouch)
            {
                anim.SetBool("CrouchIdle",false);
                anim.SetBool("Crouchwalk",false);
            }

            else if (Input.GetKey(crouchKey))
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
                playerCollider.height = 1.4f;
                playerCollider.center = new Vector3(0.03360176f,0.5f,0.1618079f);

                
                
                if (Input.GetKey(rightKey) || Input.GetKey(leftKey) || Input.GetKey(walkKey) || Input.GetKey(backKey))
                {
                    anim.SetBool("Crouchwalk",true);
                }
                else
                {
                    anim.SetBool("CrouchIdle",true);
                    anim.SetBool("Crouchwalk",false);
                }
                
            }
            else
            {
                if (!uncrouch)
                {
                    state = MovementState.crouching;
                    moveSpeed = crouchSpeed;
                    if (Input.GetKey(rightKey) || Input.GetKey(leftKey) || Input.GetKey(walkKey) || Input.GetKey(backKey))
                    {
                        anim.SetBool("Crouchwalk",true);
                    }
                    else
                    {
                        anim.SetBool("Crouchwalk",false);
                        anim.SetBool("CrouchIdle",true);
                        
                    }
                    
                }
            }
            

        }
        else if (!grounded && !readyToJump)
        {
            anim.SetBool("Jump",false);

        }
        
        
        

    }

       

    private void MovePlayer()
    {
        // calcule des mouvent et de la direction(universelle code)
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Sur une pente (resolution bug pour monter une pente trop incliner et rendu realiste)
        if (SurPente() && !sortiePente)
        {
            rb.AddForce(GetPenteMoveDirection() * moveSpeed * 30f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // touche le sol
        else if(grounded )
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // est dans les air
        else if(!grounded )
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        
        
   

        // Suppretion de la gravité pour les pentes
        rb.useGravity = !SurPente();
    }

    private void SpeedControl()
    {
        // ralentissement de vitesse dans les pente
        if (SurPente() && !sortiePente)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limitation de vitesse (acceleration infinie et exponentiel)
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        sortiePente = true;
        
        

        // reset y vitesse pour des saut equivalent
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
    }
    private void ResetJump()
    {
        readyToJump = true;

        sortiePente = false;
    }

    private bool SurPente()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out penteHit, playerHeight * 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, penteHit.normal);
            return angle < maxPenteAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetPenteMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, penteHit.normal).normalized;
    }

    
    
}

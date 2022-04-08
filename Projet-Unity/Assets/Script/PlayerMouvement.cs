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
    public float swimSpeedafk;
    public float swimSpeed;



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
    public LayerMask whatIsWater;

    bool grounded;
    bool swim;

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
        swim,
        idle,
        air
    }

    private Animator anim;
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;

        readyToJump = true;

        
        
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        swim = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsWater);


        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
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
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();
            

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Debut de l'accroupie qui nous retrecie
        
        if (!Input.GetKeyDown(crouchKey))
        {
            playerCollider.height = 2;
            playerCollider.center = new Vector3(0.03360176f,0.8391673f,0.1618079f);
        }
        

        
    }
    
    
    private void StateHandler()
    {
        // Mode - Accroupie = reduction de vitesse et state
        if (Input.GetKey(crouchKey) && !swim)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;


            playerCollider.height = 1.4f;
            playerCollider.center = new Vector3(0.03360176f,0.5f,0.1618079f);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);


            anim.SetFloat("Speed",8);
        }

        // Mode - sprint = Augmentation de vitesse
        else if(grounded && !swim && Input.GetKey(sprintKey) && Input.GetKey(walkKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            anim.SetFloat("Speed",6);

        }

        // Mode - Marche = Vitesse de marche normal si on appuie sur aucune touche hors deplacement
        else if (grounded && !swim && Input.GetKey(walkKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",11);
        }
        // Decalage gauche
        else if (grounded && !swim && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = strafeSpeed;
            anim.SetFloat("Speed",7);
        }
        // Decalage droit
        else if (grounded && !swim && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = strafeSpeed;
            anim.SetFloat("Speed",10);
        }
        // recule
        else if (grounded && !swim && Input.GetKey(backKey))
        {
            state = MovementState.walking;
            moveSpeed = backSpeed;
            anim.SetFloat("Speed",9);
        }
        // recule a gauche
        else if (grounded && !swim && Input.GetKey(backKey) && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = backSpeed;
            anim.SetFloat("Speed",3);
        }
        // recule a droit
        else if (grounded && !swim && Input.GetKey(backKey) && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = backSpeed;
            anim.SetFloat("Speed",4);
        }
        // avance a droit
        else if (grounded  && !swim && Input.GetKey(walkKey) && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",2);
        }
        // avance a gauche
        else if (grounded && !swim && Input.GetKey(walkKey) && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",1);
        }
        // Mode - Air = quand on ne touche pas le sol(ne peut sauter)
        else if (!grounded && !swim)
        {
            state = MovementState.air;
            anim.SetFloat("Speed",5);    
        }
        // Mode - swim = quand on ne touche pas le sol mais de l'eau
        else if (swim && !Input.GetKey(sprintKey) )
        {
            anim.SetFloat("Speed",12); 
            state = MovementState.swim;
            moveSpeed = swimSpeedafk;  
        }
        else if (swim && Input.GetKey(sprintKey) )
        {
            anim.SetFloat("Speed",13); 
            state = MovementState.swim;
            moveSpeed = swimSpeed;  
        }

        // Idle(afk)
        else 
        { 
            anim.SetFloat("Speed",0);
            state = MovementState.idle;
            moveSpeed = walkSpeed;  
        }


    }

    private void MovePlayer()
    {
        // calcule des mouvent et de la direction(universelle code)
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Sur une pente (resolution bug pour monter une pente trop incliner et rendu realiste)
        if (SurPente() && !sortiePente)
        {
            rb.AddForce(GetPenteMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // touche le sol
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // est dans les air
        else if(!grounded)
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

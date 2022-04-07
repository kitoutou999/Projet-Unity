using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    [Header("Mouvement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Saut")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("S'accroupir")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

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
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

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
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Arret de l'accroupie et on reprend notre forme du debut
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    
    
    private void StateHandler()
    {
        // Mode - Accroupie = reduction de vitesse et state
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            anim.SetFloat("Speed",8);
        }

        // Mode - sprint = Augmentation de vitesse
        else if(grounded && Input.GetKey(sprintKey) && Input.GetKey(walkKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            anim.SetFloat("Speed",6);

        }

        // Mode - Marche = Vitesse de marche normal si on appuie sur aucune touche hors deplacement
        else if (grounded && Input.GetKey(walkKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",11);
        }
        // Decalage gauche
        else if (grounded && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",7);
        }
        // Decalage droit
        else if (grounded && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",10);
        }
        // recule
        else if (grounded && Input.GetKey(backKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",9);
        }
        // recule a gauche
        else if (grounded && Input.GetKey(backKey) && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",3);
        }
        // recule a droit
        else if (grounded && Input.GetKey(backKey) && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",4);
        }
        // avance a droit
        else if (grounded && Input.GetKey(walkKey) && Input.GetKey(rightKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",2);
        }
        // avance a gauche
        else if (grounded && Input.GetKey(walkKey) && Input.GetKey(leftKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            anim.SetFloat("Speed",1);
        }
        // Mode - Air = quand on ne touche pas le sol(ne peut sauter)
        else if (!grounded )
        {
            state = MovementState.air;
            anim.SetFloat("Speed",5);    
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

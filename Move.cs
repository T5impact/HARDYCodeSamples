using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    //Physics based movement system
    //with customizable limited air controls and drag

    Rigidbody2D rb;
    Animator animator;

    [SerializeField] Transform visuals;
    [SerializeField] Transform flipBody;
    [SerializeField] float speed;
    [SerializeField] float maxSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float flipSmoothing = 2f;
    [SerializeField] [Range(0, 1)] float airControlerLimiter; //Limit to controlling player movement in air
    [SerializeField] [Range(0,1)] float drag;
    [SerializeField] float height;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float timeBefCannotJump;
    [SerializeField] ParticleSystem moveFX;

    public static bool isGrounded;
    public static bool isFlipped;
    bool groundTracker;
    [HideInInspector]
    public float leftRight; //left and right input from player
    bool jump;

    float timer;
    float flip;

    float inputTracker;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        groundTracker = isGrounded;
    }

    bool jumped;

    // Update is called once per frame
    void Update()
    {
        if (SceneController.state == GameState.PAUSEMENU || SceneController.state == GameState.DIE) return;

        isGrounded = CheckGrounding();

        leftRight = Input.GetAxisRaw("Horizontal");
        flip = Mathf.Lerp(flip, leftRight, Time.deltaTime * flipSmoothing);

        //Allow player to jump a little bit after leaving the ground
        //Ensures smoother and more accurate jumping
        if (groundTracker != isGrounded)
        {
            timer = Time.time + timeBefCannotJump;
        }

        if (leftRight != 0)
            GameEvent.instance.Moving();

        //Input detection and animation controls
        if (isGrounded && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && jumped == false)
        {
            jump = true;
            animator.SetTrigger("Jump");
            GameEvent.instance.Jump();

            jumped = true;
        }
        else if (Time.time <= timer && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && jumped == false)
        {
            jump = true;
            animator.SetTrigger("Jump");
            GameEvent.instance.Jump();

            jumped = true;
        }

        if(groundTracker == false && isGrounded)
        {
            animator.SetTrigger("Land");
            GameEvent.instance.Land();
            jumped = false;
        }


        groundTracker = isGrounded;
        inputTracker = leftRight;

        //Play sound effects
        if (isGrounded)
            moveFX.Play();
        else
            moveFX.Stop();
    }

    private void LateUpdate()
    {
        //Flip the character to match the direction of travelled
        if (flip >= 0)
        {
            isFlipped = false;
            visuals.localScale = new Vector3(1, visuals.localScale.y, 1);
            flipBody.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            isFlipped = true;
            visuals.localScale = new Vector3(-1, visuals.localScale.y, 1);
        }
    }

    private void FixedUpdate()
    {
        if (SceneController.state == GameState.PAUSEMENU) return;

        if (SceneController.state == GameState.DIE) {rb.velocity = Vector3.zero; return;}

        //Apply momentum based movement to the character based on input
        //and whether player is on ground
        if(isGrounded == true)
        {
            rb.velocity += new Vector2(leftRight * speed * Time.fixedDeltaTime, 0);

            //Check if player has reached max speed;
            if(Mathf.Abs(rb.velocity.x) > maxSpeed)
            {
                if(flip >= 0.5f)
                    rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
                else if (flip <= -0.5f)
                    rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
            }
        } else
        {
            rb.velocity += new Vector2(leftRight * speed * Time.fixedDeltaTime, 0) * airControlerLimiter;
        }

        //Apply upwards velocity when jumping
        if(jump == true)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jump = false;
        }

        //Apply drag to player if no player input and grounded
        if(leftRight == 0 && isGrounded)
        {
            Vector2 vel = rb.velocity;
            vel.x *= drag;
            rb.velocity = vel;
        }

        animator.SetFloat("x", rb.velocity.x / maxSpeed);
    }

    //Use raycast to check if on ground
    bool CheckGrounding()
    {
        return Physics2D.Raycast(transform.position, -transform.up, height, whatIsGround);
    }
}

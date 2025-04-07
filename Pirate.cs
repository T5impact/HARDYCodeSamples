using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pirate : MonoBehaviour
{
    [SerializeField] GameObject visuals;
    [SerializeField] GameObject visuals2;

    [Header("Movement Settings")]
    [SerializeField] float speed;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] Transform groundCheckLeft;
    [SerializeField] Transform groundCheckRight;
    [SerializeField] float flipDelay = 1f;

    [Header("Targeting Settings")]
    [SerializeField] [Range(0, 1)] float minViewValue;
    [SerializeField] [Range(0, 1)] float maxViewValue;
    [SerializeField] float memoryTimer = 2f;
    [SerializeField] float rateOfFire = 1;
    [SerializeField] float laserLifeTime = 5f;
    [SerializeField] [Range(0.001f,5)] float difficultLevel = 1;
    [SerializeField] Transform gun;
    [SerializeField] Transform gunEnd;
    [SerializeField] GameObject laserPrefab;
    [SerializeField] ParticleSystem laserFX;
    [Space]
    public Vector3 viewDirection;
    [Space]
    [SerializeField] bool debug = true;

    Vector3 toPlayer;

    bool isFlipped = false;
    bool moving;
    bool isPlayerInRange;
    bool isPlayerTargeted;
    bool playerTargetingTracker;

    GameObject player;

    Rigidbody2D rb;
    Rigidbody2D playerRigid;

    float dot;
    float dotTracker;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        moving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneController.state == GameState.PAUSEMENU || SceneController.state == GameState.DIE)
            return;

        #region Control Visuals
        if (visuals != null)
        {
            if (isFlipped == false)
            {
                visuals.transform.localScale = new Vector3(1, 1, 1);
                visuals2.transform.localScale = new Vector3(1, 1, 1);
                laserFX.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                visuals.transform.localScale = new Vector3(-1, 1, 1);
                visuals2.transform.localScale = new Vector3(-1, 1, 1);
                laserFX.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        #endregion

        Movement();

        DetermineTargeting();

        if (isPlayerTargeted)
        {
            Targeting();
        } else
        {
            if(gun != null)
                gun.rotation = Quaternion.identity;
        }

        playerTargetingTracker = isPlayerTargeted;
        dotTracker = dot;
    }

    float timer;

    //Target the player by aiming the gun at them and firing at some defined rate
    void Targeting()
    {
        moving = false;

        if (gun != null)
        {
            gun.rotation = Quaternion.FromToRotation(viewDirection, toPlayer);
        }

        if (dot < 0 && dotTracker >= 0)
        {
            FlipCharacter();
        }

        if(Time.time > timer)
        {
            Shoot();
            timer = Time.time + (1 / rateOfFire);
        }
    }

    //Shoot the laser by instantiating it
    void Shoot()
    {
        GameEvent.instance.Shoot();
        laserFX.Play();
        var laser = Instantiate(laserPrefab, gunEnd.transform.position, gun.transform.rotation).GetComponent<Laser>();
        laser.Initialize(toPlayer, laserLifeTime, difficultLevel);
    }

    //Use the dot product of two unit vectors to determine if the enemy is facing the player
    void DetermineTargeting()
    {
        if (isPlayerInRange == false)
            return;

        toPlayer = (player.transform.position - transform.position).normalized;
        Vector3 forward = viewDirection;

        dot = Vector3.Dot(toPlayer, forward);

        if(dot >= minViewValue && dot <= maxViewValue)
        {
            isPlayerTargeted = true;
        }
    }

    //Check if enemy should flip around and move the other way
    //Use an overlap check to see if there is any more space to move in front of the enemy
    private void Movement()
    {
        if (isFlipped == false && moving)
        {
            bool check = GroundCheck(groundCheckRight.position);
            if (check == false)
            {
                StartCoroutine(Flip());
            }
        }
        else if (isFlipped == true && moving)
        {
            bool check = GroundCheck(groundCheckLeft.position);
            if (check == false)
            {
                StartCoroutine(Flip());
            }
        }
    }

    //Check if player is in the enemy's range
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = collision.gameObject;
            playerRigid = player.GetComponent<Rigidbody2D>();
        }
    }

    //Check if player leaves the enemy's range
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null;
            playerRigid = null;
            isPlayerTargeted = false;
            moving = true;
        }
    }

    //Move the enemy by setting their velocity
    private void FixedUpdate()
    {
        if (SceneController.state == GameState.PAUSEMENU)
            return;

        if (moving)
        {
            rb.velocity = (viewDirection * speed * Time.deltaTime) + new Vector3(0, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    IEnumerator Remember()
    {
        yield return new WaitForSeconds(memoryTimer);
    }

    IEnumerator Flip()
    {
        moving = false;

        yield return new WaitForSeconds(flipDelay);

        viewDirection = -viewDirection;

        isFlipped = !isFlipped;

        moving = true;
    }

    void FlipCharacter()
    {
        viewDirection = -viewDirection;

        isFlipped = !isFlipped;
    }

    bool GroundCheck(Vector3 pos)
    {
        return Physics2D.OverlapCircle(pos, groundCheckRadius, whatIsGround);
    }

    //Visualize the view and checks of the enemy for debugging
    private void OnDrawGizmos()
    {
        if (groundCheckLeft == null || groundCheckRight == null || debug == false)
            return;

        Gizmos.DrawLine(transform.position, transform.position + viewDirection);
        Gizmos.DrawWireSphere(groundCheckLeft.position, groundCheckRadius);
        Gizmos.DrawWireSphere(groundCheckRight.position, groundCheckRadius);
    }
}

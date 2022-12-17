using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 10f;  
    [SerializeField] float stepRate;
    [SerializeField] AudioSource footstepSource;

    private Vector2 movement;
    private float stepCoolDown;

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        //input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
            
        //animations
        if(movement != Vector2.zero)
            animator.SetBool("Move", true);     
        else
            animator.SetBool("Move", false);

        //player flip
        if (movement.x < 0)
        {
            ServerFlipSprite(true);
        }       
        else if (movement.x > 0)
        {
            ServerFlipSprite(false);
        }           

        //footsteps
        stepCoolDown -= Time.deltaTime;

        if (movement != Vector2.zero && stepCoolDown < 0f)
        {
            PlayFootStepAudio();
            stepCoolDown = stepRate;
        }
    }

    [ServerRpc]
    private void ServerFlipSprite(bool status)
    {
        FlipSprite(status);
    }

    [ObserversRpc]
    private void FlipSprite(bool status)
    {
        if (status)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void PlayFootStepAudio()
    {
        footstepSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        footstepSource.Play();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.rigidbody.velocity = Vector3.zero;
        collision.otherRigidbody.velocity = Vector3.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((player.invincible) && collision.gameObject.tag == "Enemy")
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, false);
        }
    }
}
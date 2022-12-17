using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.UI;

public class Enemy : EnemyData
{
    private float currentHealth;

    private Transform player;
    [SerializeField] private ParticleSystem deathEffect;
    private Animator animator;
    private GameObject healthBar;
    private SpriteRenderer sprite;
    private GameObject hitbox;
    private Rigidbody2D rb;
    private bool isDead;
    private bool chasePlayer = true;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        hitbox = transform.GetChild(1).gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (GetClosestPlayer() != null)
            player = GetClosestPlayer().transform;

        if (isDead || player == null)
        {
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -90 && angle <= 90)
        {
            ServerFlipSprite(false);
        }
        else
        {
            ServerFlipSprite(true);
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (isDead || player == null)
        {
            return;
        }

        if (chasePlayer)
        {         
            Vector3 movePosition = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(movePosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerFlipSprite(bool status)
    {
        FlipSprite(status);
    }

    [ObserversRpc]
    private void FlipSprite(bool status)
    {
        if (status)
            sprite.flipX = true;
        else
            sprite.flipX = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            chasePlayer = false;
            Attack();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            chasePlayer = true;
            animator.SetBool("Attack", false);
        }
    }

    GameObject GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float minimumDistance = int.MaxValue;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < minimumDistance)
            {
                closestPlayer = player;
                minimumDistance = distanceToPlayer;
            }
        }

        return closestPlayer;
    }

    void Attack()
    {
        animator.SetBool("Attack", true);
        Debug.Log("Hit player");
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        SoundManager.Instance.Play("ZombieHurt");
        SetHealthBar();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [ObserversRpc]
    public virtual void Die()
    {
        isDead = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        hitbox.GetComponent<Collider2D>().enabled = false;
        deathEffect.Play();
        StartCoroutine(Destroy(deathEffect.main.duration));
    }

    IEnumerator Destroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    void SetHealthBar()
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<Image>().fillAmount = (float)currentHealth / maxHealth;
        if (healthBar.GetComponentInChildren<Image>().fillAmount <= 0)
            healthBar.gameObject.SetActive(false);
    }
}

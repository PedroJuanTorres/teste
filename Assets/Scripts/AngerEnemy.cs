﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngerEnemy : MonoBehaviour
{
    [SerializeField]private float       moveDirection = -1.0f;
    [SerializeField]private float       moveSpeed = 100.0f;
    [SerializeField]private float       maxTimeMoving = 2.2f;
    [SerializeField]private Collider2D  monsterCollider;
    [SerializeField]private Collider2D  punchCollider;
    [SerializeField]private int         maxHealth = 2;
    [SerializeField]private float       knockbackVelocity = 400.0f;
    [SerializeField]private float       knockbackDuration = 0.25f;
    [SerializeField]private Transform   wallCheckObject;
    [SerializeField]private float       wallCheckRadius = 3.0f;
    [SerializeField]private LayerMask   wallCheckLayer;
    [SerializeField]private float       timeOfAttack = 0.5f;
    [SerializeField]private Transform   playerCheckObject;
    [SerializeField]private float       playerCheckRadius = 3.0f;
    [SerializeField]private LayerMask   playerCheckLayer;
    [SerializeField]private float       timeOfRespawn = 20.0f;

    private Rigidbody2D rb;
    private GameManager gm;
    private Animator    animator;
    private float       timeLeft;
    private float       timeLeftAttacking;
    private float       timeLeftRespawn;
    private int         emotionState;
    private bool        isAngerMonster = false;
    private int         health;
    private float       knockbackTimer;
    private bool        isRespawning = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        
        timeLeft = maxTimeMoving;

        timeLeftAttacking = timeOfAttack;

        timeLeftRespawn = timeOfRespawn;

        gm = FindObjectOfType<GameManager>();

        monsterCollider = GetComponent<Collider2D>();

        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D wallCollider = Physics2D.OverlapCircle(wallCheckObject.position, wallCheckRadius, wallCheckLayer);

        bool isWall = (wallCollider != null);

        Collider2D playerCollider = Physics2D.OverlapCircle(playerCheckObject.position, playerCheckRadius, playerCheckLayer);

        bool playerInRange = (playerCollider != null);

        Vector2 currentVelocity = rb.velocity;

        currentVelocity.x = moveDirection * moveSpeed;

        rb.velocity = currentVelocity;

        timeLeft = timeLeft - Time.deltaTime;

        if(playerInRange && isAngerMonster)
        {
            timeLeftAttacking = timeLeftAttacking - Time.deltaTime;
            if(timeLeftAttacking < 0.6) 
            {
                punchCollider.enabled = true;
            } 
            if(timeLeftAttacking < 0)
            {
                animator.SetBool("IsAttacking",false);

                timeLeft = -0.1f;

                playerInRange = false;

                timeLeftAttacking = timeOfAttack;

                punchCollider.enabled = false;

            }
            else
            {
                currentVelocity.x = 0.0f;

                rb.velocity = currentVelocity;

                animator.SetBool("IsAttacking",true);
            }
        }
        else
        {
            animator.SetBool("IsAttacking",false);
            punchCollider.enabled = false;
        }
        if(timeLeft < 0 || isWall && !playerInRange)
        {
            moveDirection = moveDirection * (-1);
            if(moveDirection>0)
            {
                Vector3 currentRotation = transform.rotation.eulerAngles;
                currentRotation.y = 180;
                transform.rotation = Quaternion.Euler(currentRotation);
            }
            if(moveDirection<0)
            {
                Vector3 currentRotation = transform.rotation.eulerAngles;
                currentRotation.y = 0;
                transform.rotation = Quaternion.Euler(currentRotation);
            }
            
            timeLeft = maxTimeMoving;
        }


        if(knockbackTimer > 0)
        {
            knockbackTimer = knockbackTimer - Time.deltaTime;
        }

        if(isRespawning)
        {
            timeLeftRespawn = timeLeftRespawn - Time.deltaTime;

            if(timeLeftRespawn < 0)
            {
                isRespawning = false;

                timeLeftRespawn = timeOfRespawn;
            }
        }

        emotionState = gm.GetCurrentEmotion();
        if (emotionState == 1 && !isRespawning)
        {
            isAngerMonster = true;
            monsterCollider.enabled = true;
        }
        else
        {
            isAngerMonster = false;
            monsterCollider.enabled = false;
        }
        animator.SetBool("IsAngerMonster",isAngerMonster);
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();


        if(player != null)
        {
            Vector2 hitDirection = player.transform.position - transform.position;

            player.TakeDamage(2, hitDirection);
        } 

        if(collision.collider.tag == "Punch")
        {
            Vector2 hitDirection = collision.collider.transform.position - transform.position;
            TakeDamage(1, hitDirection);
        }
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        health = health - damage;

        moveDirection = moveDirection * (-1.0f);

        if(moveDirection>0)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            currentRotation.y = 180;
            transform.rotation = Quaternion.Euler(currentRotation);
        }
        if(moveDirection<0)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            currentRotation.y = 0;
            transform.rotation = Quaternion.Euler(currentRotation);
        }

        if(health<0)
        {
            health = 0;
        }
        if(health==0)
        {
            health = 5;

            isAngerMonster = false;

            monsterCollider.enabled = false;

            isRespawning = true;
        }
        else
        {

            Vector2 knockback = hitDirection.normalized * knockbackVelocity + Vector2.up * knockbackVelocity * 0.5f;

            rb.velocity = knockback;

            knockbackTimer = knockbackDuration;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{

    public int hitpoints;
    public int hitpointDefault;
    public float shotDelay;
    float nextShot;
    public float shotWarn;
    float nextWarn;
    float deathDelay;
    public float nextSpawn;
    Renderer thisRenderer;
    public GameObject weakPointObject;
    public GameObject warnIcon;
    Renderer warnIconRenderer;
    public GameObject playerObject;
    UIScope2D playerScript;
    bool isPaused;
    public bool isDead;

    public Animator animator;
    // Use this for initialization
    void Start()
    {
        isPaused = false;
        isDead = true;
        thisRenderer = gameObject.GetComponent<Renderer>();
        warnIconRenderer = warnIcon.GetComponent<Renderer>();
        playerScript = playerObject.GetComponent<UIScope2D>();
        deathDelay = 5f;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead && !isPaused)
        {
            gameObject.layer = 8;
            weakPointObject.layer = 9;
            if (Time.time > nextWarn)
            {
                if (!warnIconRenderer.enabled)
                {
                    warnIconRenderer.enabled = true;
                }
                warnIcon.transform.Rotate(Vector3.forward * (Time.deltaTime * 20));
            }

            if (Time.time > nextShot)
            {
                //shoot
                warnIconRenderer.enabled = false;
                nextShot = Time.time + shotDelay;
                nextWarn = nextShot - shotWarn;
                playerScript.TakeDamage();
            }
        }
        else
        {
            weakPointObject.layer = 10;
            gameObject.layer = 10;
        }

    }

    public void setTimeToNextShot()
    {
        nextShot = Time.time + shotDelay;
        nextWarn = nextShot - shotWarn;
    }

    public void weakPointHit()
    {
        if (thisRenderer.enabled)
        {
            hitpoints = hitpoints - 2;
            if (hitpoints <= 0)
            {
                animator.SetBool("isDying", true);
                //thisRenderer.enabled = false;
                isDead = true;
                warnIconRenderer.enabled = false;
                nextSpawn = Time.time + deathDelay;
            }
        }
    }

    public void normalHit()
    {
        if (thisRenderer.enabled)
        {
            hitpoints--;
            if (hitpoints <= 0)
            {
                animator.SetBool("isDying", true);
                //thisRenderer.enabled = false;
                isDead = true;
                warnIconRenderer.enabled = false;
                nextSpawn = Time.time + deathDelay;
            }
        }
    }

    public void togglePause()
    {
        isPaused = !isPaused;
    }
}

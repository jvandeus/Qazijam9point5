using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{

    public int hitpoints;
    public int hitpointDefault;
    public float shotDelay;
    public float shotWarn;
    float deathDelay;
    public float nextSpawn;
    Renderer thisRenderer;

    // Use this for initialization
    void Start()
    {
        thisRenderer = gameObject.GetComponent<Renderer>();
        thisRenderer.enabled = false;
        deathDelay = 5f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void weakPointHit()
    {
        if (thisRenderer.enabled)
        {
            hitpoints = hitpoints - 2;
            if (hitpoints <= 0)
            {
                thisRenderer.enabled = false;
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
                thisRenderer.enabled = false;
                nextSpawn = Time.time + deathDelay;
            }
        }
    }
}

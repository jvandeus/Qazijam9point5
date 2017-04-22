using UnityEngine;
using System.Collections.Generic;

// [RequireComponent(typeof(Camera))]
public class UIScope2D : MonoBehaviour
{
    public RectTransform scopePanel;
    public GameObject scopeOutline;
    public Camera scopeCamera;
    public Texture2D cursorTexture;
    public Color cursorColor;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    bool gamePaused;

    //Firing
    int shotPointer;
    AudioSource objectAudio;
    public AudioClip[] shotSounds;
    public float fireDelay = 0.5f;
    float nextFire = 0.0f;
    bool wasLastClipWarn = false;
    public GameObject BulletUI;
    List<GameObject> BulletUIChildren = new List<GameObject>();
    //Reloading
    bool isReloading = false;
    public float reloadDelay = 0.25f;
    float nextReload = 0.0f;
    public AudioClip reloadBullet;
    public AudioClip reloadDone;
    //Enemy Hit Detection
    public LayerMask enemyMask;
    public LayerMask weakPointMask;

    //Enemy Spawn Handling
    public GameObject enemySpawnParent;
    public float enemySpawnDelay;
    float nextSpawn = 0.0f;

    //Score
    public int score;
    public int multi;
    int topCombo;
    int thisCombo;
    public int normalHitValue;
    public int critHitValue;
    public UnityEngine.UI.Text multiTextDisplay;
    public UnityEngine.UI.Text scoreTextDisplay;
    public GameObject gameOverCanvas;

    //Health
    int hitsTaken = 0;
    public GameObject stageBG;
    Renderer BGRender;
    bool displayDamage = false;
    float timeLerpStarted = 0.0f;

    void Start()
    {
        gamePaused = false;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        shotPointer = 0;
        objectAudio = GetComponent<AudioSource>();
        BGRender = stageBG.GetComponent<Renderer>();
        score = 0;
        multi = 0;
        topCombo = 0;
        thisCombo = 0;
        multiTextDisplay.text = "";

        foreach (Transform thisChild in BulletUI.transform)
        {
            BulletUIChildren.Add(thisChild.gameObject);
        }

        gameOverCanvas.SetActive(false);
    }

    void Update()
    {
        if (!gamePaused)
        {
            Vector3 mousePos = Input.mousePosition;
            scopePanel.transform.position = mousePos;
            scopeOutline.transform.position = mousePos;
            mousePos.z = -1.0f;
            scopeCamera.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
            if (multi < 1)
            {
                multiTextDisplay.text = "";
            }
            else if (multi > 1 && multi < 5)
            {
                multiTextDisplay.text = multi.ToString() + "X";
            }
            else if (multi == 5)
            {
                multiTextDisplay.text = "MAX";
            }

            multiTextDisplay.color = new Color(multi * 0.2f, 0, 0);

            if (Time.time > nextSpawn)
            {
                nextSpawn = Time.time + enemySpawnDelay;
                SpawnNewEnemy();
            }

            if (displayDamage)
            {
                float timeSinceStarted = Time.time - timeLerpStarted;
                float percentageComplete = timeSinceStarted / 1f;
                if (percentageComplete >= 1.0f)
                {
                    displayDamage = false;
                    BGRender.material.color = Color.white;
                }
                else
                {
                    BGRender.material.color = Color.Lerp(Color.red, Color.white, percentageComplete); ;
                }
            }

            //Shooting
            if (Input.GetMouseButtonDown(0))
            {
                FireGun();
            }

            //Reloading
            if ((Input.GetKeyDown(KeyCode.R) && !isReloading) || (isReloading && Time.time > nextReload))
            {
                HandleReload();
            }
        }
    }

    /**
	 * Fire the gun into the scene, or click make a click if no bullets.
	 */
    public void FireGun()
    {
        // pseudo code:
        // Is there any bullets left?
        // if not, click, and do not fire. return.
        // otherwise play a fire sound effect
        // raycast from either mouse position or the center position of this object into the scene to see if anything is hit.
        // run the function in the hitable object to react to the hit.

        if (shotPointer < 6 && Time.time > nextFire)
        {
            isReloading = false;
            nextReload = 0.0f;
            nextFire = Time.time + fireDelay;
            AudioClip thisSound = shotSounds[Random.Range(0, shotSounds.Length)];
            objectAudio.PlayOneShot(thisSound, 0.5f);
            wasLastClipWarn = false;
            Renderer rend = BulletUIChildren[shotPointer].GetComponent<Renderer>();
            rend.enabled = false;
            shotPointer++;

            RaycastHit eHit;
            //Check for weak point hit, then check for normal hit
            if (Physics.Raycast(scopeCamera.transform.position, Vector3.forward, out eHit, 25f, weakPointMask, QueryTriggerInteraction.Ignore))
            {
                //weakpoint hitbox is a child of enemy, get parent first
                EnemyHandler eScript = eHit.transform.parent.transform.GetComponent<EnemyHandler>();
                eScript.weakPointHit();
                if (multi < 5)
                {
                    multi++;
                }
                thisCombo++;
                if (thisCombo > topCombo)
                {
                    topCombo = thisCombo;
                }
                score = score + (critHitValue * multi);
            }
            else if (Physics.Raycast(scopeCamera.transform.position, Vector3.forward, out eHit, 25f, enemyMask, QueryTriggerInteraction.Ignore))
            {
                EnemyHandler eScript = eHit.transform.GetComponent<EnemyHandler>();
                eScript.normalHit();
                multi = 0;
                score = score + normalHitValue;
            }
            else
            {
                multi = 0;
            }
        }
        else if (shotPointer == 6 && Time.time > nextFire)
        {
            if (!objectAudio.isPlaying || (objectAudio.isPlaying && !wasLastClipWarn))
            {
                objectAudio.PlayOneShot(objectAudio.clip, 0.5f);
                wasLastClipWarn = true;
            }
        }
    }

    public void HandleReload()
    {
        if (shotPointer != 0 && Time.time > nextFire + 0.25f)
        {
            /*foreach (GameObject thisBullet in BulletUIChildren)
            {
                Renderer rend = thisBullet.GetComponent<Renderer>();
                rend.enabled = true;
            }
            shotPointer = 0;*/

            shotPointer--;
            Renderer rend = BulletUIChildren[shotPointer].GetComponent<Renderer>();
            rend.enabled = true;
            if (shotPointer == 0)
            {
                isReloading = false;
                nextReload = 0.0f;
                objectAudio.PlayOneShot(reloadDone, 0.75f);
            }
            else
            {
                isReloading = true;
                nextReload = Time.time + reloadDelay;
                objectAudio.PlayOneShot(reloadBullet, 0.75f);
            }
        }
    }

    public void SpawnNewEnemy()
    {
        //Get all unenabled spawns from enemy parent, pick one at random to enable
        List<GameObject> validSpawns = new List<GameObject>();
        Renderer eRenderer;
        EnemyHandler eScript;
        foreach (Transform thisEnemy in enemySpawnParent.transform)
        {
            eRenderer = thisEnemy.gameObject.GetComponent<Renderer>();
            eScript = thisEnemy.gameObject.GetComponent<EnemyHandler>();
            if (eScript.isDead && Time.time > eScript.nextSpawn)
            {
                validSpawns.Add(thisEnemy.gameObject);
            }
        }

        if (validSpawns.Count > 0)
        {
            GameObject randomSpawn = validSpawns[Random.Range(0, validSpawns.Count)];
            eScript = randomSpawn.GetComponent<EnemyHandler>();
            eScript.isDead = false;
            eScript.animator.SetBool("isDying", false);
            eScript.animator.SetBool("isRising", true);
            eScript.hitpoints = eScript.hitpointDefault;
            eScript.setTimeToNextShot();
            Debug.Log(Time.time);
        }
        else
        {
            nextSpawn = Time.time + enemySpawnDelay;
        }

    }

    public void TakeDamage()
    {
        timeLerpStarted = Time.time;
        displayDamage = true;
        hitsTaken++;
        if (hitsTaken >= 3)
        {
            GameOver();
        }
        else
        {
            BGRender.material.color = Color.red;
        }

    }

    public void GameOver()
    {
        gamePaused = true;
        scoreTextDisplay.text = "Score : " + score.ToString() + "\n Best Combo : " + topCombo.ToString();
        EnemyHandler eScript;
        foreach (Transform thisEnemy in enemySpawnParent.transform)
        {
            eScript = thisEnemy.gameObject.GetComponent<EnemyHandler>();
            eScript.togglePause();
        }
        gameOverCanvas.SetActive(true);
    }
}
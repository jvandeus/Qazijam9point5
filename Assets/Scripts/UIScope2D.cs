using UnityEngine;
using System.Collections.Generic;

// [RequireComponent(typeof(Camera))]
public class UIScope2D : MonoBehaviour
{
    public RectTransform scopePanel;
    public GameObject scopeOutline;
    public Camera scopeCamera;
    public Texture2D cursorTexture;
    public Texture2D hitCursorTexture;
    public Texture2D critCursorTexture;
    float hitMarkerDelay = 0.25f;
    float removeHitMarker = 0.0f;
    bool checkCursor = false;
    public AudioClip hitSound;
    public AudioClip critSound;
    private AudioSource hitSource;
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
    public GameObject HealthUI;
    List<GameObject> HealthUIChildren = new List<GameObject>();
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
    public UnityEngine.UI.Text inGameScoreTextDisplay;
    public UnityEngine.UI.Text scoreTextDisplay;
    public GameObject gameOverCanvas;

    //Health
    int hitsTaken = 0;
    public GameObject stageBG;
    Renderer BGRender;
    bool displayDamage = false;
    float timeLerpStarted = 0.0f;
    public AudioClip[] eShotSounds;

    //Difficulty Ramping
    float nextLevel;
    float levelDelay;
    int difficultyLevel;

    //Powerups
    public int enemySpawnsToPowerUp;
    int spawnCounter;
    public GameObject powerupContainer;
    List<GameObject> powerupChildren = new List<GameObject>();
    public float chargeDuration;
    bool isCharged = false;
    float chargeExpire = 0.0f;

    //Music
    AudioSource BGAudio;
    public AudioClip[] GameOverTracks;
    float audioVolume = 0.0f;
    bool fadingOutMainTrack;
    bool playingGameOver;

    void Start()
    {
        gamePaused = false;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        shotPointer = 0;
        objectAudio = GetComponent<AudioSource>();
        BGRender = stageBG.GetComponent<Renderer>();
        BGAudio = stageBG.GetComponent<AudioSource>();
        audioVolume = BGAudio.volume;
        score = 0;
        multi = 0;
        topCombo = 0;
        thisCombo = 0;
        spawnCounter = 0;
        multiTextDisplay.text = "";
        fadingOutMainTrack = false;
        playingGameOver = false;

        foreach (Transform thisChild in BulletUI.transform)
        {
            BulletUIChildren.Add(thisChild.gameObject);
        }

        foreach (Transform thisChild in HealthUI.transform)
        {
            HealthUIChildren.Add(thisChild.gameObject);
        }

        foreach (Transform thisChild in powerupContainer.transform)
        {
            powerupChildren.Add(thisChild.gameObject);
        }

        hitSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        hitSource.playOnAwake = false;

        gameOverCanvas.SetActive(false);
        levelDelay = 5f;
        nextLevel = Time.time + levelDelay;
        difficultyLevel = 1;
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

            inGameScoreTextDisplay.text = score.ToString();

            if (Time.time > nextSpawn)
            {

                SpawnNewEnemy(false);
                spawnCounter++;
                if (difficultyLevel >= 20)
                {
                    SpawnNewEnemy(true);
                }
                else if (difficultyLevel >= 15)
                {
                    if (Random.Range(0, 100) > 24)
                    {
                        SpawnNewEnemy(true);
                    }
                }
                else if (difficultyLevel >= 10)
                {
                    if (Random.Range(0, 100) > 49)
                    {
                        SpawnNewEnemy(true);
                    }
                }
                else if (difficultyLevel >= 5)
                {
                    enemySpawnDelay = 3f;
                    if (Random.Range(0, 100) > 74)
                    {
                        SpawnNewEnemy(true);
                    }
                }
                nextSpawn = Time.time + enemySpawnDelay;
            }

            if (isCharged && Time.time > chargeExpire)
            {
                isCharged = false;
                reloadDelay = 0.25f;
                fireDelay = 0.5f;
                foreach (GameObject thisBullet in BulletUIChildren)
                {
                    Renderer bRend = thisBullet.GetComponent<Renderer>();
                    bRend.material.color = Color.white;
                }
            }

            if (spawnCounter >= enemySpawnsToPowerUp)
            {
                spawnCounter = 0;
                int thisRoll = Random.Range(0, 100);
                if (hitsTaken >= 2)
                {
                    spawnPowerUp("HealthPack");
                }
                else if (hitsTaken > 0 && thisRoll > 69)
                {
                    spawnPowerUp("HealthPack");
                }
                else if (hitsTaken > 0 && thisRoll <= 39)
                {
                    spawnPowerUp("Supercharger");
                }
                else if (hitsTaken > 0 && thisRoll <= 69)
                {
                    spawnPowerUp("ScreenClear");
                }
                else if (thisRoll >= 49)
                {
                    spawnPowerUp("Supercharger");
                }
                else
                {
                    spawnPowerUp("ScreenClear");
                }
            }

            if (Time.time > nextLevel)
            {
                difficultyLevel++;
                nextLevel = Time.time + levelDelay;
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

            if (checkCursor && Time.time > removeHitMarker)
            {
                checkCursor = false;
                Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
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
        else
        {
            if (playingGameOver)
            {             
                BGAudio.volume = 0.35f;
                AudioClip thisTrack = GameOverTracks[Random.Range(0, GameOverTracks.Length)];
                BGAudio.loop = false;
                BGAudio.clip = thisTrack;
                BGAudio.Play();
                playingGameOver = false;
            }
            else if (fadingOutMainTrack)
            {
                audioVolume -= 0.1f * Time.deltaTime;
                BGAudio.volume = audioVolume;
                if (audioVolume <= 0.01)
                {
                    playingGameOver = true;
                    fadingOutMainTrack = false;
                    BGAudio.Stop();
                }
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
            objectAudio.PlayOneShot(thisSound, 0.35f);
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
                Cursor.SetCursor(critCursorTexture, hotSpot, cursorMode);
                removeHitMarker = Time.time + hitMarkerDelay;
                checkCursor = true;
                hitSource.PlayOneShot(critSound, 1f);
                spawnCounter++;
            }
            else if (Physics.Raycast(scopeCamera.transform.position, Vector3.forward, out eHit, 25f, enemyMask, QueryTriggerInteraction.Ignore))
            {
                if (eHit.transform.GetComponent<EnemyHandler>() == null)
                {
                    if (eHit.transform.tag == "HealthPack")
                    {
                        foreach (GameObject thisPack in HealthUIChildren)
                        {
                            Renderer healthRend = thisPack.GetComponent<Renderer>();
                            healthRend.enabled = true;
                        }
                        hitsTaken = 0;
                        AudioSource packAudio = eHit.transform.GetComponent<AudioSource>();
                        packAudio.PlayOneShot(packAudio.clip);
                        Renderer packRend = eHit.transform.GetComponent<Renderer>();
                        packRend.enabled = false;
                        eHit.transform.gameObject.layer = 10;
                    }
                    else if (eHit.transform.tag == "Supercharger")
                    {
                        AudioSource packAudio = eHit.transform.GetComponent<AudioSource>();
                        packAudio.PlayOneShot(packAudio.clip);
                        Renderer packRend = eHit.transform.GetComponent<Renderer>();
                        packRend.enabled = false;
                        eHit.transform.gameObject.layer = 10;
                        chargeExpire = Time.time + chargeDuration;
                        isCharged = true;
                        reloadDelay = 0.12f;
                        fireDelay = 0.25f;
                        nextReload = 0.0f;
                        foreach (GameObject thisBullet in BulletUIChildren)
                        {
                            Renderer bRend = thisBullet.GetComponent<Renderer>();
                            bRend.material.color = Color.red;
                        }
                    }
                    else if (eHit.transform.tag == "ScreenClear")
                    {
                        AudioSource packAudio = eHit.transform.GetComponent<AudioSource>();
                        packAudio.PlayOneShot(packAudio.clip);
                        Renderer packRend = eHit.transform.GetComponent<Renderer>();
                        packRend.enabled = false;
                        eHit.transform.gameObject.layer = 10;
                        EnemyHandler eScript;
                        foreach (Transform thisEnemy in enemySpawnParent.transform)
                        {
                            eScript = thisEnemy.gameObject.GetComponent<EnemyHandler>();
                            if (!eScript.isDead)
                            {
                                eScript.weakPointHit();
                                score = score + critHitValue;
                            }
                        }
                    }
                }
                else
                {
                    EnemyHandler eScript = eHit.transform.GetComponent<EnemyHandler>();
                    eScript.normalHit();
                    multi = 0;
                    score = score + normalHitValue;
                    Cursor.SetCursor(hitCursorTexture, hotSpot, cursorMode);
                    removeHitMarker = Time.time + hitMarkerDelay;
                    checkCursor = true;
                    hitSource.PlayOneShot(hitSound, 2f);
                    thisCombo = 0;
                }
            }
            else
            {
                multi = 0;
                thisCombo = 0;
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

    public void SpawnNewEnemy(bool isSecondSpawn)
    {
        //Get all unenabled spawns from enemy parent, pick one at random to enable
        List<GameObject> validSpawns = new List<GameObject>();
        EnemyHandler eScript;
        foreach (Transform thisEnemy in enemySpawnParent.transform)
        {
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
            if (difficultyLevel >= 30)
            {
                eScript.shotDelay = 2.5f;
                eScript.shotWarn = 0.5f;
            }
            else if (difficultyLevel >= 25)
            {
                eScript.shotDelay = 3f;
                eScript.shotWarn = 0.5f;
            }
            else if (difficultyLevel >= 10)
            {
                eScript.shotDelay = 3.5f;
                eScript.shotWarn = 1f;
            }
            else if (difficultyLevel >= 2)
            {
                eScript.shotDelay = 4f;
                eScript.shotWarn = 1.5f;
            }
            if (isSecondSpawn)
            {
                eScript.shotDelay = eScript.shotDelay + 1f;
            }
            eScript.setTimeToNextShot();
        }
        else
        {
            nextSpawn = Time.time + enemySpawnDelay;
        }

    }

    public void spawnPowerUp(string powerUpTag)
    {
        List<GameObject> validPowerUps = new List<GameObject>();
        Renderer pRenderer;
        foreach (GameObject thisPowerUp in powerupChildren)
        {
            pRenderer = thisPowerUp.GetComponent<Renderer>();
            if (thisPowerUp.transform.tag == powerUpTag)
            {           
                if (pRenderer.enabled == false)
                {
                    validPowerUps.Add(thisPowerUp);
                }
            }
            if (pRenderer.enabled)
            {
                pRenderer.enabled = false;
            }
        }

        if (validPowerUps.Count > 0)
        {
            GameObject randomPower = validPowerUps[Random.Range(0, validPowerUps.Count)];
            pRenderer = randomPower.GetComponent<Renderer>();
            pRenderer.enabled = true;
            randomPower.layer = 8;
        }
    }

    public void TakeDamage()
    {
        timeLerpStarted = Time.time;
        displayDamage = true;
        Renderer rend = HealthUIChildren[hitsTaken].GetComponent<Renderer>();
        rend.enabled = false;
        hitsTaken++;
        AudioClip thisSound = eShotSounds[Random.Range(0, eShotSounds.Length)];
        objectAudio.PlayOneShot(thisSound, 0.75f);
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
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        gameOverCanvas.SetActive(true);
        fadingOutMainTrack = true;
    }
}
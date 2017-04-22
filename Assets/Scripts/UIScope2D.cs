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
    public int normalHitValue;
    public int critHitValue;
    public UnityEngine.UI.Text multiTextDisplay;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        shotPointer = 0;
        objectAudio = GetComponent<AudioSource>();
        score = 0;
        multi = 0;
        multiTextDisplay.text = "";

        foreach (Transform thisChild in BulletUI.transform)
        {
            BulletUIChildren.Add(thisChild.gameObject);
        }
    }

    void Update()
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
        if (shotPointer != 0 && Time.time > nextFire + 0.05f)
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
            if (!eRenderer.enabled && Time.time > eScript.nextSpawn)
            {
                validSpawns.Add(thisEnemy.gameObject);
            }
        }

        if (validSpawns.Count > 0)
        {
            GameObject randomSpawn = validSpawns[Random.Range(0, validSpawns.Count)];
            eRenderer = randomSpawn.GetComponent<Renderer>();
            eRenderer.enabled = true;
            eScript = randomSpawn.GetComponent<EnemyHandler>();
            eScript.hitpoints = eScript.hitpointDefault;
        }
        else
        {
            nextSpawn = Time.time + enemySpawnDelay;
        }

    }
}
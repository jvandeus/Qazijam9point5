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

    //Gunstuff
    int shotPointer;
    AudioSource objectAudio;
    public AudioClip[] shotSounds;
    bool wasLastClipWarn = false;
    public GameObject BulletUI;
    List<GameObject> BulletUIChildren = new List<GameObject>();

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        scopePanel.transform.position = mousePos;
        scopeOutline.transform.position = mousePos;
        mousePos.z = -1.0f;
        scopeCamera.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

        //Shooting
        if (Input.GetMouseButtonDown(0))
        {
            FireGun();
        }

        //Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadGun();
        }
    }

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        shotPointer = 0;
        objectAudio = GetComponent<AudioSource>();

        foreach (Transform thisChild in BulletUI.transform)
        {
            BulletUIChildren.Add(thisChild.gameObject);
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
        Debug.Log("in");

        if (shotPointer < 6)
        {
            AudioClip thisSound = shotSounds[Random.Range(0, shotSounds.Length)];
            objectAudio.PlayOneShot(thisSound, 0.5f);
            wasLastClipWarn = false;
            Renderer rend = BulletUIChildren[shotPointer].GetComponent<Renderer>();
            rend.enabled = false;
            shotPointer++;
        }
        else
        {
            if (!objectAudio.isPlaying || (objectAudio.isPlaying && !wasLastClipWarn))
            {
                objectAudio.PlayOneShot(objectAudio.clip, 0.5f);
                wasLastClipWarn = true;
            }     
        }
    }

    public void ReloadGun()
    {
        if (shotPointer != 0)
        {
            foreach (GameObject thisBullet in BulletUIChildren)
            {
                Renderer rend = thisBullet.GetComponent<Renderer>();
                rend.enabled = true;
            }
            shotPointer = 0;
        }
    }
}
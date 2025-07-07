using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SoundFX))]
public class Luxnaught : MonoBehaviour
{
    public static bool FirstEncounter = false;
    private Canvas worldCanvas;
    // first GameObject is the invisible object attached to the player, second GameObject is the image UI on the world canvas
    private Dictionary<KeyValuePair<GameObject, Vector3>, KeyValuePair<GameObject, PlayerController>> objsToImagesInScene = new();
    private AudioSource audioSource;
    public GameObject rectImage;
    public Image tempVignette;

    public GameObject obj;
    public AudioClip spawnSfx;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        worldCanvas = GetComponentInChildren<Canvas>();
        rectImage = worldCanvas.GetComponentInChildren<Image>().gameObject;
        foreach (PlayerController plr in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            GameObject newObj = Instantiate(obj, plr.transform);
            newObj.transform.localPosition = Random.insideUnitSphere * 2 + Vector3.up;
            Vector3 posOffset = newObj.transform.position - plr.transform.position;
            
            GameObject newImg = Instantiate(rectImage, worldCanvas.transform);
            newImg.GetComponent<Image>().enabled = true;
            
            objsToImagesInScene.Add(new KeyValuePair<GameObject, Vector3>(newObj, posOffset), new KeyValuePair<GameObject, PlayerController>(newImg, plr));
        }
        
        audioSource.PlayOneShot(spawnSfx);
        StartCoroutine(VignetteTransparencyCycle());
    }

    private void Update()
    {
        if (objsToImagesInScene.Count <= 0)
        {
            foreach (PlayerController plr in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                plr.levelStats.timeMultiplier = 1;
            }
            Destroy(gameObject);
        }
        
        foreach (var kvp in objsToImagesInScene)
        {
            PlayerController plr = kvp.Value.Value;
            kvp.Key.Key.transform.position = plr.transform.position + kvp.Key.Value;
            plr.levelStats.timeMultiplier = 1 + (0.15f * objsToImagesInScene.Count);

            float r = Vector3.Dot(kvp.Key.Value, plr.transform.forward);
            
            if (r < -0.65f)
            {
                kvp.Value.Key.GetComponent<Image>().enabled = false;
            }
            else
            {
                kvp.Value.Key.GetComponent<Image>().enabled = true;
                kvp.Value.Key.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(kvp.Key.Key.transform.position);
            }
        }
    }

    private IEnumerator VignetteTransparencyCycle()
    {
        bool firstCycle = false;
        float progress = 0;
        while (gameObject is not null)
        {
            progress += Time.deltaTime;
            if (progress > 1)
                progress = 1;

            if (firstCycle)
            {
                tempVignette.color = new Color(tempVignette.color.r, tempVignette.color.g, tempVignette.color.b, 1 - progress);
            }
            else
            {
                tempVignette.color = new Color(tempVignette.color.r, tempVignette.color.g, tempVignette.color.b, (1 - progress) * 0.4f);
            }

            if (progress >= 1)
            {
                firstCycle = true;
                progress = 0;
                audioSource.PlayOneShot(spawnSfx, audioSource.volume * 0.4f);
            }
            yield return null;
        }
    }

    public void DestroyImage(GameObject imageObj)
    {
        foreach (var kvp in objsToImagesInScene)
        {
            if (kvp.Value.Key == imageObj)
            {
                Destroy(kvp.Value.Key);
                Destroy(kvp.Key.Key);
                objsToImagesInScene.Remove(kvp.Key);
                break;
            }
        }
    }
}

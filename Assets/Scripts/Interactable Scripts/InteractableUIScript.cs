using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(PlayerController))]
public class InteractableUIScript : MonoBehaviour
{
    private PlayerController plrController;
    public GameObject interactPrefab;
    private GameObject interactUIInScene;
    public List<GameObject> closeInteractables;
    public GameObject closestInteractable;
    public Canvas worldUICanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrController = GetComponent<PlayerController>();
        interactUIInScene = Instantiate(interactPrefab);
        interactUIInScene.transform.SetParent(worldUICanvas.transform);
        interactUIInScene.SetActive(false);
        interactUIInScene.transform.GetChild(0).GetComponent<TMP_Text>().text = plrController.interact.ToString().ToUpper();
    }

    // Update is called once per frame
    void Update()
    {
        if (closeInteractables.Count == 0)
        {
            closestInteractable = null;
        } else if (closeInteractables.Count == 1)
        {
            closestInteractable = closeInteractables[0];
        } else if (closeInteractables.Count > 1)
        {
            float shortestDistance = (closeInteractables[0].transform.position - transform.position).magnitude;
            GameObject closest = closeInteractables[0];

            foreach (GameObject thing in closeInteractables)
            {
                float dist = (thing.transform.position - transform.position).magnitude;
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    closest = thing;
                }
            }

            closestInteractable = closest;
        }

        if (closestInteractable != null && Mathf.Abs(closestInteractable.GetComponent<Interactable>().screenPointPosition.x) < plrController.camera.pixelWidth
            && Mathf.Abs(closestInteractable.GetComponent<Interactable>().screenPointPosition.y) < plrController.camera.pixelHeight
            && !plrController.paused && !plrController.inDialogue)
        {
            interactUIInScene.SetActive(true);
            Interactable inter = closestInteractable.GetComponent<Interactable>();
            Vector3 screenPoint = inter.screenPointPosition;

            interactUIInScene.GetComponent<RectTransform>().position = new Vector3(screenPoint.x, screenPoint.y, 0);
            if (Input.GetKeyDown(plrController.interact) && !plrController.inDialogue && !plrController.paused && !plrController.usingZipline)
            {
                inter.Interact(plrController);
            }
        } else
        {
            interactUIInScene.SetActive(false);
        }
    }

    public void CreateUIForInteractable(GameObject obj)
    {
        closeInteractables.Add(obj);
    }

    public void InteractableOutOfRange(GameObject obj)
    {
        closeInteractables.Remove(obj);
    }

    public void ChangeDisplayingBind(KeyCode newBind)
    {
        interactUIInScene.transform.GetChild(0).GetComponent<TMP_Text>().text = newBind.ToString();
    }
}

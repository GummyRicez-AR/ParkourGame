using UnityEngine;

public class Interactable : MonoBehaviour
{
    protected GameObject player;
    protected PlayerController plrController;
    protected InteractableUIScript interactableUIScript;
    public Vector3 screenPointPosition;
    public float interactDistance = 5;
    protected bool canBeInteractedWith;
    protected bool visibleUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        plrController = FindFirstObjectByType<PlayerController>();
        player = plrController.gameObject;
        interactableUIScript = player.GetComponent<InteractableUIScript>();
        canBeInteractedWith = true;
        visibleUI = false;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        screenPointPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPointPosition.z < interactDistance && screenPointPosition.z > 0 && canBeInteractedWith)
        {
            if (!visibleUI)
            {
                visibleUI = true;
                interactableUIScript.CreateUIForInteractable(gameObject);
            }
        } else
        {
            if (visibleUI)
            {
                visibleUI = false;
                interactableUIScript.InteractableOutOfRange(gameObject);
            }
        }
    }

    public virtual void Interact(PlayerController player)
    {
        Debug.Log(gameObject.name + " interacted with by " + player.gameObject.name);
    }

    public virtual void OnDestroy()
    {
        canBeInteractedWith = false;
        if (interactableUIScript != null)
            interactableUIScript.InteractableOutOfRange(gameObject);
    }
}

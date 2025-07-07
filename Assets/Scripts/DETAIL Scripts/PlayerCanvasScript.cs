using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCanvasScript : MonoBehaviour
{
    private PlayerController plrController;
    public float hudSensivitity = 1;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float yOffset;
    private bool movingCanvas;
    private Queue<float> lastXDeltas;
    private Queue<float> lastYDeltas;

    private readonly int numDeltaEntries = 3;
    private float leftXBound;
    private float rightXBound;
    private float bottomYBound;
    private float topYBound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrController = GetComponentInParent<PlayerController>();
        lastXDeltas = new();
        lastYDeltas = new();

        for (int i = 0; i < numDeltaEntries; i++)
        {
            lastXDeltas.Enqueue(0); lastYDeltas.Enqueue(0);
        }

        movingCanvas = false;
        yOffset = 0;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        leftXBound = initialLocalPosition.x - 0.25f;
        rightXBound = initialLocalPosition.x + 0.25f;
        bottomYBound = initialLocalPosition.y - 0.25f;
        topYBound = initialLocalPosition.y + 0.25f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!plrController.inDialogue && !plrController.paused) {

            lastXDeltas.Dequeue(); lastXDeltas.Enqueue(Input.GetAxis("Mouse X"));
            lastYDeltas.Dequeue(); lastYDeltas.Enqueue(Input.GetAxis("Mouse Y"));
            yOffset = Mathf.MoveTowards(yOffset, -plrController.GetMovementVector().y / 45, Time.deltaTime / 2);
            yOffset = Mathf.Clamp(yOffset, -0.1f, 0.1f);

            float avgX = 0;
            float avgY = 0;

            foreach (float val in lastXDeltas)
                avgX += val;
            avgX /= numDeltaEntries;
            foreach (float val in lastYDeltas)
                avgY += val;
            avgY /= numDeltaEntries;

            if (avgX == 0 && avgY == 0 && plrController.GetMovementVector().y == 0)
            {
                if (!movingCanvas)
                    StartCoroutine(MoveCanvasToOriginalPos(0.5f));
            } else
            {
                StopAllCoroutines();
                movingCanvas = false;
                transform.localPosition = initialLocalPosition + (new Vector3(-avgX / 200f, (-avgY / 100f), 0) * hudSensivitity);
                transform.localRotation = initialLocalRotation * Quaternion.Euler(0, -avgX / 1.25f, 0);
            }
            transform.localPosition += new Vector3(0, yOffset * hudSensivitity * 0.3f, 0);
        } else
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }
        

        if (transform.localPosition.x < leftXBound)
            transform.localPosition = new Vector3(leftXBound, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.x > rightXBound)
            transform.localPosition = new Vector3(rightXBound, transform.localPosition.y, transform.localPosition.z);
        
        if (transform.localPosition.y < bottomYBound)
            transform.localPosition = new Vector3(transform.localPosition.x, bottomYBound, transform.localPosition.z);
        if (transform.localPosition.y > topYBound)
            transform.localPosition = new Vector3(transform.localPosition.x, topYBound, transform.localPosition.z);
    }

    private IEnumerator MoveCanvasToOriginalPos(float time)
    {
        movingCanvas = true;
        float progress = 0;
        Vector3 targetPos = initialLocalPosition;
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, -Mathf.Pow(progress, 2) + (2 * progress));
            yield return null;
        }
        movingCanvas = false;
    }
}

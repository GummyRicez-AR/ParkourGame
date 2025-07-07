using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public PlayerController plrController;
    public Transform playerTransform;
    public float maxXRotation = 85;
    public float sensitivity = 1;
    private float xRotation;
    private new Camera camera;
    private float initialFOV;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = GetComponent<Camera>();
        initialFOV = camera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        xRotation = playerTransform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!plrController.paused && !plrController.inDialogue)
        {
            xRotation -= Input.GetAxis("Mouse Y") * sensitivity;
            xRotation = Mathf.Clamp(xRotation, -maxXRotation, maxXRotation);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

            if (plrController.sliding)
            {
                plrController.head.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * sensitivity, 0));
            }
            else
            {
                playerTransform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * sensitivity, 0));
            }
        }

        Vector3 movementVector = plrController.GetMovementVector();
        Vector3 planeVector = new Vector3(movementVector.x, 0, movementVector.z);
        camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, initialFOV + (planeVector.magnitude / 8f), Time.deltaTime * 5f);
    }
}

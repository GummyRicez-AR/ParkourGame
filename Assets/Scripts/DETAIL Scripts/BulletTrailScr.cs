using UnityEngine;

public class BulletTrailScr : MonoBehaviour
{
    public float travelSpeed = 100;
    public Vector3 target;
    private float timer;
    private new Light light;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, travelSpeed * Time.deltaTime);

            if (transform.position == target)
            {
                light.enabled = false;
                Destroy(gameObject, 0.2f);
            }
                
        }
    }
}

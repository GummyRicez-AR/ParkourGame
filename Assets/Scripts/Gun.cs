using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Gun : MonoBehaviour
{
    public UnityEvent OnShot;
    public GunItemData gunItemSO;

    public LayerMask layerMask;
    public GameObject barrel;
    public GameObject particleObject;

    private AudioSource audioSource;
    private Animator animator;
    private PlayerController plrController;
    private int reserveAmmo;
    private int ammoInClip;
    private bool reloading;
    private float fireCooldown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        plrController = GetComponentInParent<PlayerController>();
        reserveAmmo = gunItemSO.maxAmmo;
        ammoInClip = gunItemSO.clipSize;
        fireCooldown = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0)
                fireCooldown = 0;
        }

        if (((Input.GetKeyDown(plrController.useItem) && !gunItemSO.autoFire) || (Input.GetKey(plrController.useItem) && gunItemSO.autoFire)) && !plrController.paused && !plrController.inDialogue)
        {
            if (fireCooldown == 0)
            {
                if (ammoInClip > 0)
                    Fire();
                else if (!reloading)
                    StartCoroutine(Reload());
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!reloading)
            {
                StartCoroutine(Reload());
            }
        }

        if (Input.GetKeyDown(plrController.itemSpecial) && !plrController.paused && !plrController.inDialogue)
        {
            switch (gunItemSO.gunAbility)
            {
                case GunAbility.ChipToss:
                    ChipToss();
                    break;
            }
        }
    }

    protected virtual IEnumerator Reload()
    {
        reloading = true;
        plrController.inventory.GetComponent<InventoryScript>().reloading.GetComponent<TMPro.TMP_Text>().enabled = true;
        plrController.inventory.GetComponent<InventoryScript>().worldReloading.GetComponent<TMPro.TMP_Text>().enabled = true;

        float time = 0;
        while (time < gunItemSO.reloadTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        reloading = false;
        plrController.inventory.GetComponent<InventoryScript>().reloading.GetComponent<TMPro.TMP_Text>().enabled = false;
        plrController.inventory.GetComponent<InventoryScript>().worldReloading.GetComponent<TMPro.TMP_Text>().enabled = false;

        int ammoDiff = gunItemSO.clipSize - ammoInClip;
        ammoInClip = gunItemSO.clipSize;
        reserveAmmo -= ammoDiff;
    }

    protected virtual void Fire()
    {
        ammoInClip--;
        reloading = false;
        StopAllCoroutines();

        audioSource.PlayOneShot(gunItemSO.fireSFX);
        animator.SetTrigger("Fire");
        plrController.inventory.GetComponent<InventoryScript>().reloading.GetComponent<TMPro.TMP_Text>().enabled = false;
        plrController.inventory.GetComponent<InventoryScript>().worldReloading.GetComponent<TMPro.TMP_Text>().enabled = false;

        ProcessUIHit();
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(r, out RaycastHit castHit, Mathf.Infinity, layerMask);

        GameObject newTrail = Instantiate(gunItemSO.lineObject);
        newTrail.transform.position = barrel.transform.position;
        if (hit)
        {
            newTrail.GetComponent<BulletTrailScr>().target = castHit.point;
            ProcessHit(castHit, r);
        } else
        {
            newTrail.GetComponent<BulletTrailScr>().target = barrel.transform.position + (r.direction * 500);
        }

        fireCooldown = gunItemSO.fireRate;
    }

    private void ProcessUIHit()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("WorldUIInteract"))
            {
                if (result.gameObject.CompareTag("Luxnaught"))
                {
                    result.gameObject.GetComponentInParent<Luxnaught>().DestroyImage(result.gameObject);
                }
            }
        }
    }

    private void ProcessHit(RaycastHit castHit, Ray ray)
    {
        GameObject newParticles = Instantiate(particleObject, castHit.point, particleObject.transform.rotation);
        newParticles.transform.rotation = Quaternion.LookRotation(castHit.normal) * Quaternion.Euler(0, 0, 180);
        ParticleSystem bulletClankSystem = newParticles.GetComponentInChildren<ParticleSystem>();
        
        bulletClankSystem.emission.SetBurst(0, new ParticleSystem.Burst { time = 0, count = Random.Range(2, 5), cycleCount = 1, repeatInterval = 0.01f, probability = 1 });
        bulletClankSystem.Play();
        Destroy(newParticles, 1);
        switch (castHit.collider.tag)
        {
            case "Target":
                castHit.collider.gameObject.GetComponent<TargetScr>().GunShot(gunItemSO.damage, plrController);
                OnShot?.Invoke();
                break;
            case "Head":
                castHit.collider.gameObject.GetComponentInParent<BaseEnemyScript>().GunShot(gunItemSO.damage * gunItemSO.headshotMult, plrController);
                OnShot?.Invoke();
                break;
            case "Limb":
                castHit.collider.gameObject.GetComponentInParent<BaseEnemyScript>().GunShot(gunItemSO.damage, plrController);
                OnShot?.Invoke();
                break;
            case "Chip":
                castHit.collider.gameObject.GetComponent<ChipScr>().ChipShot(this, plrController);
                OnShot?.Invoke();
                break;
        }
    }

    protected virtual void ChipToss()
    {
        if (plrController.inventory.chips <= 0) { return; }
        plrController.inventory.chips--;
        Instantiate(gunItemSO.chipTossObj, plrController.transform.position + (plrController.transform.rotation * Vector3.forward * 2), plrController.transform.rotation)
            .GetComponent<ChipScr>().playerThatThrewChip = plrController;
    }

    public int GetReserveAmmo() { return reserveAmmo; }
    public int GetAmmoInClip() { return ammoInClip; }
}

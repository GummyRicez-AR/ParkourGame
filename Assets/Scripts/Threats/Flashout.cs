using UnityEngine;
using System.Collections;
using TMPro;

public class Flashout : MonoBehaviour
{
    public static bool FirstEncounter = false;
    private SoundFX soundFXScr;
    private UnityEngine.UI.Image vignette;
    private PlayerController[] players;
    private bool attacked;
    private float timer;
    private TMP_Text timerUI;
    private float origFontSize;
    
    public AudioSource humSource;
    public float attackTime = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        soundFXScr = GetComponent<SoundFX>();
        timerUI = GetComponentInChildren<TMP_Text>();
        origFontSize = timerUI.fontSize;
        vignette = GetComponentInChildren<UnityEngine.UI.Image>();
        vignette.color = new Color(vignette.color.r, vignette.color.g, vignette.color.b, 0);

        attacked = false;
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController plr in players)
        {
            StartCoroutine(ShakeCamera(plr.camera));
        }

        BarLightScr[] lights = FindObjectsByType<BarLightScr>(FindObjectsSortMode.None);
        foreach (BarLightScr light in lights)
        {
            for (var i = 0; i <= 2; i++)
            {
                light.Invoke(nameof(light.BlinkOff), 0.55f * i);
                light.Invoke(nameof(light.BlinkOn), (0.55f * i) + 0.1f);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!attacked)
        {
            bool allPlayersSafe = true;
            foreach (PlayerController plr in players)
            {
                if (!plr.safe)
                {
                    allPlayersSafe = false;
                    break;
                }
            }

            if (allPlayersSafe)
            {
                timer += Time.deltaTime * 3;
            }
            else
            {
                timer += Time.deltaTime;
            }

            vignette.color = new Color(vignette.color.r, vignette.color.g, vignette.color.b, timer / attackTime);
            humSource.volume = (timer / attackTime) * soundFXScr.volumeMult;
            timerUI.text = (attackTime - timer).ToString("F2");
            
            if (timer < attackTime)
                timerUI.text = timerUI.text.Insert(timerUI.text.Length, "</size>").Insert(Mathf.Max(1, (int)Mathf.Log10((attackTime - timer) * 10)), "<size=30%>");
            
            timerUI.fontSize = origFontSize + (150 * (timer / attackTime));
            timerUI.color = new Color(1, 1 - (timer / attackTime), 1 - (timer / attackTime), 8/255f);
            
            timerUI.ForceMeshUpdate();
            Mesh tMesh = timerUI.mesh;
            Vector3[] vertices  = tMesh.vertices;

            for (int i = 0; i < timerUI.textInfo.characterInfo.Length; i++)
            {
                TMP_CharacterInfo c = timerUI.textInfo.characterInfo[i];

                int index = c.vertexIndex;

                Vector3 offset = Random.onUnitSphere * (10 * (timer / attackTime));
                vertices[index] += offset + (Random.onUnitSphere * 4);
                vertices[index + 1] += offset + (Random.onUnitSphere * 4);
                vertices[index + 2] += offset + (Random.onUnitSphere * 4);
                vertices[index + 3] += offset + (Random.onUnitSphere * 4);
            }

            tMesh.vertices = vertices;
            timerUI.canvasRenderer.SetMesh(tMesh);

            if (timer >= attackTime)
            {
                timerUI.text = "";
                foreach (PlayerController plr in players)
                {
                    if (!plr.safe)
                    {
                        plr.levelStats.ChangeLevelTime(8);
                    }
                    attacked = true;
                    humSource.Stop();
                    vignette.color = new Color(vignette.color.r, vignette.color.g, vignette.color.b, 0);
                    Destroy(gameObject, 2);
                }
            }
        }
    }

    private IEnumerator ShakeCamera(Camera camera)
    {
        Vector3 initPosition = camera.transform.localPosition;
        while (!attacked)
        {
            Vector3 randOffset = (timer / attackTime) * 0.05f * Random.insideUnitCircle;
            camera.transform.localPosition = initPosition + randOffset;
            yield return null;
        }

        Vector3 posAfterShake = camera.transform.localPosition;
        float tweenProgress = 0;
        while (tweenProgress < 1)
        {
            tweenProgress += Time.deltaTime / 0.25f;
            camera.transform.localPosition = Vector3.Lerp(posAfterShake, initPosition, tweenProgress);
            yield return null;
        }
    }
}

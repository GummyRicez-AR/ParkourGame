using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoSheetScr : MonoBehaviour
{
    public TMP_Text text;
    public Image image;

    private PlayerController plr;
    private Mesh tMesh;
    private Vector3[] vertices;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plr = GetComponentInParent<PlayerController>();
        text = GetComponentInChildren<TMP_Text>();
        image = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(plr.useItem))
        {
            plr.inventory.RemoveItem(gameObject);
        }
        
        text.ForceMeshUpdate();
        tMesh = text.mesh;
        vertices = tMesh.vertices;

        for (int i = 0; i < text.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo c = text.textInfo.characterInfo[i];
            
            int index = c.vertexIndex;

            Vector3 offset = Random.insideUnitCircle * 0.2f;
            if (i == 0)
            {
                Vector3 newOffset = offset * 0.2f;
                
                vertices[index] += newOffset;
                vertices[index + 1] += newOffset;
                vertices[index + 2] += newOffset;
                vertices[index + 3] += newOffset; 
            }
            else
            {
                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset; 
            }
        }
        
        tMesh.vertices = vertices;
        text.canvasRenderer.SetMesh(tMesh);
    }
}

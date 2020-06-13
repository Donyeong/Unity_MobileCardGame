using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPostProcess : MonoBehaviour
{

    [SerializeField] Material mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}

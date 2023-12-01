using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ScreenSizeAdjuster : MonoBehaviour
{
    private Vector2 resolution;
    private RenderTexture renderTexture;
    public PanelSettings myPanelSettings;

    public Material Material;
    // Start is called before the first frame update
    void Awake()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        if (myPanelSettings == null) {Debug.Log("No panelsettings assigned"); return;}

        if(myPanelSettings.targetTexture != null)
        {
            Destroy(myPanelSettings.targetTexture );
        }
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        myPanelSettings.targetTexture = renderTexture;
        Material.mainTexture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    {

        if (resolution.x != Screen.width || resolution.y != Screen.height)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            myPanelSettings.targetTexture = renderTexture;
            Material.mainTexture = renderTexture;
            resolution = new Vector2(Screen.width, Screen.height);
        }
    }
}

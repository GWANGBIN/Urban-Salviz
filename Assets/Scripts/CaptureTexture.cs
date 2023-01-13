/*
  This sample code is for demonstrating and testing the functionality
  of Unity Capture, and is placed in the public domain.

  This code generates a scrolling color texture simply for the purposes of demonstration.
  Other uses may include sending a video, another webcam feed or a static image to the output.
*/

using UnityEngine;
using UnityEngine.UI;

public class CaptureTexture : MonoBehaviour
{
    public int width = 320;
    public int height = 240;
    public int Target_FrameRate = 30;
    public Camera maincam;
    public RectTransform cam_texture;    
    //public MeshRenderer outputRenderer;
    Texture2D activeTex;
    private RenderTexture a;
    //public RenderTexture b;
    UnityCapture.Interface captureInterface;
    int y = 0;
    Color color = Color.red;

    private void Awake()
    {
        Application.targetFrameRate = Target_FrameRate;
    }

    void Start()
    {
        //width = (int)cam_texture.rect.width;
        //height = (int)cam_texture.rect.height;
        // Create texture and capture interface
        activeTex = new Texture2D(width, height, TextureFormat.ARGB32, false);        
        captureInterface = new UnityCapture.Interface(UnityCapture.ECaptureDevice.CaptureDevice1);

        //if (outputRenderer != null) outputRenderer.material.mainTexture = activeTex;
    }

    void OnDestroy()
    {
        //Cleanup capture interface
        captureInterface.Close();
    }

    void Update()
    {
        /*for (int x = 0; x < width; x++)
        {
            activeTex.SetPixel(x, y, color);
        }

        y += 1;
        if (y > height)
        {
            y = 0;
            color = new Color(color.g, color.b, color.r);
        }
        
        activeTex.Apply();*/

        // Update the capture texture
        //a.width = (int)cam_texture.rect.width;
        //a.height = (int)cam_texture.rect.height;
        a = new RenderTexture((int)cam_texture.rect.width, (int)cam_texture.rect.height, 30);
        maincam.targetTexture = a;
        maincam.Render();
        UnityCapture.ECaptureSendResult result = captureInterface.SendTexture(a);
        if (result != UnityCapture.ECaptureSendResult.SUCCESS)
            Debug.Log("SendTexture failed: " + result);
    }    
}

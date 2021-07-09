using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamDetecter : MonoBehaviour
{
    public Texture2D processedTexYOLO = null;
    public Texture2D processedTexP2P = null;
    public RunYOLOModel runYoloModel;
    public RunPix2PixModel runPix2PixModel;
    public Text text;
    public UpdateIndicators updateIndicators;
    

    private WebCamTexture webcamTex;
    private TextureScaler texScalerYOLO;
    private TextureScaler texScalerP2P;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float time = 0f;
    const float posDistance = 0.4f;
    const float rotDifference = 0.9f;

    // Start is called before the first frame update
    void Start()
    {
        webcamTex = new WebCamTexture(WebCamTexture.devices[0].name);
        webcamTex.Play();
        texScalerYOLO = new TextureScaler(416, 416);
        texScalerP2P = new TextureScaler(512, 512);
        lastCameraPosition = Camera.main.transform.position;
        lastCameraRotation = Camera.main.transform.rotation;
        runYoloModel.UpdateYOLOModel();
        runPix2PixModel.UpdatePix2PixModel();
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 10f && CheckPosChange(posDistance, rotDifference))
        {
            processedTexYOLO = TextureCropTools.CropToSquare(webcamTex);
            texScalerYOLO.Scale(processedTexYOLO);
            processedTexP2P = TextureCropTools.CropWithRect(webcamTex, new Rect(Vector2.zero, new Vector2(webcamTex.width, webcamTex.height)));
            texScalerP2P.Scale(processedTexP2P);
            runYoloModel.UpdateYOLOModel();
            if(runYoloModel.results.Count > 0)
            {
                runPix2PixModel.UpdatePix2PixModel();
                updateIndicators.UpdateCall(runYoloModel.results, runPix2PixModel.hint_position);
            }
            time = 0;
        }
        time += Time.deltaTime;
    }

    bool CheckPosChange(float posDistance, float rotDifference)
    {
        text.text = (runYoloModel.results.Count > 0 ? runYoloModel.results[0].rect.ToString() : "0");
        if (Vector3.Distance(Camera.main.transform.position, lastCameraPosition) >= posDistance)
        {
            lastCameraPosition = Camera.main.transform.position;
            return true;
        }
        else if(Quaternion.Dot(Camera.main.transform.rotation, lastCameraRotation) <= rotDifference){
            lastCameraRotation = Camera.main.transform.rotation;
            return true;
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class RunPix2PixModel : MonoBehaviour
{
    public NNModel modelAsset;
    public WebCamDetecter webCamDetecter;
    public bool HAVE_GPU = true;
    public RenderTexture outputRendTex;
    public int[,] available_slots;
    public int[] hint_position;
    public float available_thres = 0.8f;

    private Model m_RuntimeModel;
    private IWorker m_Worker;
    private IOps m_Ops;
    private Tensor premulTensor;
    private TextureScaler scaler;

    // Start is called before the first frame update
    void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        if (HAVE_GPU)
        {
            m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
            m_Ops = new PrecompiledComputeOps(verbose: false);
        }
        else
        {
            m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharp, m_RuntimeModel);
            m_Ops = new ReferenceCPUOps();
        }
        premulTensor = new Tensor(1, 1, new float[] { 255 });
        available_slots = new int[8,8];
        scaler = new TextureScaler(128, 128);
    }

    public void UpdatePix2PixModel()
    {
        if (webCamDetecter.processedTexP2P)
        {
            UpdateInfluencedMap();
            Texture2D temp = TextureCropTools.toTexture2D(outputRendTex);
            scaler.Scale(temp);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color[] temp_color = temp.GetPixels(16 * i, 16 * j, 16, 16);
                    int sum = 0;
                    for (int k = 0; k < temp_color.Length; k++)
                    {
                        sum += (temp_color[k].b > available_thres) ? 1 : 0;
                    }
                    available_slots[i, j] = sum > 256 * available_thres ? 1 : 0;
                }
            }
            hint_position = new int[] { 0, 0 };
            int maximum = 98;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (available_slots[i, j] == 1)
                    {
                        if (i * i + j * j <= maximum)
                        {
                            hint_position = new int[] { i, j };
                            maximum = i * i + j * j;
                        }
                    }
                }
            }
        }
    }

    private void UpdateInfluencedMap()
    {
        Tensor input = new Tensor(webCamDetecter.processedTexP2P);

        // Preprocess the texture by multiplying every pix by 255
        Tensor preprocessed = m_Ops.Mul(new Tensor[] { input, premulTensor });
        input.Dispose();

        m_Worker.Execute(preprocessed);
        preprocessed.Dispose();
        Tensor output = m_Worker.PeekOutput();
        output.ToRenderTexture(outputRendTex);
        output.Dispose();

    }
}

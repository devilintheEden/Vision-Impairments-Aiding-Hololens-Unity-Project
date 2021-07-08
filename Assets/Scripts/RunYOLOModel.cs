using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;

public class RunYOLOModel : MonoBehaviour
{
    public struct ResultBox
    {
        public Rect rect;
        public float[] classes;
        public int bestClassIdx;
    }

    public NNModel modelAsset;
    public WebCamDetecter webCamDetecter;
    public List<ResultBox> results = new List<ResultBox>();
    public bool HAVE_GPU = true;

    private Model m_RuntimeModel;
    private IWorker m_Worker;
    private IOps m_Ops;
    private Tensor premulTensor;

    private const float DISCARD_THRESHOLD = 0.1f; // how confident is the box?
    private const float OVERLAP_THRESHOLD = 0.2f; // Does this box overlaps with other box with same label and higher possibility?
    private const float PROBABILITY_THRESHOLD = 0.3f; // Does this label has a high enough possibility?

    private float[] anchors = new float[] { 1.08f, 1.19f, 3.42f, 4.41f, 6.63f, 11.38f, 9.42f, 5.11f, 16.62f, 10.52f };

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
    }

    // Update is called once per frame
    public void UpdateYOLOModel()
    {
        if (webCamDetecter.processedTexYOLO)
        {
            Tensor input = new Tensor(webCamDetecter.processedTexYOLO);

            // Preprocess the texture by multiplying every pix by 255
            Tensor preprocessed = m_Ops.Mul(new Tensor[] { input, premulTensor });
            input.Dispose();

            //Run the model
            m_Worker.Execute(preprocessed);
            preprocessed.Dispose();
            Tensor output = m_Worker.PeekOutput();
            float[] data = output.AsFloats();
            output.Dispose();

            //Postprocess the output data
            results = PostProcessed(data, DISCARD_THRESHOLD, OVERLAP_THRESHOLD, PROBABILITY_THRESHOLD);
        }
    }

    List<ResultBox> PostProcessed(float[] data, float d_thres, float o_thres, float p_thres)
    {
        /* 
         * the data is arranged in a 13 * 13 * 5 * 25 manner
         * this is how tinyYoloV2 works, 
         * details see https://machinethink.net/blog/object-detection-with-yolo/ 
         */

        List<ResultBox> temp = new List<ResultBox>();
        for (int y_cell = 0; y_cell < 13; y_cell++)
        {
            for (int x_cell = 0; x_cell < 13; x_cell++)
            {
                for (int box = 0; box < 5; box++)
                {
                    int idx = (x_cell + y_cell * 13) * 125 + box * 25;
                    // check the box overall confidence
                    if (Utils.Sigmoid(data[idx + 4]) > d_thres)
                    {
                        //decode the rect and the classes with sigmoid and softmax
                        Rect boundingBox = DecodeBoxRectangle(data, idx, x_cell, y_cell, box);
                        float[] classes = DecodeBoxClasses(data, idx, Utils.Sigmoid(data[idx + 4]));
                        temp.Add(new ResultBox { rect = boundingBox, classes = classes, bestClassIdx = Utils.MaxIdx(classes) });
                    }
                }
            }
        }
        RemoveDuplicates(temp, o_thres); //remove the duplicates
        for (int i = temp.Count - 1; i >= 0; i --)
        {
            //check if the label has a high enough possibility
            if (temp[i].bestClassIdx >= 20 || temp[i].classes[temp[i].bestClassIdx] <= p_thres)
            {
                temp.RemoveAt(i);
            }
        }
        return temp;
    }

    private Rect DecodeBoxRectangle(float[] data, int startIndex, int x_cell, int y_cell, int box)
    {
        float box_x = (x_cell + Utils.Sigmoid(data[startIndex])) / 13;
        float box_y = (y_cell + Utils.Sigmoid(data[startIndex + 1])) / 13;
        float box_width = Mathf.Exp(data[startIndex + 2]) * anchors[2 * box] / 13;
        float box_height = Mathf.Exp(data[startIndex + 3]) * anchors[2 * box + 1] / 13;

        return new Rect(box_x - box_width / 2,
            box_y - box_height / 2, box_width, box_height);
    }

    private float[] DecodeBoxClasses(float[] data, int startIndex, float box_score)
    {
        float[] box_classes = data.GetRange(startIndex + 5, startIndex + 25);
        box_classes = Softmax(box_classes);
        box_classes.Update(x => x * box_score);
        return box_classes;
    }

    private void RemoveDuplicates(List<ResultBox> boxes, float o_thres)
    {
        // check if the box overlap with another box with same label and higher probability in that label
        if (boxes.Count == 0) { return; }
        for (int i = 0; i < boxes.Count; i++)
        {
            int c = boxes[i].bestClassIdx;
            if (c < 20)
            {
                float[] classValues = new float[boxes.Count];
                classValues.Update((x, i) => boxes[i].classes[c]);
                int[] sortedIndexes = Utils.SortIdx(classValues);
                int p = Array.IndexOf(sortedIndexes, i);
                for (int j = 0; j < p; j++)
                {
                    if (Utils.BoxesIOU(boxes[i].rect, boxes[sortedIndexes[j]].rect) >= o_thres)
                    {
                        boxes[i].classes[c] = 0;
                    }
                }
            }
        }
        
    }

    private float[] Softmax(float[] values)
    {
        Tensor t = new Tensor(1, values.Length, values);
        var ret = m_Ops.Softmax(t).AsFloats();
        t.Dispose();
        return ret;
    }

    void OnApplicationPause()
    {
        premulTensor.Dispose();
    }

    void OnApplicationQuit()
    {
        premulTensor.Dispose();
    }

}

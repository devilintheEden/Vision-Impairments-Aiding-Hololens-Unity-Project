using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateIndicators : MonoBehaviour
{
    public DrawOutline drawOutline;
    public DrawOnScreenHint drawOnScreenHint;
    public TextAsset classesFile;
    public RelativeToCam relative2Cam;
    public AudioSource adSource;
    public AudioClip thereis;
    public AudioClip[] typeName;
    public AudioClip[] oClock;


    private string[] classesNames;
    const float vari = 0.6f;
    const float z = 2;

    // Start is called before the first frame update
    void Start()
    {
        classesNames = classesFile.text.Split(',');
    }

    // Update is called once per frame
    public void UpdateCall(List<RunYOLOModel.ResultBox> results, int[] hint_position)
    {
        List<Transform> transformFixed = new List<Transform>();
        List<Vector2> boxSizes = new List<Vector2>();
        List<AudioClip> audioClips = new List<AudioClip>();
        foreach (RunYOLOModel.ResultBox i in results)
        {
            Rect box = i.rect;
            Rect new_box = new Rect(new Vector2((box.x - 0.5f) * vari, (0.5f - box.y - box.height) * vari), box.size * vari);
            relative2Cam.SetPosition(new Vector3(new_box.center.x, new_box.center.y, z));
            Transform temp = transform;
            temp.position = relative2Cam.GetWorldPosition();
            temp.rotation = relative2Cam.GetWorldRotation();
            transformFixed.Add(temp);
            boxSizes.Add(box.size);
            audioClips.AddRange(new AudioClip[3] { thereis, typeName[i.bestClassIdx], oClock[Mathf.FloorToInt(box.center.x * 3)] });
        }
        drawOutline.transformFixed = transformFixed;
        drawOutline.boxSizes = boxSizes;
        drawOnScreenHint.relativePos = new Vector3((float)(hint_position[0] * 2 - 7) / 9 * vari, (float)(7 - hint_position[1] * 2) / 16 * vari, z);
        StartCoroutine(PlayAudioSequentially(audioClips));
    }

    private IEnumerator PlayAudioSequentially(List<AudioClip> audioClips)
    {
        yield return null;

        for (int i = 0; i < audioClips.Count; i++)
        {
            adSource.clip = audioClips[i];
            adSource.Play();
            while (adSource.isPlaying)
            {
                yield return null;
            }
        }
        drawOnScreenHint.relativePos = Vector3.zero;
    }

}

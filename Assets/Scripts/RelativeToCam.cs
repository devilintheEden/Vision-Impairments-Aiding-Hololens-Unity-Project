using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeToCam : MonoBehaviour
{
    public void SetPosition(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public Vector3 GetWorldPosition()
    {
        return Utils.GetTranslation(transform.localToWorldMatrix);
    }

    public Quaternion GetWorldRotation()
    {
        return Utils.GetRotation(transform.localToWorldMatrix);
    }
}

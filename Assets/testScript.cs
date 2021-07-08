using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class testScript : ImmediateModeShapeDrawer
{
    public RelativeToCam relative2Cam;
    public float vari = 0.6f;
    public float z = 2;
    public int[] hint_position;
    void Start()
    {

    }

    public override void DrawShapes(Camera cam)
    {
        using (Draw.Command(cam))
        {
            relative2Cam.SetPosition(new Vector3((float)(hint_position[0] * 2 - 7) / 9 * vari, (float)(7 - hint_position[1] * 2) / 16 * vari, z));
            Draw.Ring(relative2Cam.GetWorldPosition(), relative2Cam.GetWorldRotation(), 0.01f, 0.006f, Color.red);
        }
    }
}


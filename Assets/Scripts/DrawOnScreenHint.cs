using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class DrawOnScreenHint : ImmediateModeShapeDrawer
{
    public RelativeToCam relative2Cam;
    public Vector3 relativePos = Vector3.zero;
    public override void DrawShapes(Camera cam)
    {
        using (Draw.Command(cam))
        {
            if(relativePos != Vector3.zero)
            {
                relative2Cam.SetPosition(relativePos);
                Draw.Ring(relative2Cam.GetWorldPosition(), relative2Cam.GetWorldRotation(), 0.01f, 0.006f, Color.red);
            }
        }
    }
}

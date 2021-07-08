using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class DrawOutline : ImmediateModeShapeDrawer
{
    public List<Transform> transformFixed = new List<Transform>();
    public List<Vector2> boxSizes = new List<Vector2>();
    public override void DrawShapes(Camera cam)
    {
        using (Draw.Command(cam))
        {
            int idx = 0;
            foreach (Transform t in transformFixed)
            {
                Draw.RectangleBorder(t.position, t.rotation, boxSizes[idx], 0.006f, Color.red);
                idx++;
            }
        }
    }
}

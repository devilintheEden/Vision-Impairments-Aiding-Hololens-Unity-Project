using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Utils
{

    public static T[] GetRange<T>(this ICollection<T> collection, int start = 0, int end = -1)
    {
        end = end < 0 ? end = collection.Count + end + 1 : end;
        var arr = Array.CreateInstance(typeof(T), end - start);
        for (int i = start, j = 0; i < end; i++, j++)
        {
            var v = collection.ElementAt(i);
            arr.SetValue(v, j);
        }
        return (T[])arr;
    }

    public static int MaxIdx(this ICollection<float> collection)
    {
        int idx = 0;
        float max = collection.ElementAt(0);
        for (int i = 1; i < collection.Count; i++)
        {
            if (collection.ElementAt(i) > max)
            {
                idx = i;
                max = collection.ElementAt(i);
            }
        }
        return idx;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
    {
        foreach (var e in enumerable) { func(e); }
    }

    public static void ForEach<T>(this ICollection<T> collection, Action<T, int> func)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            func(collection.ElementAt(i), i);
        }
    }

    public static IList<T> Update<T>(this IList<T> collection, Func<T, T> func)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            collection[i] = func(collection[i]);
        }
        return collection;
    }

    public static IList<T> Update<T>(this IList<T> collection, Func<T, int, T> func)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            collection[i] = func(collection[i], i);
        }
        return collection;
    }

    public static float Sigmoid(float value)
    {
        return 1f / (1f + Mathf.Exp(-value));
    }

    private static float _interval_overlap(float box1_min, float box1_max, float box2_min, float box2_max)
    {
        if (box2_min < box1_min)
        {
            if (box2_max < box1_min) { return 0; }
            else { return Mathf.Min(box1_max, box2_max) - box1_min; }
        }
        else
        {
            if (box1_max < box2_min) { return 0; }
            else { return Mathf.Min(box1_max, box2_max) - box2_min; }
        }
    }

    // IOU -> intersect / union
    public static float BoxesIOU(Rect box1, Rect box2)
    {
        float intersect_w = _interval_overlap(box1.x, box1.x + box1.width, box2.x, box2.x + box2.width);
        float intersect_h = _interval_overlap(box1.y, box1.y + box1.height, box2.y, box2.y + box2.height);

        float intersect = intersect_w * intersect_h;

        float union = box1.width * box1.height + box2.width * box2.height - intersect;
        return intersect / union;
    }

    public static void DrawRect(Texture2D tex, Rect rect, Color color, int width = 1, bool rectIsNormalized = true, bool revertY = false)
    {
        if (rectIsNormalized)
        {
            rect.x *= tex.width;
            rect.y *= tex.height;
            rect.width *= tex.width;
            rect.height *= tex.height;
        }

        if (revertY) { rect.y = rect.y * -1 + tex.height - rect.height; }

        if (rect.width <= 0 || rect.height <= 0) { return; }

        _draw_line(rect.x, rect.y, rect.width + width, width, color, tex);
        _draw_line(rect.x, rect.y + rect.height, rect.width + width, width, color, tex);

        _draw_line(rect.x, rect.y, width, rect.height + width, color, tex);
        _draw_line(rect.x + rect.width, rect.y, width, rect.height + width, color, tex);
        tex.Apply();
    }

    private static void _draw_line(float x, float y, float width, float height, Color col, Texture2D tex)
    {
        if (x > tex.width | y > tex.height) { return; }
        if (x < 0) { width += x; x = 0; }
        if (y < 0) { height += y; y = 0; }
        if (width < 0 || height < 0) { return; }

        width = x + width > tex.width ? tex.width - x : width;
        height = y + height > tex.height ? tex.height - y : height;

        int len = (int)width * (int)height;
        Color[] c = new Color[len];
        for (int i = 0; i < len; i++) { c[i] = col; }

        tex.SetPixels((int)x, (int)y, (int)width, (int)height, c);
    }

    public static int[] SortIdx(float[] values)
    {
        List<KeyValuePair<int, float>> dic = new List<KeyValuePair<int, float>>();
        values.ForEach((x, i) => dic.Add(new KeyValuePair<int, float>(i, x)));
        dic.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        return (int[])new int[values.Length].Update((x, i) => dic[i].Key);
    }

    public static Vector3 GetTranslation(this Matrix4x4 m)
    {
        var col = m.GetColumn(3);
        return new Vector3(col.x, col.y, col.z);
    }

    public static Quaternion GetRotation(this Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    public static Vector3 GetScale(this Matrix4x4 m)
    {
        return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
    }

}

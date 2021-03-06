using System;
using UnityEngine;
using UnityEngine.Profiling;

public class TextureCropTools
{
    // Based on https://gist.github.com/natsupy/e129936543f9b4663a37ea0762172b3b

    public static Texture2D CropToSquare(Texture2D tex)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        var xOffset = (tex.width - smaller) / 2;
        var yOffset = (tex.height - smaller) / 2;
        return CropWithRect(tex, new Rect(xOffset, yOffset, smaller, smaller));
    }

    public static Texture2D CropToSquare(WebCamTexture tex)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        return CropWithRect(tex, new Rect(0, 0, smaller, smaller));
    }

    public static void CropToSquare(WebCamTexture tex, ref Texture2D outputTexture)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        CropWithRect(tex, ref outputTexture, new Rect(0, 0, smaller, smaller));
    }

    public static void CropToSquare(Texture2D tex, ref Texture2D outputTexture)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        CropWithRect(tex, ref outputTexture, new Rect(0, 0, smaller, smaller));
    }

    public static void CropToSquare(WebCamTexture tex, int sideLength, ref Texture2D outputTexture)
    {
        if(sideLength > tex.width || sideLength > tex.height)
        {
            throw new System.ArgumentException("Invalid side length");
        }
        int rect_x = (tex.width - sideLength) / 2;
        int rect_y = (tex.height - sideLength) / 2;
        CropWithRect(tex, ref outputTexture, new Rect(rect_x, rect_y, sideLength, sideLength));
    }

    public static Texture2D CropWithRect(Texture2D texture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        Texture2D result = new Texture2D((int)rect.width, (int)rect.height);

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = (texture.width - rect.width) / 2;
            float yRect = (texture.height - rect.height) / 2;
            float widthRect = rect.width;
            float heightRect = rect.height;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }

            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }

        Profiler.EndSample();
        return result;
    }

    public static Texture2D CropWithRect(WebCamTexture texture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        Texture2D result = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = (texture.width - rect.width) / 2;
            float yRect = (texture.height - rect.height) / 2;
            float widthRect = rect.width;
            float heightRect = rect.height;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }

        Profiler.EndSample();
        return result;
    }

    public static void CropWithRect(WebCamTexture texture, ref Texture2D outputTexture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        if (outputTexture == null)
            outputTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (outputTexture.width != rect.width || outputTexture.height != rect.height)
            outputTexture.Resize((int)rect.width, (int)rect.height);

        Texture2D result = outputTexture;

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }
        Profiler.EndSample();
    }

    public static void CropWithRect(Texture2D texture, ref Texture2D outputTexture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        if (outputTexture == null)
            outputTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (outputTexture.width != rect.width || outputTexture.height != rect.height)
            outputTexture.Resize((int)rect.width, (int)rect.height);

        Texture2D result = outputTexture;

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }
        Profiler.EndSample();
    }

    public static Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

}

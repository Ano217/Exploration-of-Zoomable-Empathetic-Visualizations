using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructTools
{
    public static void echange(string[] t, int i, int j)
    {
        string tmp = t[i];
        t[i] = t[j];
        t[j] = tmp;
    }

    public static void sortTab(string[] t)
    {
        int n = t.Length;
        if (n == 0) return;
        try
        {
            for(int i = 0; i<n; i++)
            {
                int min = i;
                for(int j=i; j<n; j++)
                {
                    if (float.Parse(t[j]) < float.Parse(t[min])) min = j; 
                }
                if (min != i) echange(t, i, min);
            }
        }
        catch
        {
            for (int i = 0; i < n; i++)
            {
                string[] ti = t[i].Split('/');
                string[] minDate = ti;
                int min = i;
                for (int j = i; j < n; j++)
                {
                    string[] tj = t[j].Split('/');
                    if (float.Parse(minDate[1])>float.Parse(tj[1]) || 
                        (float.Parse(minDate[1]) == float.Parse(tj[1]) && float.Parse(minDate[0]) > float.Parse(tj[0]))){
                        minDate = tj;
                        min = j;
                    }
                }
                if (min != i) echange(t, i, min);
            }
        }
    }

    public static GameObject findShape(ShapeAssociation[] shapes, string key)
    {
        foreach (ShapeAssociation s in shapes)
        {
            if (s.categoryName == key)
            {
                return s.prefab[0];
            }
        }
        return null;
    }

    // sources : https://forum.unity.com/threads/change-rendering-mode-via-script.476437/
    public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Code from : https://forum.unity.com/threads/change-rendering-mode-via-script.476437/
public static class MaterialExtensions
{
    public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", 1);//(int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", 0);// (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", 5);// (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", 10);// (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;// (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

}
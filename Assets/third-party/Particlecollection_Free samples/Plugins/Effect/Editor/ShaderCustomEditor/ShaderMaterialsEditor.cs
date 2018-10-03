using UnityEditor;
using UnityEngine;

public class ShaderMaterialsEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        var bEnableCutOut = false;
        var bEnableDisTex = false;
        var bEnableUVRotation = false;
        var bEnableUVScroll = false;
        var bEnableUVMirror = false;
        var bEnableBloom = false;
        var bRange01 = false;
        var bRange02 = false;
        var bRange03 = false;
        var bRange04 = false;
        var targetMat = materialEditor.target as Material;
        foreach (var property in properties)
        {
            materialEditor.ShaderProperty(property, property.displayName);


            if (property.type == MaterialProperty.PropType.Texture)
            {
                if (property.name.Equals("_CutTex"))
                    if (property.textureValue != null)
                        bEnableCutOut = true;

                if (property.name.Equals("_DisTex"))
                    if (property.textureValue != null)
                        bEnableDisTex = true;
            }
            //   else if (property.type == MaterialProperty.PropType.Color)
            //   {

            //   }
            else if (property.type == MaterialProperty.PropType.Range)
            {
                if (property.name.Equals("_UVMirrorX")
                    && property.floatValue != 0.0f)
                    bEnableUVMirror = true;
                else if (property.name.Equals("_UVMirrorY")
                         && property.floatValue != 0.0f)
                    bEnableUVMirror = true;
                else if (property.name.Equals("_EmissionGain")
                         && property.floatValue != 0.0f)
                    bEnableBloom = true;
                else if (property.name.Equals("_MainRotation")
                         && property.floatValue != 0.0f)
                    bEnableUVRotation = true;
                else if (property.name.Equals("_Range01")
                         && property.floatValue != 0.0f)
                    bRange01 = true;
                else if (property.name.Equals("_Range02")
                         && property.floatValue != 0.0f)
                    bRange02 = true;
                else if (property.name.Equals("_Range03")
                         && property.floatValue != 0.0f)
                    bRange03 = true;
                else if (property.name.Equals("_Range04")
                         && property.floatValue != 0.0f)
                    bRange04 = true;


                if (bEnableCutOut)
                    if (property.name.Equals("_CutRotation")
                        && property.floatValue != 0.0f)
                        bEnableUVRotation = true;
            }
            else if (property.type == MaterialProperty.PropType.Float)
            {
                if (property.name.Equals("_MainRotation")
                    && property.floatValue != 0.0f)
                    bEnableUVRotation = true;
                else if (property.name.Equals("_UVScrollX")
                         && property.floatValue != 0.0f)
                    bEnableUVScroll = true;
                else if (property.name.Equals("_UVScrollY")
                         && property.floatValue != 0.0f)
                    bEnableUVScroll = true;

                if (bEnableCutOut)
                {
                    if (property.name.Equals("_CutRotation")
                        && property.floatValue != 0.0f)
                        bEnableUVRotation = true;
                    else if (property.name.Equals("_UVCutScrollX")
                             && property.floatValue != 0.0f)
                        bEnableUVScroll = true;
                    else if (property.name.Equals("_UVCutScrollY")
                             && property.floatValue != 0.0f)
                        bEnableUVScroll = true;
                }
            }
        }

        if (bEnableCutOut)
            targetMat.EnableKeyword("Enable_AlphaMask");
        else
            targetMat.DisableKeyword("Enable_AlphaMask");

        if (bEnableDisTex)
            targetMat.EnableKeyword("Enable_DisTex");
        else
            targetMat.DisableKeyword("Enable_DisTex");

        if (bEnableUVRotation)
            targetMat.EnableKeyword("Enable_UVRotation");
        else
            targetMat.DisableKeyword("Enable_UVRotation");

        if (bEnableUVScroll)
            targetMat.EnableKeyword("Enable_UVScroll");
        else
            targetMat.DisableKeyword("Enable_UVScroll");

        if (bEnableUVMirror)
            targetMat.EnableKeyword("Enable_UVMirror");
        else
            targetMat.DisableKeyword("Enable_UVMirror");

        if (bEnableBloom)
            targetMat.EnableKeyword("Enable_Bloom");
        else
            targetMat.DisableKeyword("Enable_Bloom");

        if (bRange01)
            targetMat.EnableKeyword("Enable_Range01");
        else
            targetMat.DisableKeyword("Enable_Range01");

        if (bRange02)
            targetMat.EnableKeyword("Enable_Range02");
        else
            targetMat.DisableKeyword("Enable_Range02");

        if (bRange03)
            targetMat.EnableKeyword("Enable_Range03");
        else
            targetMat.DisableKeyword("Enable_Range03");

        if (bRange04)
            targetMat.EnableKeyword("Enable_Range04");
        else
            targetMat.DisableKeyword("Enable_Range04");
    }
}
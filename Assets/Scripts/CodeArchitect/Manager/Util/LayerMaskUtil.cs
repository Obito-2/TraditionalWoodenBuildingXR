using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskUtil 
{
    public static int ChangeLayerMaskToInt(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask.value, 2);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorUtil 
{
    public static Vector3 ColorToVector3(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    public static Color Vector3ToColor(Vector3 vector)
    {
        return new Color(vector.x, vector.y, vector.z);
    }
    public static Vector3[] ColorArrayToVector3Array(Color[] colors)
    {
        Vector3[] result = new Vector3[colors.Length];
        for(int i = 0; i < colors.Length; i++)
        {
            result[i] = ColorToVector3(colors[i]);
        }
        return result;
    }
    public static Color[] Vector3ArrayToColorArray(Vector3[] vectors)
    {
        Color[] result = new Color[vectors.Length];
        for( int i = 0; i < vectors.Length; i++)
        {
            result[i] = Vector3ToColor(vectors[i]);
        }
        return result;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChoice: MonoBehaviour
{
    [Serializable]
    public class MaterialElement
    {
        public Material material;
        public string name;
    }
    public  List<MaterialElement> materialElements = new List<MaterialElement>();
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleLinkedListNode<T>
{
    public T Value { get; set; }
    public SingleLinkedListNode<T> Next { get; set; }
    public SingleLinkedListNode()
    {
        Value = default(T);
        Next = null;
    }
}

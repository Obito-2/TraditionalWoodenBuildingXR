using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPoolBase<T>  where T : class
{
    public SingleLinkedList<T> itemPool;

    public abstract T GetItem();
    public abstract void PushItem(T item);
}

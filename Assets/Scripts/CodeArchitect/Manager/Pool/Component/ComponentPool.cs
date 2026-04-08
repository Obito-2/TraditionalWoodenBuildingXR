using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPool : ObjectPoolBase<Component>
{
    public ComponentPool(Component component)
    {
        itemPool = new SingleLinkedList<Component>();
        PushItem(component);
    }
    public override Component GetItem()
    {
        var component = itemPool.First.Value;
        if(component == null) return null;
        itemPool.DeleteFirst();
        (component as Behaviour).enabled = true;
        return component;
    }

    public override void PushItem(Component component)
    {
        itemPool.AddLast(component);
        (component as Behaviour).enabled = false;
    }
}
//test
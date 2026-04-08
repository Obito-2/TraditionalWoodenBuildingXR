using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : ObjectPoolBase<GameObject>
{
    //ĳһ�໺�����Ϸ����ĸ����壬���������ָ����廹����һ��������
    public GameObject fatherObj;
    /// <summary>
    /// ��ʼ�������
    /// </summary>
    /// <param name="poolObj">������洢��ĳһ����Ϸ����</param>
    /// <param name="gameObjectRoot">�������͵Ļ������Ϸ����ĸ�����</param>
    public GameObjectPool(GameObject poolObj, GameObject gameObjectRoot)
    {
        //��һ����󴴽�һ�������󣬲��ҰѸ�������Ϊpool�������������
        fatherObj = new GameObject(poolObj.name);
        fatherObj.transform.parent = gameObjectRoot.transform;
        itemPool = new SingleLinkedList<GameObject>();
        PushItem(poolObj);
    }
    public override GameObject GetItem()
    {
        var obj = itemPool.First.Value;
        itemPool.DeleteFirst();
        obj.transform.SetParent(null,false);
        obj.SetActive(true);
        return obj;
    }
    public override void PushItem(GameObject item)
    {
        itemPool.AddLast(item);
        item.SetActive(false);
        item.transform.SetParent(fatherObj.transform,false);
    }
}

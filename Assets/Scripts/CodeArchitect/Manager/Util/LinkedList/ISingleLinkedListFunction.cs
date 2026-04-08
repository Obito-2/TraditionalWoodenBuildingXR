using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISingleLinkedListFunction<T>
{
    /// <summary>
    /// ��һ���ڵ�
    /// </summary>
    SingleLinkedListNode<T> First { get; }

    /// <summary>
    /// ���һ���ڵ�
    /// </summary>
    SingleLinkedListNode<T> Last { get; }

    /// <summary>
    /// ��������
    /// </summary>
    int Count { get; }
    /// <summary>
    /// �����Ƿ�Ϊ��
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// �������
    /// </summary>
    void Clear();

    /// <summary>
    /// �������Ƿ����������
    /// </summary>
    /// <param name="value">��֤������</param>
    /// <returns></returns>
    bool Contains(T value);

    /// <summary>
    /// ���һ���ڵ㵽�ڵ����
    /// </summary>
    /// <param name="value">�ڵ�����</param>
    /// <returns>����Node�ڵ�</returns>
    SingleLinkedListNode<T> AddFirst(T value);

    /// <summary>
    /// ���һ���ڵ㵽�ڵ����
    /// </summary>
    /// <param name="node">��Ҫ��ӵĽڵ�</param>
    /// <returns>��ӵĲ����ǲ����ڵ�,���ǲ�������,������ӵĽڵ�</returns>
    SingleLinkedListNode<T> AddFirst(SingleLinkedListNode<T> node);

    /// <summary>
    /// ���һ���ڵ㵽�������
    /// </summary>
    /// <param name="value">�ڵ�����</param>
    /// <returns>�ڵ�</returns>
    SingleLinkedListNode<T> AddLast(T value);

    ///<summary>
    /// ���һ���ڵ㵽�������
    /// </summary>
    /// <param name="node">��Ҫ��ӵĽڵ�</param>
    /// <returns>��ӵĲ����ǲ����ڵ�,���ǲ�������,������ӵĽڵ�</returns>
    SingleLinkedListNode<T> AddLast(SingleLinkedListNode<T> node);

    /// <summary>
    /// ����һ���ڵ㵽ָ�����±��
    /// </summary>
    /// <param name="value">�ڵ�����</param>
    /// <param name="index">�����±�</param>
    /// <returns>�����ڵ�</returns>
    SingleLinkedListNode<T> Insert(T value, int index);

    /// <summary>
    /// ����һ���ڵ㵽ָ�����±��
    /// </summary>
    /// <param name="node">�ڵ�</param>
    /// <param name="index">�����±�</param>
    /// <returns>��ӵĲ����ǲ����ڵ�,���ǲ�������,������ӵĽڵ�</returns>
    SingleLinkedListNode<T> Insert(SingleLinkedListNode<T> node, int index);

    /// <summary>
    /// ɾ���ڵ�
    /// </summary>
    /// <param name="value">����</param>
    /// <returns>�Ƿ�ɾ���ɹ�</returns>
    bool Delete(T value);

    /// <summary>
    /// ɾ���ڵ�
    /// </summary>
    /// <param name="index">ɾ���Ľڵ��±�</param>
    /// <returns>�Ƿ�ɾ���ɹ�</returns>
    bool DeleteAt(int index);

    /// <summary>
    /// ɾ���ڵ�
    /// </summary>
    /// <param name="index">ɾ���Ľڵ�</param>
    /// <returns>�Ƿ�ɾ���ɹ�</returns>
    bool Delete(SingleLinkedListNode<T> node);

    /// <summary>
    /// ɾ����һ���ڵ�
    /// </summary>
    /// <returns>�Ƿ�ɾ���ɹ�</returns>
    bool DeleteFirst();

    /// <summary>
    /// ɾ�����һ���ڵ�
    /// </summary>
    /// <returns>�Ƿ�ɾ���ɹ�</returns>
    bool DeleteLast();

    /// <summary>
    /// �������ݲ��ҽڵ�
    /// </summary>
    /// <param name="value">����</param>
    /// <returns>�ڵ�</returns>
    SingleLinkedListNode<T> Find(T value);

    /// <summary>
    /// �������ݲ�����һ���ڵ�
    /// </summary>
    /// <param name="value">����</param>
    /// <returns>�ڵ�</returns>
    SingleLinkedListNode<T> FindPrevious(T value);

    /// <summary>
    /// ������ȡ����
    /// </summary>
    /// <param name="index">�±�</param>
    /// <returns></returns>
    T this[int index] { get; }

    /// <summary>
    /// �����±�ȡ����
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    T GetElement(int index);

    /// <summary>
    /// �������ݻ�ȡ�±�
    /// </summary>
    /// <param name="value">����</param>
    /// <returns>���ݽڵ��±�</returns>
    int IndexOf(T value);
}

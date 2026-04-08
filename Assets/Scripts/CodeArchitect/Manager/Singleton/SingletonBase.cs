public class SingletonBase<T> where T:new()
{
    private static T instance;
    // ���̰߳�ȫ����
    private static readonly object locker = new object();
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //lockдif������Ϊֻ�и����ʵ����û����ʱ������Ҫ����
                lock (locker)
                {
                    if (instance == null)
                        instance = new T();
                }
            }
            return instance;
        }
    }
}



using System.Collections.Generic;

public class UnsortList<T>
{
    int mCount = 0;
    List<T> mDataList;

    public List<T> dataList
    {
        get { return mDataList; }
    }

    public int Capacity
    {
        get { return mDataList.Capacity; }
    }

    public int Count
    {
        get { return mCount; }
    }

    public UnsortList(int cap)
    {
        mDataList = new List<T>(cap);
    }

    public void Add(T data)
    {
        mDataList.Add(data);
    }

    public void Remove(T data)
    {
        mDataList.Remove(data);
    }

    public void Clear(T data)
    {
       mCount = 0;
    }
}

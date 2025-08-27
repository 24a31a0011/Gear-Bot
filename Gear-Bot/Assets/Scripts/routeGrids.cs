///
/// 作成者　グエン
///

using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class routeGrids<T>
{
    private List<T> list = new List<T>();

    // 変更時のイベント
    public event Action OnChanged;

    // コンストラクタ
    public routeGrids()
    {
        list = new List<T>();
    }

    public routeGrids(IEnumerable<T> collection)
    {
        list = new List<T>(collection);
    }

    // 要素を追加
    public void Add(T item)
    {
        list.Add(item);
        OnChanged?.Invoke();
    }

    // 要素を削除
    public void Remove(T item)
    {
        list.Remove(item);
        OnChanged?.Invoke();
    }

    // インデクサ
    public T this[int index]
    {
        get => list[index];
        set
        {
            list[index] = value;
            OnChanged?.Invoke();
        }
    }

    // 要素数
    public int Count => list.Count;

    // Listに存在するか判定
    public bool Contains(T item)
    {
        return list.Contains(item);
    }

    // 最後の要素を取得（存在しなければ例外）
    public T Last
    {
        get
        {
            if (list.Count == 0)
                throw new InvalidOperationException("リストが空です。");
            return list[list.Count - 1];
        }
    }

    // 最後の要素を安全に取得（存在しなければdefault）
    public T LastOrDefault => list.Count > 0 ? list[list.Count - 1] : default;

    // 内部のListをコピーして取得したい場合
    public List<T> ToList() => new List<T>(list);
}

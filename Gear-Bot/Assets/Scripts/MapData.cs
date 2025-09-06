using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapRow
{
    public int[] tiles;  // 1行分のタイルデータ
}

public class MapData : MonoBehaviour
{
    [Header("マップデータ（行ごとの配列）")]
    public MapRow[] map;  // これをインスペクターで設定可能

    void Start()
    {
        // マップ表示サンプル
        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].tiles.Length; x++)
            {
                Debug.Log($"({x},{y}) = {map[y].tiles[x]}");
            }
        }
    }
}

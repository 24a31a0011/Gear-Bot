///
/// 作成者 : グエン
///
using UnityEngine;

public enum TileType
{
    Abyss = 0,      // 奈落
    Ground = 1,     // 地面
    PowerGear = 2,  // 電源ギア
    GimmickGear = 3,// ギミックギア
    Gimmick = 4     // ギミック
}

[System.Serializable]
public class TileData
{
    public TileType type;

    // ギミックの場合のみ設定したい GameObject
    public GameObject gimmickPrefab;
}

[System.Serializable]
public class MapRow
{
    public TileData[] tiles;  // 1行分のデータ
}

public class MapData : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static MapData Instance { get; private set; }

    [Header("マップデータ")]
    public MapRow[] map;

    [Header("マップ左上のワールド座標")]
    public Vector3 worldTopLeft = Vector3.zero; // yは無視

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    /// <summary>
    /// ワールド座標(x,z)を入力して対応する TileData を返す
    /// </summary>
    public TileData GetTileData(Vector3 worldPos)
    {
        // ワールド座標からマップ座標に変換
        int mapX = Mathf.FloorToInt((worldPos.x - worldTopLeft.x) / 1);
        int mapY = Mathf.FloorToInt((worldTopLeft.z - worldPos.z) / 1); // 上が0なので z方向は反転

        // 範囲外チェック
        if (mapY < 0 || mapY >= map.Length) return null;
        if (mapX < 0 || mapX >= map[mapY].tiles.Length) return null;

        return map[mapY].tiles[mapX];
    }
}

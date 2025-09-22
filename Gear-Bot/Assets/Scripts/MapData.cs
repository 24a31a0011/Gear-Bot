///
/// 作成者 : グエン
///
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Abyss,      // 奈落
    Ground,     // 地面
    PowerGear,  // 電源ギア
    GimmickGear,// ギミックギア
    Bridge,     // 橋
    Belt,       // ベルトコンベア
    Obstacle,   // 障害物
    Goal,       // ゴール
    Bag,        // 荷物
    Gate,       // ゲート
    Arm,        // アーム
    WaitingArea // アームの待機所
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
    [SerializeField] MapRow[] map;

    [Header("マップ左上のワールド座標")]
    [SerializeField] Vector3 worldTopLeft = Vector3.zero; // yは無視

    // 障害物のワールド座標を登録するリスト
    private List<Vector3> obstaclePositions = new List<Vector3>();

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
    /// 外部から障害物のリストを渡す
    /// </summary>
    public void SetObstacles(List<Transform> obstacles)
    {
        obstaclePositions.Clear();

        foreach (var obs in obstacles)
        {
            if (obs != null)
            {
                obstaclePositions.Add(obs.position);
            }
        }
    }

    /// <summary>
    /// ワールド座標(x,z)から TileData を取得する
    /// </summary>
    public TileData GetTileData(Vector3 worldPos)
    {
        Vector3Int mapPos = WorldToMap(worldPos);

        // 範囲外チェック
        if (mapPos.y < 0 || mapPos.y >= map.Length ||
            mapPos.x < 0 || mapPos.x >= map[mapPos.y].tiles.Length)
        {
            return new TileData { type = TileType.Abyss };
        }

        // 基本タイル
        TileData baseTile = map[mapPos.y].tiles[mapPos.x];

        // 障害物チェック
        foreach (var obsPos in obstaclePositions)
        {
            Vector3Int obsMapPos = WorldToMap(obsPos);

            if (obsMapPos.x == mapPos.x && obsMapPos.y == mapPos.y)
            {
                return new TileData { type = TileType.Obstacle };
            }
        }

        return baseTile;
    }

    /// <summary>
    /// ワールド座標をマップ座標に変換
    /// </summary>
    public Vector3Int WorldToMap(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - worldTopLeft.x) / 1);
        int y = Mathf.FloorToInt((worldTopLeft.z - worldPos.z) / 1);
        return new Vector3Int(x, y, 0);
    }
}

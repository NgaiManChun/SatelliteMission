using ExtensionMethods;
using UnityEngine;

// =======================================================
// SatelliteController
// -------------------------------------------------------
// 対象の周囲を旋回するオブジェクト制御クラス
//
// サテライトガンの通常時の周回挙動を担当する
// =======================================================

public class SatelliteController : MonoBehaviour
{
    public GameObject target;

    // 周回中心からのオフセット
    public Vector3 position_offset = Vector3.zero;

    // 周回周期
    public float cycle_period = 1.0f;

    // 周回半径
    public float radius = 2.0f;

    // 周回面の回転周期
    public float rotate_period = 1.0f;

    // 個体ごとの時間差
    public float time_offset = 0.0f;

    // 周回再開時に現在位置から戻る時間
    public float return_pos_time = 0.5f;

    public bool active = false;

    private float time_count = 0.0f;

    // 周回位置へ戻る補間用
    private float return_t = 0.0f;

    private Vector3 return_start_position;
    private Quaternion return_start_rotation;

    private Renderer render;

    void Start()
    {
        render = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (active && target)
        {
            // 周回中心座標
            Vector3 center = target.GetCenterPoint() + position_offset;

            // 基本の円軌道位置
            float cycle_t =
                ((time_count + time_offset) % cycle_period) / cycle_period;

            float x =
                radius * Mathf.Cos(Mathf.PI * cycle_t * 360 / 180);

            float y = 0;

            float z =
                radius * Mathf.Sin(Mathf.PI * cycle_t * 360 / 180);

            // 円軌道自体を回転させ、単純な水平回転にならないようにする
            float rotate_t =
                ((time_count + time_offset) % rotate_period) / rotate_period;

            float angle = 360.0f * rotate_t;

            Vector3 local_pos =
                Quaternion.Euler(0, 0, angle) * new Vector3(x, y, z);

            Vector3 world_pos =
                center + local_pos;

            // 周回再開時は、現在位置から周回位置へ補間して戻す
            if (return_t < 1.0f)
            {
                world_pos =
                    Vector3.Lerp(transform.position, world_pos, return_t);

                return_t += Time.fixedDeltaTime / return_pos_time;
            }

            transform.LookAt(world_pos, transform.up);
            transform.position = world_pos;
        }

        time_count += Time.fixedDeltaTime;
    }

    // 周回開始
    public void start()
    {
        return_t = 0.0f;

        return_start_position = transform.position;
        return_start_rotation = transform.rotation;

        active = true;
    }

    // 周回停止
    public void stop()
    {
        active = false;
    }
}
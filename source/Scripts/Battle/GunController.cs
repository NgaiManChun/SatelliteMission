using ExtensionMethods;
using UnityEngine;

// =======================================================
// GunController
// -------------------------------------------------------
// サテライトガン1基分の射撃制御クラス
//
// 発射状態、連射間隔、弾生成、マズルフラッシュ、
// 射撃時の位置・角度ブレを管理する
// =======================================================

public class GunController : MonoBehaviour
{
    // 射撃状態
    private enum GUN_STATE
    {
        READY,
        FIRE_START,
        FIRE,
        FIRE_END
    };

    public GameObject owner;

    [Header("秒間発数")]
    public float rate_of_fire = 7.0f;

    [Header("位置ブレ")]
    public float position_vel = 0.04f;

    [Header("角度ブレ")]
    public float rotation_vel = 4.0f;

    private GUN_STATE state = GUN_STATE.READY;

    // 弾の発射位置
    private Transform gunpoint;

    // 次弾発射までの進行率
    private float next_fire_processed = 1.0f;

    // 射撃中に蓄積したブレ
    private Vector3 vel_accumulated_position = Vector3.zero;
    private Quaternion vel_accumulated_rotation = Quaternion.identity;

    private Vector3 vel_position = Vector3.zero;
    private Quaternion vel_rotation = Quaternion.identity;

    private SatelliteGunController satelliteGunController;

    void Start()
    {
        gunpoint = transform.Find("gunpoint");
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        // 射撃開始
        if (state == GUN_STATE.FIRE_START)
        {
            vel_accumulated_position = Vector3.zero;
            vel_accumulated_rotation = Quaternion.identity;

            state = GUN_STATE.FIRE;
        }

        // 射撃終了
        else if (state == GUN_STATE.FIRE_END)
        {
            transform.Find("MuzzleFlash").gameObject.SetActive(false);

            transform.position -= vel_accumulated_position;

            transform.rotation =
                transform.rotation * Quaternion.Inverse(vel_accumulated_rotation);

            vel_position = Vector3.zero;
            vel_rotation = Quaternion.identity;

            state = GUN_STATE.READY;
        }

        // 射撃中
        else if (state == GUN_STATE.FIRE)
        {
            transform.Find("MuzzleFlash").gameObject.SetActive(true);

            if (next_fire_processed >= 1.0f)
            {
                next_fire_processed = 0.0f;

                // 残弾消費
                if (satelliteGunController.current_load - satelliteGunController.use_load < 0)
                {
                    satelliteGunController.current_load = 0;
                }
                else
                {
                    satelliteGunController.current_load -= satelliteGunController.use_load;
                }

                // 弾生成
                GameObject bullet = (GameObject)Resources.Load("bullet");

                bullet.SetOwner(owner);

                Vector3 init_position = gunpoint.position;

                Quaternion ro = Quaternion.LookRotation(transform.forward);

                Instantiate(bullet, init_position, ro);
            }
        }

        // 次弾発射までの進行率更新
        next_fire_processed =
            Mathf.Min(
                next_fire_processed + rate_of_fire * Time.fixedDeltaTime,
                1.0f
            );
    }

    // 射撃時の位置ブレを取得
    public Vector3 getVelPosition()
    {
        return new Vector3(
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f)
        ) * position_vel / 2;
    }

    // 射撃時の角度ブレを取得
    public Quaternion getVelRotation()
    {
        return Quaternion.Euler(
            new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
            ) * rotation_vel / 2
        );
    }

    // 射撃開始
    public void fire()
    {
        if (state == GUN_STATE.READY)
        {
            state = GUN_STATE.FIRE_START;
        }
    }

    // 射撃停止
    public void stop()
    {
        if (state == GUN_STATE.FIRE)
        {
            state = GUN_STATE.FIRE_END;
        }
    }

    public void SetSatelliteGunController(SatelliteGunController satelliteGunController)
    {
        this.satelliteGunController = satelliteGunController;
    }
}
using ExtensionMethods;
using UnityEngine;

// サテライトガン個別情報
public class Satellite_Gun_Individual
{
    public GunController gun;

    // 発射時配置オフセット
    public Vector3 fire_offset;

    // 元位置へ戻すための情報
    public Vector3 return_start_position;
    public Quaternion return_start_rotation;

    public SatelliteController satelliteController;
}

// =======================================================
// SatelliteGunController
// -------------------------------------------------------
// プレイヤー周囲を旋回するサテライトガン制御クラス
//
// 通常時はSatelliteControllerによって周回し、
// 攻撃時は指定位置へ移動して射撃を行う
// =======================================================

public class SatelliteGunController : WeaponController
{
    // サテライトガン状態
    private enum GUN_STATE
    {
        INIT,
        READY,
        FIRE_START,
        FIRE,
        FIRE_END
    };

    public string name = "Satellite Guns";

    [Header("連結Gameobject")]
    public GunController[] guns;

    [Header("発射時の銃の位置")]
    public Vector3[] fire_offsets;

    [Header("最大残弾数")]
    public int max_load_value = 120;

    [Header("残弾回復秒速")]
    public int load_speed = 4;

    [Header("最大連射数")]
    public int max_continued_fire = 21;

    [Header("一発消耗弾数")]
    public int use_load = 1;

    public GameObject owner;

    // 元位置へ戻る時間
    public float return_pos_time = 0.2f;

    // 起動演出時間
    public float startup_time = 1.0f;

    private float startup_count = 0.0f;

    // 現在残弾数
    public float current_load = 0;

    // UI表示用
    public WeaponFeedback weapon_feedback;

    // 個別サテライト情報
    public Satellite_Gun_Individual[] gun_inds =
        new Satellite_Gun_Individual[2];

    private GUN_STATE state = GUN_STATE.INIT;

    // 射撃位置補間用
    private float return_t = 0.0f;

    private Vector3 aiming_to = Vector3.zero;
    private GameObject target;

    void Start()
    {
        gun_inds = new Satellite_Gun_Individual[guns.Length];

        // =======================================================
        // サテライトガン初期化
        // =======================================================

        for (int i = 0; i < guns.Length; i++)
        {
            gun_inds[i] = new Satellite_Gun_Individual();

            gun_inds[i].gun = guns[i];
            gun_inds[i].fire_offset = fire_offsets[i];

            gun_inds[i].return_start_position =
                guns[i].transform.position;

            gun_inds[i].return_start_rotation =
                guns[i].transform.rotation;

            SatelliteController satelliteController =
                guns[i].GetComponent<SatelliteController>();

            if (satelliteController)
            {
                satelliteController.target = this.gameObject;

                satelliteController.start();

                gun_inds[i].satelliteController =
                    satelliteController;
            }

            guns[i].SetSatelliteGunController(this);

            // プレイヤーの子から外して独立移動させる
            guns[i].transform.parent = null;

            guns[i].owner = this.gameObject;
        }

        current_load = max_load_value;

        // 武器UI情報
        weapon_feedback = new WeaponFeedback();

        weapon_feedback.name = name;

        weapon_feedback.max_value = max_load_value;

        weapon_feedback.current_value =
            Mathf.FloorToInt(current_load);
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        // UI更新
        weapon_feedback.current_value =
            Mathf.FloorToInt(current_load);

        // =======================================================
        // 初期出現演出
        // =======================================================

        if (state == GUN_STATE.INIT)
        {
            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                gun_ind.gun.gameObject
                    .GetComponent<Renderer>()
                    .material.color =
                    Color.Lerp(
                        Color.clear,
                        Color.white,
                        startup_count / startup_time
                    );

                gun_ind.gun.transform.parent = null;
            }

            if (startup_count > startup_time)
            {
                state = GUN_STATE.READY;
            }

            startup_count += Time.fixedDeltaTime;
        }

        // =======================================================
        // 攻撃開始
        // =======================================================

        if (state == GUN_STATE.FIRE_START)
        {
            return_t = 0.0f;

            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                // 周回停止
                if (gun_ind.satelliteController)
                {
                    gun_ind.satelliteController.stop();
                }

                // 現在位置保存
                gun_ind.return_start_position =
                    gun_ind.gun.transform.position;

                gun_ind.return_start_rotation =
                    gun_ind.gun.transform.rotation;
            }

            state = GUN_STATE.FIRE;
        }

        // =======================================================
        // 攻撃終了
        // =======================================================

        else if (state == GUN_STATE.FIRE_END)
        {
            return_t = 0.0f;

            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                gun_ind.gun.stop();

                SatelliteController satelliteController =
                    gun_ind.gun.GetComponent<SatelliteController>();

                // 周回再開
                if (gun_ind.satelliteController)
                {
                    gun_ind.satelliteController.start();
                }
            }

            state = GUN_STATE.READY;
        }

        // =======================================================
        // 射撃中
        // =======================================================

        else if (state == GUN_STATE.FIRE)
        {
            // 射撃位置へ移動完了後
            if (return_t >= 1f)
            {
                foreach (Satellite_Gun_Individual gun_ind in gun_inds)
                {
                    // 射撃位置へ配置
                    gun_ind.gun.transform.position =
                        transform.position +
                        Quaternion.LookRotation(
                            aiming_to - transform.position
                        ) * gun_ind.fire_offset;

                    gun_ind.gun.transform.LookAt(aiming_to);

                    // 慣性移動
                    gun_ind.gun.transform.position +=
                        gun_ind.gun.getVelPosition();

                    gun_ind.gun.transform.rotation *=
                        gun_ind.gun.getVelRotation();

                    // 残弾チェック
                    if (current_load >= use_load)
                    {
                        gun_ind.gun.fire();
                    }
                    else
                    {
                        gun_ind.gun.stop();
                    }
                }
            }

            // 射撃位置へ移動中
            else
            {
                foreach (Satellite_Gun_Individual gun_ind in gun_inds)
                {
                    Vector3 final_position =
                        transform.position +
                        Quaternion.LookRotation(
                            aiming_to - transform.position
                        ) * gun_ind.fire_offset;

                    Quaternion final_rotation =
                        Quaternion.FromToRotation(
                            final_position,
                            aiming_to
                        );

                    float t = return_t.EaseInOutQuad();

                    gun_ind.gun.transform.position =
                        Vector3.Slerp(
                            gun_ind.return_start_position,
                            final_position,
                            t
                        );

                    gun_ind.gun.transform.rotation =
                        Quaternion.Slerp(
                            gun_ind.return_start_rotation,
                            final_rotation,
                            t
                        );
                }
            }
        }

        // =======================================================
        // 残弾回復
        // =======================================================

        if (state == GUN_STATE.READY)
        {
            float load_amount =
                load_speed * Time.fixedDeltaTime;

            if (current_load + load_amount > max_load_value)
            {
                current_load = max_load_value;
            }
            else
            {
                current_load += load_amount;
            }
        }

        // 補間進行
        return_t += Time.fixedDeltaTime / return_pos_time;
    }

    // 攻撃開始
    override public void fire(Vector3 aiming_to, GameObject target)
    {
        this.aiming_to = aiming_to;
        this.target = target;

        if (state == GUN_STATE.READY)
        {
            state = GUN_STATE.FIRE_START;
        }
    }

    // 攻撃停止
    override public void stop()
    {
        this.aiming_to = Vector3.zero;
        this.target = null;

        if (state == GUN_STATE.FIRE)
        {
            state = GUN_STATE.FIRE_END;
        }
    }

    private void OnDestroy()
    {
        // サテライトガン削除
        foreach (GunController gun in guns)
        {
            if (gun != null && gun.gameObject != null)
            {
                Destroy(gun.gameObject);
            }
        }
    }
}
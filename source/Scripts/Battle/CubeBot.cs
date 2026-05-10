using ExtensionMethods;
using UnityEngine;

// =======================================================
// CubeBot
// -------------------------------------------------------
// 敵Bot用モデルクラス
//
// 飛行移動、ビーム攻撃、残弾回復、
// 破壊処理などを管理する
// =======================================================

public class CubeBot : CharacterModel
{
    [Header("最大残弾数")]
    public int max_load_value = 3;

    [Header("残弾回復秒速")]
    public float load_speed = 0.5f;

    [Header("秒間発数")]
    public float rate_of_fire = 1.0f;

    // UI表示用
    private WeaponFeedback weapon_feedback;

    // 攻撃進行
    private float main_attack_progress = 1.0f;

    // 次弾発射までの進行率
    private float next_fire_processed = 1.0f;

    public float current_load = 0;

    private Vector3 aiming_to;
    private GameObject target;

    public override void Start()
    {
        base.Start();

        // 武器UI情報初期化
        weapon_feedback = new WeaponFeedback();

        weapon_feedback.name = "Beam";
        weapon_feedback.max_value = max_load_value;
        weapon_feedback.current_value = Mathf.FloorToInt(current_load);

        weapon_feedbacks.Add(weapon_feedback);
    }

    public override void Attack(string id, Vector3 aiming_to, GameObject target)
    {
        if (id == "main")
        {
            this.aiming_to = aiming_to;
            this.target = target;

            main_attack_progress = 0.0f;
        }
    }

    protected override void BeforeUpdate()
    {
        weapon_feedback.current_value =
            Mathf.FloorToInt(current_load);

        // =======================================================
        // 破壊処理
        // =======================================================

        if (destroying)
        {
            if (destory_progress >= 1.0f)
            {
                Instantiate(
                    (GameObject)Resources.Load("BigExplosion"),
                    gameObject.GetCenterPoint(),
                    transform.rotation
                );

                Destroy(this.gameObject);

                if (small_explosion)
                {
                    Destroy(small_explosion);
                }
            }

            lock_action = true;

            move_jump = false;
            move_step = false;
            move_fly = false;

            jump_progress = 1.0f;

            destory_progress += Time.fixedDeltaTime / destory_action_time;
        }
        else
        {
            rb.useGravity = false;

            Vector3 direction = move_direction;

            // =======================================================
            // メイン攻撃
            // =======================================================

            if (main_attack_progress < 1.0f)
            {
                transform.LookAt(aiming_to);

                if (next_fire_processed >= 1.0f && current_load > 1.0f)
                {
                    GameObject bullet =
                        (GameObject)Resources.Load("laserB");

                    bullet.SetOwner(gameObject);

                    Vector3 init_position =
                        transform.position + transform.forward;

                    Quaternion ro =
                        Quaternion.LookRotation(transform.forward);

                    Instantiate(bullet, init_position, ro);

                    current_load--;

                    main_attack_progress = 1.0f;

                    next_fire_processed = 0.0f;
                }

                direction = Vector3.zero;
            }

            // =======================================================
            // 移動方向へ回転
            // =======================================================

            if (direction != Vector3.zero)
            {
                float rotate_angle =
                    Vector3.Angle(transform.forward, direction * 2.0f);

                float delta_rotate_speed =
                    max_rotate_speed * Time.fixedDeltaTime;

                float t = 1.0f;

                if (delta_rotate_speed < rotate_angle)
                {
                    t = delta_rotate_speed / rotate_angle;
                }

                Quaternion rotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.Lerp(transform.rotation, rotation, t);

                // 一定以上方向がズレている場合は前進しない
                if (rotate_angle > start_move_angle)
                {
                    direction = Vector3.zero;
                }
            }

            // 最高速度まで加速
            Vector3 max_velocity =
                direction.normalized * max_speed;

            Vector3 add_velocity =
                (max_velocity - rb.velocity) *
                Time.fixedDeltaTime / time_to_peak;

            rb.velocity += add_velocity;
        }

        // =======================================================
        // 残弾回復
        // =======================================================

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

        // 次弾発射までの進行率更新
        next_fire_processed =
            Mathf.Min(
                next_fire_processed + rate_of_fire * Time.fixedDeltaTime,
                1.0f
            );
    }

    protected override void AfterUpdate()
    {
        base.AfterUpdate();
    }

    public override void SetMoveDirection(Vector3 move_direction)
    {
        this.move_direction = move_direction.normalized;
    }
}
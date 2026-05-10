using ExtensionMethods;
using UnityEngine;

// =======================================================
// RadarDish
// -------------------------------------------------------
// レーダー用モデルクラス
//
// CubeBot生成とHP回復処理を持つ拠点キャラクター
// =======================================================

public class RadarDish : CharacterModel
{
    // CubeBot生成関連
    public int cube_bot_max_load = 1;
    public float cube_bot_fire_rate = 0.2f;
    public float cube_bot_recover_rate = 0.03f;
    public string cube_bot_name = "Cube Bot";

    // HP回復関連
    public int hp_recover_max_load = 1;
    public float hp_recover_fire_rate = 10f;
    public float hp_recover_recover_rate = 1f;
    public int hp_recover_amount = 1;
    public string hp_recover_name = "Recover";

    // CubeBot生成状態
    private bool moving_attack_main = false;
    private float cube_bot_current_load = 0.0f;
    private float cube_bot_next_fire_processed = 1.0f;

    // HP回復状態
    private bool moving_attack_sub = false;
    private float hp_recover_current_load = 0.0f;
    private float hp_recover_next_fire_processed = 1.0f;

    // UI表示用
    private WeaponFeedback weapon_feedback;

    public override void Start()
    {
        base.Start();

        // CubeBot生成用UI情報初期化
        weapon_feedback = new WeaponFeedback();

        weapon_feedback.name = cube_bot_name;
        weapon_feedback.max_value = cube_bot_max_load;
        weapon_feedback.current_value = Mathf.FloorToInt(cube_bot_current_load);

        weapon_feedbacks.Add(weapon_feedback);
    }

    protected override void BeforeUpdate()
    {
        base.BeforeUpdate();

        // 破壊中は各行動を停止
        if (destroying)
        {
            moving_attack_main = false;
            moving_attack_sub = false;
        }

        // =======================================================
        // CubeBot生成
        // =======================================================

        if (moving_attack_main)
        {
            if (cube_bot_next_fire_processed >= 1.0f && cube_bot_current_load >= 1.0f)
            {
                GameObject cube_bot_obj =
                    (GameObject)Resources.Load("cube_bot");

                CharacterModel model =
                    (CharacterModel)cube_bot_obj.GetComponent<CharacterModel>();

                CubeBotController cube_bot_controller =
                    (CubeBotController)cube_bot_obj.GetComponent<CubeBotController>();

                cube_bot_controller.pattren_type =
                    CUBE_BOT_PATTREN_TYPE.DEFENSE_AREA;

                cube_bot_controller.defense_position =
                    transform.position;

                model.team = this.team;

                Instantiate(
                    cube_bot_obj,
                    gameObject.GetCenterPoint() + transform.forward * 4.0f,
                    Quaternion.identity
                );

                cube_bot_current_load =
                    Mathf.Max(cube_bot_current_load - 1, 0.0f);

                cube_bot_next_fire_processed = 0.0f;
            }
        }

        // =======================================================
        // HP回復
        // =======================================================

        if (moving_attack_sub)
        {
            if (hp_recover_next_fire_processed >= 1.0f && hp_recover_current_load >= 1.0f)
            {
                hp =
                    Mathf.Min(hp + hp_recover_amount, max_hp);

                hp_recover_current_load =
                    Mathf.Max(hp_recover_current_load - 1, 0.0f);

                hp_recover_next_fire_processed = 0.0f;
            }
        }
    }

    protected override void AfterUpdate()
    {
        base.AfterUpdate();

        // UI反映
        weapon_feedback.current_value =
            Mathf.FloorToInt(cube_bot_current_load);

        // CubeBot生成ゲージ回復
        cube_bot_current_load =
            Mathf.Min(
                cube_bot_current_load +
                cube_bot_recover_rate * Time.fixedDeltaTime,
                cube_bot_max_load
            );

        cube_bot_next_fire_processed =
            Mathf.Min(
                cube_bot_next_fire_processed +
                cube_bot_fire_rate * Time.fixedDeltaTime,
                1.0f
            );

        // HP回復ゲージ回復
        hp_recover_current_load =
            Mathf.Min(
                hp_recover_current_load +
                hp_recover_recover_rate * Time.fixedDeltaTime,
                hp_recover_max_load
            );

        hp_recover_next_fire_processed =
            Mathf.Min(
                hp_recover_next_fire_processed +
                hp_recover_fire_rate * Time.fixedDeltaTime,
                1.0f
            );
    }

    public override void Attack(string id, Vector3 aiming_to, GameObject target)
    {
        if (id == "main")
        {
            moving_attack_main = true;
        }
        else if (id == "sub")
        {
            moving_attack_sub = true;
        }
    }
}
using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

// =======================================================
// MissionSequenceController
// -------------------------------------------------------
// ミッション専用進行管理クラス
//
// 敵AIの役割分担、レーダー防衛、Bot生成制御、
// ランク判定などを管理する
// =======================================================

public class MissionSequenceController : BattleSequenceController
{
    // プレイヤー側レーダー
    public RadarDish self_radar;

    // 敵Bot最大数
    public int bot_limit = 8;

    // ランク表示
    public string[] ranks = { "S", "A", "B", "C", "D", "E" };

    // 各ランク到達時間
    public float[] rank_times = { 140f, 170f, 200f, 240f, 300f };

    // CPU行動再編成周期
    private float pattren_ajust_period = 20.0f;

    private float pattren_ajust_count = 0.0f;

    public override void Update()
    {
        base.Update();

        // =======================================================
        // CPU行動再編成
        // =======================================================

        if (pattren_ajust_count == 0.0f)
        {
            CharacterModel[] characters =
                GetCharacterModels(2);

            List<RadarDish> radars =
                new List<RadarDish>();

            List<CubeBot> cube_bots =
                new List<CubeBot>();

            // レーダーとBotを分類
            foreach (CharacterModel character in characters)
            {
                if (character is RadarDish)
                {
                    radars.Add(character as RadarDish);
                }
                else if (character is CubeBot)
                {
                    cube_bots.Add(character as CubeBot);
                }
            }

            int num_of_radar = radars.Count;
            int num_of_bots = cube_bots.Count;

            // レーダー攻撃担当数
            int radar_attack_bot =
                Mathf.Min(Mathf.RoundToInt(num_of_bots / 4), 4);

            // プレイヤー攻撃担当数
            int player_attack_bot =
                Mathf.Min(Mathf.RoundToInt(num_of_bots / 4), 4);

            // =======================================================
            // 防衛Bot割り当て
            // =======================================================

            if (num_of_radar > 0)
            {
                int defense_bot =
                    num_of_bots -
                    radar_attack_bot -
                    player_attack_bot;

                int defense_bot_per_radar =
                    Mathf.Min(
                        Mathf.RoundToInt(defense_bot / num_of_radar),
                        4
                    );

                foreach (RadarDish radar in radars)
                {
                    // レーダーから近い順にBotを並び替える
                    CharacterDistanceComparer distance_comparer =
                        new CharacterDistanceComparer();

                    distance_comparer.origin =
                        radar.gameObject.GetCenterPoint();

                    cube_bots.Sort(distance_comparer);

                    int defense_bot_count = 0;

                    while (cube_bots.Count > 0 &&
                           defense_bot_count < defense_bot_per_radar)
                    {
                        CubeBot cube_bot = cube_bots[0];

                        CubeBotController cube_bot_controller =
                            (CubeBotController)cube_bot.GetComponent<CubeBotController>();

                        cube_bot_controller.defense_position =
                            distance_comparer.origin;

                        cube_bot_controller.pattren_type =
                            CUBE_BOT_PATTREN_TYPE.DEFENSE_AREA;

                        cube_bots.Remove(cube_bot);

                        defense_bot_count++;
                    }

                    // Bot生成制御
                    RadarDishController radar_controller =
                        (RadarDishController)radar.GetComponent<RadarDishController>();

                    if (num_of_bots < bot_limit)
                    {
                        radar_controller.make_bot = true;
                    }
                    else
                    {
                        radar_controller.make_bot = false;
                    }
                }
            }

            // =======================================================
            // レーダー攻撃担当
            // =======================================================

            int radar_attack_bot_count = 0;

            while (cube_bots.Count > 0 &&
                   radar_attack_bot_count < radar_attack_bot)
            {
                CubeBot cube_bot = cube_bots[0];

                CubeBotController cube_bot_controller =
                    (CubeBotController)cube_bot.GetComponent<CubeBotController>();

                cube_bot_controller.pattren_type =
                    CUBE_BOT_PATTREN_TYPE.ATTACK_RADAR;

                cube_bots.Remove(cube_bot);

                radar_attack_bot_count++;
            }

            // =======================================================
            // プレイヤー攻撃担当
            // =======================================================

            int player_attack_bot_count = 0;

            while (cube_bots.Count > 0 &&
                   player_attack_bot_count < player_attack_bot)
            {
                CubeBot cube_bot = cube_bots[0];

                CubeBotController cube_bot_controller =
                    (CubeBotController)cube_bot.GetComponent<CubeBotController>();

                cube_bot_controller.pattren_type =
                    CUBE_BOT_PATTREN_TYPE.ATTACK_PLAYER;

                cube_bots.Remove(cube_bot);

                player_attack_bot_count++;
            }

            // =======================================================
            // 残りは近距離優先行動
            // =======================================================

            foreach (CubeBot cube_bot in cube_bots)
            {
                CubeBotController cube_bot_controller =
                    (CubeBotController)cube_bot.GetComponent<CubeBotController>();

                cube_bot_controller.pattren_type =
                    CUBE_BOT_PATTREN_TYPE.ATTACK_NEAR;
            }
        }

        // 再編成タイマー更新
        pattren_ajust_count += Time.deltaTime;

        if (pattren_ajust_count > pattren_ajust_period)
        {
            pattren_ajust_count = 0.0f;
        }
    }

    // =======================================================
    // 敗北条件
    // =======================================================

    protected override bool CheckLose()
    {
        // 自軍レーダー破壊
        bool result = (self_radar.hp == 0);

        if (result && playerController.player_character)
        {
            playerController.player_character.audio_source.PlayOneShot(
                playerController.player_character.audio_clips[12]
            );
        }

        return base.CheckLose() || result;
    }

    // =======================================================
    // ランク判定
    // =======================================================

    protected override string GetRank()
    {
        for (int i = 0; i < ranks.Length || i < rank_times.Length; i++)
        {
            if (time_count < rank_times[i])
            {
                return ranks[i];
            }
        }

        return ranks[ranks.Length - 1];
    }
}
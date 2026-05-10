using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

// =======================================================
// EmemyController
// -------------------------------------------------------
// 敵キャラクターの簡易AI制御クラス
//
// プレイヤー周辺への移動、攻撃判定、
// 障害物を考慮した移動先補正を行う
// =======================================================

public class EmemyController : MonoBehaviour
{
    CharacterModel self_character;

    // 移動制御用ポイント
    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p3;

    private float t = 2.0f;
    private float distance = 0.0f;
    private float height = 0.0f;

    private Collider collider;
    private LineRenderer lineRenderer;

    // 攻撃可能状態
    private bool can_main_attack = true;

    // 行動状態
    private ACTION_PATTERN action_pattern = ACTION_PATTERN.POSITION;

    private enum ACTION_PATTERN
    {
        POSITION,
        WANDERING,
        ATTACK
    }

    void Start()
    {
        self_character = GetComponent<CharacterModel>();
        lineRenderer = GetComponent<LineRenderer>();
        collider = GetComponent<Collider>();

        p0 = transform.position;
        p1 = transform.position;
        p2 = transform.position;
        p3 = transform.position;
    }

    void Update()
    {
        CharacterModel player = BattleSequenceController.getPlayerCharacter();

        if (player)
        {
            // 新しい移動先を決定
            if (action_pattern == ACTION_PATTERN.POSITION)
            {
                // プレイヤー周辺のランダム位置を目標地点にする
                float outer_distance = Random.Range(5f, 50f);
                p0 = player.transform.position + Random.onUnitSphere * outer_distance;

                Vector3 p1 = gameObject.GetCenterPoint();
                Vector3 p2 = p0;

                List<Vector3> positions = new List<Vector3>();
                positions.Add(p1);

                LayerMask layerMask = 1 << LayerMask.NameToLayer("ground");

                bool hitted = false;

                // 目標地点までの間に障害物がある場合、反射方向へ目標地点を補正する
                do
                {
                    hitted = false;

                    foreach (RaycastHit hit in Physics.RaycastAll(p1, (p2 - p1).normalized, Vector3.Distance(p1, p2)))
                    {
                        if (hit.collider.gameObject.isStatic)
                        {
                            positions.Add(hit.point);

                            p1 = hit.point;
                            p2 = hit.point + Vector3.Reflect(p2 - hit.point, hit.normal);

                            hitted = true;
                            break;
                        }
                    }
                } while (hitted);

                p0 = p2;

                positions.Add(p2);

                action_pattern = ACTION_PATTERN.WANDERING;
            }

            // 攻撃可能な距離・残弾であれば攻撃状態へ移行
            if (can_main_attack)
            {
                float distance = Vector3.Distance(player.gameObject.GetCenterPoint(), gameObject.GetCenterPoint());

                if (distance < 30.0f)
                {
                    foreach (WeaponFeedback weaponFeedback in self_character.GetWeaponFeedbacks())
                    {
                        if (weaponFeedback.current_value > 1.0f)
                        {
                            action_pattern = ACTION_PATTERN.ATTACK;
                        }
                    }
                }
            }

            // 目標地点へ移動
            if (action_pattern == ACTION_PATTERN.WANDERING)
            {
                self_character.SetMoveDirection((p0 - gameObject.GetCenterPoint()).normalized);

                if (Vector3.Distance(gameObject.GetCenterPoint(), p0) < collider.bounds.size.z)
                {
                    can_main_attack = true;
                    action_pattern = ACTION_PATTERN.POSITION;
                }
            }

            // プレイヤーへ攻撃
            else if (action_pattern == ACTION_PATTERN.ATTACK)
            {
                self_character.Attack("main", player.gameObject.GetCenterPoint(), player.gameObject);

                action_pattern = ACTION_PATTERN.POSITION;

                // 残弾が少ない場合は、しばらく移動行動へ戻す
                foreach (WeaponFeedback weaponFeedback in self_character.GetWeaponFeedbacks())
                {
                    if (weaponFeedback.current_value < 1.0f)
                    {
                        can_main_attack = false;
                    }
                }
            }
        }
    }
}
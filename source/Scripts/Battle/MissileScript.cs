using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

// =======================================================
// MissileScript
// -------------------------------------------------------
// 誘導ミサイルの挙動を管理するクラス
//
// 発射後、一定間隔でターゲット方向を補正しながら飛行し、
// 命中時に爆発範囲内のキャラクターへダメージを与える
// =======================================================

public class MissileScript : MonoBehaviour
{
    // 推進力
    public float speed = 10.0f;

    // 有効射程
    public float effective_distance = 100.0f;

    // 攻撃力
    public int power = 1;

    // 誘導補正のブレ量
    public float vel = 0.5f;

    // 誘導方向を再計算する間隔
    public float adjust_load_time = 0.15f;

    // 爆発範囲
    public float explosion_raduis = 1.0f;

    // 発射した所有者
    public GameObject owner;

    // 誘導対象
    public GameObject target;

    private Rigidbody rigidbody;

    // 発射済みフラグ
    private bool shooted = false;

    // 移動距離カウント
    private float distance_count = 0.0f;

    private Vector3 prevous_position = Vector3.zero;

    // 命中状態
    private bool hitted = false;
    private Vector3 hitted_position = Vector3.zero;

    // 誘導補正タイマー
    private float adjust_load = 1.0f;

    private int adjust_count = 0;

    private Vector3 explosion_debugs_position = Vector3.zero;

    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();

        prevous_position = transform.position;
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        // 命中後、命中位置で削除する
        if (hitted)
        {
            rigidbody.position = hitted_position;

            Destroy(this.gameObject);
        }

        // 初回のみ前方へ打ち出す
        if (!shooted && !hitted)
        {
            rigidbody.AddForce(
                transform.forward * speed / 2 * Time.fixedDeltaTime,
                ForceMode.Impulse
            );

            shooted = true;
        }
        else
        {
            // ターゲットがある場合、一定間隔で誘導方向を補正する
            if (target)
            {
                if (adjust_load >= 1.0f)
                {
                    adjust_load = 0.0f;

                    Vector3 target_position =
                        target.GetCenterPoint();

                    float distance =
                        Vector3.Distance(target_position, transform.position);

                    // 目標位置にランダム性を加え、完全追尾になりすぎないようにする
                    target_position +=
                        Random.insideUnitSphere * distance * vel;

                    transform.LookAt(target_position);
                }
            }

            // 現在の前方方向へ推進
            rigidbody.AddForce(
                transform.forward * speed * Time.fixedDeltaTime,
                ForceMode.Force
            );

            adjust_load += Time.fixedDeltaTime / adjust_load_time;

            adjust_load =
                Mathf.Min(1.0f, adjust_load);
        }

        // 射程距離を超えたら削除
        if (distance_count >= effective_distance)
        {
            Destroy(this.gameObject);
        }

        distance_count +=
            Vector3.Distance(prevous_position, transform.position);

        prevous_position = transform.position;
    }

    // 命中処理
    private void handleHit(Vector3 position, GameObject gameObject)
    {
        // 爆発エフェクト生成
        GameObject SmallExplosion =
            (GameObject)Resources.Load("SmallExplosion");

        Instantiate(
            SmallExplosion,
            position,
            transform.rotation
        );

        // 直接命中したキャラクター
        List<CharacterModel> characters =
            new List<CharacterModel>();

        CharacterModel character =
            getCharacterModel(gameObject);

        if (character)
        {
            characters.Add(character);
        }

        // 爆発範囲内のキャラクター取得
        Collider[] colliders =
            Physics.OverlapSphere(position, explosion_raduis);

        foreach (Collider collider in colliders)
        {
            CharacterModel _character =
                getCharacterModel(collider.gameObject);

            if (_character && !characters.Contains(_character))
            {
                characters.Add(_character);
            }
        }

        // 範囲内の敵キャラクターへダメージ
        foreach (CharacterModel _character in characters)
        {
            bool sameTeam =
                (_character.team == owner.GetTeam());

            if (!sameTeam)
            {
                _character.GotHit(power, sameTeam);
            }
        }
    }

    // GameObjectまたはRootからCharacterModelを取得する
    private CharacterModel getCharacterModel(GameObject gameObject)
    {
        CharacterModel char_model =
            gameObject.GetComponent<CharacterModel>();

        if (!char_model && gameObject.transform.root != null)
        {
            char_model =
                gameObject.transform.root.GetComponent<CharacterModel>();
        }

        return char_model;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isOwner =
            (other.gameObject == owner);

        bool sameOwner =
            (other.gameObject.GetOwner() == owner);

        // 自分自身、または同じ所有者の弾には当たらない
        if (!isOwner && !sameOwner)
        {
            hitted = true;

            hitted_position =
                other.ClosestPointOnBounds(transform.position);

            handleHit(hitted_position, other.gameObject);
        }
    }
}
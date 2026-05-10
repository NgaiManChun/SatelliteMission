using ExtensionMethods;
using UnityEngine;

// =======================================================
// BulletScript
// -------------------------------------------------------
// 直進弾の挙動を管理するクラス
//
// 発射方向への移動、射程距離による削除、
// 命中時のエフェクト生成とダメージ処理を行う
// =======================================================

public class BulletScript : MonoBehaviour
{
    // 弾速
    public float speed = 10.0f;

    // 有効射程
    public float effective_distance = 100.0f;

    // 攻撃力
    public int power = 1;

    // 発射した所有者
    public GameObject owner;

    private Rigidbody rigidbody;

    // 発射済みフラグ
    private bool shooted = false;

    // 移動距離カウント
    private float distance_count = 0.0f;

    private Vector3 prevous_position = Vector3.zero;

    // 命中状態
    private bool hitted = false;
    private Vector3 hitted_position = Vector3.zero;

    // public Quaternion 

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

        // 初回のみ速度を設定して発射
        if (!shooted && !hitted)
        {
            rigidbody.velocity = transform.forward * speed;

            shooted = true;
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
        // 命中エフェクト生成
        GameObject HitEffect =
            (GameObject)Resources.Load("HitEffect");

        Instantiate(
            HitEffect,
            position,
            Quaternion.Inverse(transform.rotation)
        );

        // 命中対象からCharacterModelを取得
        CharacterModel char_model =
            gameObject.GetComponent<CharacterModel>();

        if (!char_model && gameObject.transform.root != null)
        {
            char_model =
                gameObject.transform.root.GetComponent<CharacterModel>();
        }

        // キャラクターに命中した場合はダメージ処理
        if (char_model)
        {
            bool sameTeam =
                (char_model.team == owner.GetTeam());

            char_model.GotHit(power, sameTeam);
        }
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
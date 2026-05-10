using UnityEngine;
using UnityEngine.UI;

// =======================================================
// StatusMonitor
// -------------------------------------------------------
// プレイヤーステータスUI制御クラス
//
// HP減少時の被弾演出として、
// UI揺れとフィルター点滅を行う
// =======================================================

public class StatusMonitor : MonoBehaviour
{
    // HPゲージ
    public Image hp_bar;

    // 揺らす対象
    public RectTransform inner_transform;

    // 被弾フィルター
    public Image filter;

    // 通常時フィルター色
    public Color filter_normal_color =
        new Color(1, 0, 0, 0);

    // 被弾時フィルター色
    public Color filter_hit_color =
        new Color(1, 0, 0, 60f / 255f);

    // 揺れ強度
    public float hit_effect_vel = 3.0f;

    private float previous_hp = 0.0f;

    // 被弾演出進行率
    private float hit_effect_progress = 1.0f;

    // 被弾演出時間
    private float hit_effect_time = 0.5f;

    // UI初期位置
    private Vector3 hit_effect_origin = Vector3.zero;

    void Start()
    {
        previous_hp = hp_bar.fillAmount;

        hit_effect_origin =
            inner_transform.localPosition;
    }

    private void FixedUpdate()
    {
        // =======================================================
        // 被弾演出
        // =======================================================

        if (hit_effect_progress < 1.0f)
        {
            // フィルター点滅
            filter.color =
                Color.Lerp(
                    filter_normal_color,
                    filter_hit_color,
                    1.0f - Mathf.Abs(hit_effect_progress * 2 - 1.0f)
                );

            // UI揺れ
            inner_transform.localPosition =
                hit_effect_origin +
                new Vector3(
                    Random.Range(-1, 1),
                    Random.Range(-1, 1),
                    Random.Range(-1, 1)
                ) * hit_effect_vel * hit_effect_progress;

            filter.transform.localPosition =
                inner_transform.localPosition;

            hit_effect_progress +=
                Time.fixedDeltaTime / hit_effect_time;
        }
        else
        {
            // 演出終了後は元位置へ戻す
            filter.color = filter_normal_color;

            inner_transform.localPosition =
                hit_effect_origin;

            filter.transform.localPosition =
                inner_transform.localPosition;
        }

        // HP減少時に被弾演出開始
        if (hp_bar.fillAmount < previous_hp)
        {
            hit_effect_progress = 0.0f;
        }

        previous_hp = hp_bar.fillAmount;
    }
}
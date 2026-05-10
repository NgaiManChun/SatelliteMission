using UnityEngine;
using UnityEngine.UI;

public class StatusMonitor : MonoBehaviour
{
    public Image hp_bar;
    public RectTransform inner_transform;
    public Image filter;
    public Color filter_normal_color = new Color(1, 0, 0, 0);
    public Color filter_hit_color = new Color(1, 0, 0, 60f / 255f);
    public float hit_effect_vel = 3.0f;

    private float previous_hp = 0.0f;

    private float hit_effect_progress = 1.0f;
    private float hit_effect_time = 0.5f;
    private Vector3 hit_effect_origin = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        previous_hp = hp_bar.fillAmount;
        hit_effect_origin = inner_transform.localPosition;
    }

    private void FixedUpdate()
    {

        if (hit_effect_progress < 1.0f)
        {

            filter.color = Color.Lerp(filter_normal_color, filter_hit_color, 1.0f - Mathf.Abs(hit_effect_progress * 2 - 1.0f));

            inner_transform.localPosition = hit_effect_origin + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * hit_effect_vel * hit_effect_progress;
            filter.transform.localPosition = inner_transform.localPosition;
            hit_effect_progress += Time.fixedDeltaTime / hit_effect_time;
        }
        else
        {
            filter.color = filter_normal_color;
            inner_transform.localPosition = hit_effect_origin;
            filter.transform.localPosition = inner_transform.localPosition;
        }
        if (hp_bar.fillAmount < previous_hp)
        {
            hit_effect_progress = 0.0f;
        }
        previous_hp = hp_bar.fillAmount;
    }
}

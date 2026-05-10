using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =======================================================
// WeaponMonitor
// -------------------------------------------------------
// 武器情報表示用UIクラス
//
// 武器名、残弾数、残弾ゲージを保持する
// =======================================================

public class WeaponMonitor : MonoBehaviour
{
    // 武器名
    public TextMeshProUGUI name_label;

    // 残弾数表示
    public TextMeshProUGUI load_label;

    // 残弾ゲージ
    public Image load_bar;
}
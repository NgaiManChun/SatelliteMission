using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =======================================================
// CharacterMonitor
// -------------------------------------------------------
// キャラクター情報表示用UIクラス
//
// HP、敵味方ラベル、名前、距離、
// ターゲット表示用クロスヘアを保持する
// =======================================================

public class CharacterMonitor : MonoBehaviour
{
    // HPゲージ
    public Slider hp_slider;

    // 敵ラベル
    public Image enemy_label;

    // 味方ラベル
    public Image friend_label;

    // キャラクター名
    public TextMeshProUGUI name_label;

    // プレイヤーからの距離表示
    public TextMeshProUGUI distance_label;

    // ロックオン表示
    public RawImage crosshair;
}
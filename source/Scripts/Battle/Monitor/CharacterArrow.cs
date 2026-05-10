using UnityEngine;
using UnityEngine.UI;

// =======================================================
// CharacterArrow
// -------------------------------------------------------
// 画面外キャラクター表示用矢印UIクラス
//
// 敵味方に応じて矢印色を切り替える
// =======================================================

public class CharacterArrow : MonoBehaviour
{
    public Image image;

    // 敵用カラー
    public Color enemy_color =
        new Color(1f, 0, 0, 100f / 255f);

    // 味方用カラー
    public Color friend_color =
        new Color(0f, 101f / 255f, 1f, 100f / 255f);

    // 敵味方に応じて色を設定
    public void SetColor(bool is_enemy)
    {
        image.color =
            (is_enemy)
            ? enemy_color
            : friend_color;
    }
}
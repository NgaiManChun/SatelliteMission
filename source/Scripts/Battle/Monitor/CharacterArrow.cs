using UnityEngine;
using UnityEngine.UI;

public class CharacterArrow : MonoBehaviour
{
    public Image image;
    public Color enemy_color = new Color(1f, 0, 0, 100f/255f);
    public Color friend_color = new Color(0f, 101f/255f, 1f, 100f / 255f);
    
    public void SetColor(bool is_enemy)
    {
        image.color = (is_enemy)?enemy_color: friend_color;
    }
}

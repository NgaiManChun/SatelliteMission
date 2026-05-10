using ExtensionMethods;
using UnityEngine;

public class RadarDishController : MonoBehaviour
{

    public CharacterModel self_character;

    public bool make_bot = false;

    // Start is called before the first frame update
    void Start()
    {
        self_character = GetComponent<CharacterModel>();
    }

    // Update is called once per frame
    void Update()
    {
        if(BattleSequenceController.GetSceneState() == SCENE_STATE.PLAYING)
        {
            if (make_bot)
            {
                self_character.Attack("main", gameObject.GetCenterPoint(), gameObject);
            }
            
            self_character.Attack("sub", gameObject.GetCenterPoint(), gameObject);
        }
        
    }
}

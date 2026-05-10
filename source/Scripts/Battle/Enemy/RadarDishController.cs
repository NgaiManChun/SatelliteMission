using ExtensionMethods;
using UnityEngine;

// =======================================================
// RadarDishController
// -------------------------------------------------------
// レーダーの行動制御クラス
//
// ミッション中に、状況に応じてCubeBot生成と
// 自身のHP回復処理を実行する
// =======================================================

public class RadarDishController : MonoBehaviour
{
    public CharacterModel self_character;

    // CubeBot生成を行うかどうか
    public bool make_bot = false;

    void Start()
    {
        self_character = GetComponent<CharacterModel>();
    }

    void Update()
    {
        if (BattleSequenceController.GetSceneState() == SCENE_STATE.PLAYING)
        {
            // 必要に応じてCubeBot生成を実行
            if (make_bot)
            {
                self_character.Attack(
                    "main",
                    gameObject.GetCenterPoint(),
                    gameObject
                );
            }

            // HP回復を実行
            self_character.Attack(
                "sub",
                gameObject.GetCenterPoint(),
                gameObject
            );
        }
    }
}
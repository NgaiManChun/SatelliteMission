using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

public class MissionSequenceController : BattleSequenceController
{
    public RadarDish self_radar;
    public int bot_limit = 8;
    public string[] ranks = { "S", "A", "B", "C", "D", "E" };
    public float[] rank_times = { 140f, 170f, 200f, 240f, 300f };

    private float pattren_ajust_period = 20.0f;
    private float pattren_ajust_count = 0.0f;

    public override void Update()
    {
        base.Update();
        if(pattren_ajust_count == 0.0f)
        {
            // CPU ë‘ê®óßÇƒíºÇ∑
            CharacterModel[] characters = GetCharacterModels(2);
            
            List< RadarDish > radars = new List< RadarDish >();
            List< CubeBot > cube_bots = new List< CubeBot >();
            foreach (CharacterModel character in characters)
            {
                if(character is RadarDish)
                {
                    radars.Add(character as RadarDish);
                }
                else if(character is CubeBot)
                {
                    cube_bots.Add(character as CubeBot);
                }
            }
            int num_of_radar = radars.Count;
            int num_of_bots = cube_bots.Count;
            int radar_attack_bot = Mathf.Min(Mathf.RoundToInt(num_of_bots / 4), 4);
            int player_attack_bot = Mathf.Min(Mathf.RoundToInt(num_of_bots / 4), 4);
            if(num_of_radar > 0)
            {
                int defense_bot = num_of_bots - radar_attack_bot - player_attack_bot;
                int defense_bot_per_radar = Mathf.Min(Mathf.RoundToInt(defense_bot / num_of_radar), 4);
                foreach (RadarDish radar in radars)
                {
                    CharacterDistanceComparer distance_comparer = new CharacterDistanceComparer();
                    distance_comparer.origin = radar.gameObject.GetCenterPoint();
                    cube_bots.Sort(distance_comparer);
                    int defense_bot_count = 0;
                    while (cube_bots.Count > 0 && defense_bot_count < defense_bot_per_radar)
                    {
                        CubeBot cube_bot = cube_bots[0];
                        CubeBotController cube_bot_controller = (CubeBotController)cube_bot.GetComponent<CubeBotController>();
                        cube_bot_controller.defense_position = distance_comparer.origin;
                        cube_bot_controller.pattren_type = CUBE_BOT_PATTREN_TYPE.DEFENSE_AREA;
                        cube_bots.Remove(cube_bot);
                        defense_bot_count++;
                    }
                    RadarDishController radar_controller = (RadarDishController)radar.GetComponent<RadarDishController>();
                    if (num_of_bots < bot_limit)
                    {
                        radar_controller.make_bot = true;
                    }
                    else
                    {
                        radar_controller.make_bot = false;
                    }
                }
            }

            int radar_attack_bot_count = 0;
            while (cube_bots.Count > 0 && radar_attack_bot_count < radar_attack_bot)
            {
                CubeBot cube_bot = cube_bots[0];
                CubeBotController cube_bot_controller = (CubeBotController)cube_bot.GetComponent<CubeBotController>();
                cube_bot_controller.pattren_type = CUBE_BOT_PATTREN_TYPE.ATTACK_RADAR;
                cube_bots.Remove(cube_bot);
                radar_attack_bot_count++;
            }

            int player_attack_bot_count = 0;
            while (cube_bots.Count > 0 && player_attack_bot_count < player_attack_bot)
            {
                CubeBot cube_bot = cube_bots[0];
                CubeBotController cube_bot_controller = (CubeBotController)cube_bot.GetComponent<CubeBotController>();
                cube_bot_controller.pattren_type = CUBE_BOT_PATTREN_TYPE.ATTACK_PLAYER;
                cube_bots.Remove(cube_bot);
                player_attack_bot_count++;
            }

            foreach(CubeBot cube_bot in cube_bots)
            {
                CubeBotController cube_bot_controller = (CubeBotController)cube_bot.GetComponent<CubeBotController>();
                cube_bot_controller.pattren_type = CUBE_BOT_PATTREN_TYPE.ATTACK_NEAR;
            }

        }
        

        pattren_ajust_count += Time.deltaTime;
        if(pattren_ajust_count > pattren_ajust_period)
        {
            pattren_ajust_count = 0.0f;
        }
    }

    protected override bool CheckLose()
    {
        bool result = (self_radar.hp == 0);
        if(result && playerController.player_character)
        {
            playerController.player_character.audio_source.PlayOneShot(playerController.player_character.audio_clips[12]);
        }
        return base.CheckLose() || result;
    }

    protected override string GetRank()
    {
        
        for(int i = 0; i < ranks.Length || i < rank_times.Length; i++)
        {
            if (time_count < rank_times[i])
            {
                return ranks[i];
            }
        }
        return ranks[ranks.Length - 1];
    }
}

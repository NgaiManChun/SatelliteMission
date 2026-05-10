using UnityEngine;

static public class Options
{
    static private float _camera_speed = 1.0f;
    static private bool _camera_speed_loaded = false;
    public const float camera_speed_max = 2.0f;
    public const float camera_speed_min = 0.3f;

    static private float _bgm_volume = 0.0f;
    static private bool _bgm_volume_loaded = false;
    public const float bgm_volume_max = 20.0f;
    public const float bgm_volume_min = -80f;

    static private float _se_volume = 0.0f;
    static private bool _se_volume_loaded = false;
    public const float se_volume_max = 20.0f;
    public const float se_volume_min = -80f;

    static public float camera_speed
    {
        get {
            if (!_camera_speed_loaded)
            {
                _camera_speed = PlayerPrefs.GetFloat("camera_speed", 1.0f);
                _camera_speed_loaded = true;
            }
            
            return _camera_speed;
        }
        set {
            _camera_speed = value;
            PlayerPrefs.SetFloat("camera_speed", value);
            PlayerPrefs.Save();
            _camera_speed_loaded = true;
        }
    }

    static public float bgm_volume
    {
        get {
            if (!_bgm_volume_loaded)
            {
                _bgm_volume = PlayerPrefs.GetFloat("bgm_volume", 0.0f);
                _bgm_volume_loaded = true;
            }

            return _bgm_volume;
        }
        set {
            _bgm_volume = value;
            PlayerPrefs.SetFloat("bgm_volume", value);
            PlayerPrefs.Save();
            _bgm_volume_loaded = true;
        }
    }

    static public float se_volume
    {
        get
        {
            if (!_se_volume_loaded)
            {
                _se_volume = PlayerPrefs.GetFloat("se_volume", 0.0f);
                _se_volume_loaded = true;
            }

            return _se_volume;
        }
        set
        {
            _se_volume = value;
            PlayerPrefs.SetFloat("se_volume", value);
            PlayerPrefs.Save();
            _se_volume_loaded = true;
        }
    }

    static public bool fullScreen
    {
        get {
            return Screen.fullScreen; 
        }
        set {
            PlayerPrefs.SetInt("fullScreen", (value)?1:0);
            Screen.fullScreen = value;
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScript : MonoBehaviour
{
    public GameObject main_menu;
    public GameObject option_menu;
    public GameObject credit_panel;
    public Slider camera_speed_slider;
    public Slider bgm_slider;
    public Slider se_slider;
    public Toggle fullScreen_toggle;
    public Animation main_animation;
    public Animation options_animation;
    public Animation credit_animation;
    public AudioMixer audio_mixer;
    private bool init_camera_speed_slider = false;
    private bool init_bgm_slider = false;
    private bool init_se_slider = false;
    private bool init_full_screen = false;
    // Start is called before the first frame update
    void Start()
    {
        camera_speed_slider.maxValue = Options.camera_speed_max;
        camera_speed_slider.minValue = Options.camera_speed_min;
        camera_speed_slider.value = Options.camera_speed;
        init_camera_speed_slider = true;

        bgm_slider.maxValue = Options.bgm_volume_max;
        bgm_slider.minValue = Options.bgm_volume_min;
        bgm_slider.value = Options.bgm_volume;
        init_bgm_slider = true;

        se_slider.maxValue = Options.se_volume_max;
        se_slider.minValue = Options.se_volume_min;
        se_slider.value = Options.se_volume;
        init_se_slider = true;

        fullScreen_toggle.isOn = Options.fullScreen;
        init_full_screen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        main_menu.SetActive(true);
        option_menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float bgm_volume;
        audio_mixer.GetFloat("bgm_volume", out bgm_volume);
    }

    public void OnButtonClick(string id)
    {
        if(id == "mission1")
        {
            SceneManager.LoadScene("mission1");
        }
        else if (id == "mission2")
        {
            SceneManager.LoadScene("mission2");
        }
        else if (id == "option")
        {
            options_animation.Play("panel_in");
            main_menu.SetActive(false);
            option_menu.SetActive(true);
        }
        else if (id == "credit")
        {
            credit_animation.Play("panel_in");
            main_menu.SetActive(false);
            credit_panel.SetActive(true);
        }
        else if (id == "return")
        {
            main_animation.Play("panel_in");
            main_menu.SetActive(true);
            option_menu.SetActive(false);
            credit_panel.SetActive(false);
        }
        else if(id == "exit")
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    public void CameraSpeedChange(Slider slider)
    {
        if(init_camera_speed_slider)
        {
            Options.camera_speed = slider.value;
        }
        
    }

    public void BGMVolumeChange(Slider slider)
    {
        if (init_camera_speed_slider)
        {
            Options.bgm_volume = slider.value;
            audio_mixer.SetFloat("bgm_volume", slider.value);
        }
    }

    public void SEVolumeChange(Slider slider)
    {
        if (init_se_slider)
        {
            Options.se_volume = slider.value;
            audio_mixer.SetFloat("se_volume", slider.value);
        }
    }

    public void FullScreenChange(Toggle toggle)
    {
        if (init_full_screen)
        {
            Options.fullScreen = toggle.isOn;
        }
    }
}

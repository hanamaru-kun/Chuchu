using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Sound : MonoBehaviour
{
    public static Sound instance = null;

    private AudioSource BgmPlayer;
    private AudioSource[] SePlayer;

    public AudioClip[] BgmClips;
    public AudioClip[] SeClips;

    private float[] BGM_VOLUME = new float[] {
        0.3f,
        0.5f,
    };
    public enum BGM {
        THEME,
        CLEAR,
    }
    private float[] SE_VOLUME = new float[] {
        0.5f,
        0.5f,
        0.5f,
        1.0f,
        1.0f,
    };
    public enum SE {
        BUTTON,
        CRUSH,
        DROP,
        ITEM,
        STEP,
    }

    [RuntimeInitializeOnLoadMethod]
    static void Startup() {
        var tmp = Resources.Load<GameObject>("Sound");
        Instantiate(tmp);
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(this);

        BgmPlayer = gameObject.AddComponent<AudioSource>();

        int CHANNELS = 2;
        SePlayer = new AudioSource[CHANNELS];
        for (int i = 0; i < CHANNELS; i++) {
            SePlayer[i] = gameObject.AddComponent<AudioSource>();
        }
        foreach (var c in BgmClips) {
            c.LoadAudioData();
        }
        foreach (var c in SeClips) {
            c.LoadAudioData();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 曲の再生
    /// </summary>
    /// <param name="id"></param>
    /// <param name="loop"></param>
    public static void PlayBgm(BGM id, bool loop = false) {
        instance.PlayBgm((int)id, loop);
    }
    private void PlayBgm(int id, bool loop) {
        StopCoroutine(FadeOut(BgmPlayer));
        Play(BgmPlayer, BgmClips[id], BGM_VOLUME[id], loop, false);
    }
    /// <summary>
    /// 曲の停止
    /// </summary>
    public static void StopBgm() {
        //instance.BgmPlayer.Stop();
        instance.StartCoroutine(instance.FadeOut(instance.BgmPlayer));
    }
    /// <summary>
    /// 曲の演奏中か
    /// </summary>
    /// <returns></returns>
    public static bool IsPlayingBgm() {
        return instance.BgmPlayer.isPlaying;
    }
    /// <summary>
    /// 効果音の再生
    /// </summary>
    /// <param name="id"></param>
    /// <param name="loop"></param>
    public static void PlaySE(SE id, int channel = 0, bool loop = false) {
        instance.PlaySE((int)id, channel, loop);
    }
    private void PlaySE(int id, int channel, bool loop) {
        StopCoroutine(FadeOut(SePlayer[channel]));
        Play(SePlayer[channel], SeClips[id], SE_VOLUME[id], loop, true);
    }
    private void Play(AudioSource source, AudioClip clip, float volume, bool loop, bool shot) {
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        if (shot) {
            source.PlayOneShot(clip);
        } else {
            source.Play();
        }
    }
    /// <summary>
    /// 効果音の停止
    /// </summary>
    /// <param name="channel"></param>
    public static void StopSE(int channel = 0) {
        //instance.SePlayer[channel].Stop();
        instance.StartCoroutine(instance.FadeOut(instance.SePlayer[channel]));
    }
    private IEnumerator FadeOut(AudioSource source) {
        if (!source.isPlaying) {
            yield break;
        }
        float volume = source.volume;
        while (volume > 0) {
            source.volume = volume;
            volume -= 0.01f;
            yield return null;
        }
        source.Stop();
    }
}

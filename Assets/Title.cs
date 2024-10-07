using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Title : MonoBehaviour
{
    public Image mask;
    bool starting = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!starting && Input.anyKeyDown) {
                starting = true;
                var cg = GetComponent<CanvasGroup>();
                Sound.PlaySE(Sound.SE.BUTTON);
                DOTween.To(() => mask.color.a, alpha => mask.color = new Color(0,0,0,alpha), 1f, 0.5f).OnComplete(() => {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("GameMain");
                });

        }
    }
}

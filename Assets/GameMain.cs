using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameMain : MonoBehaviour {
    public GameObject tmpBlock;
    public GameObject tmpWall;
    public GameObject tmpFood1;
    public GameObject tmpFood2;
    public GameObject MazeRoot;
    public Animator Anim;

    public Text time;
    public Text food;
    public GameObject goal;

    public const int MazeSize = 25;

    public static float timer = 0f;
    private int foodCount = 0;

    private enum Type : byte {
        space,
        block,
        food1,
        food2,
    }
    private Type[,] maze = new Type[MazeSize, MazeSize];
    private List<Vector3> DIRC = new List<Vector3>() { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    private Dictionary<(int,int),GameObject> Foods = new Dictionary<(int, int), GameObject>();
    private bool gameover = false;

    // Start is called before the first frame update
    void Start() {
        for (int y = 0; y < MazeSize; y++) {
            for (int x = 0; x < MazeSize; x++) {
                if (x == 0 || y == 0 || x == MazeSize - 1 || y == MazeSize - 1) {
                    maze[x, y] = Type.block;
                } else {
                    maze[x, y] = Type.space;
                }
            }
        }
        maze[(MazeSize + 1) / 2, MazeSize - 1] = Type.space;

        for (int y = 2; y < MazeSize - 2; y += 2) {
            for (int x = 2; x < MazeSize - 2; x += 2) {
                if (maze[x, y] == Type.block)
                    continue;
                List<(int, int)> MyBlocks = new List<(int, int)>();
                maze[x, y] = Type.block;
                MyBlocks.Add((x, y));
                Vector3 p = new Vector3(x, 0f, y);

                for (; ; ) {
                    DIRC = DIRC.OrderBy(a => Guid.NewGuid()).ToList();
                    int t = 0;
                    for (; t < 4; t++) {
                        var p1 = p + DIRC[t];
                        if (p1.x < 0f || p1.z < 0f || p1.x >= MazeSize || p1.z >= MazeSize)
                            continue;
                        var p2 = p1 + DIRC[t];
                        if (maze[(int)p2.x, (int)p2.z] == Type.space) {
                            // 行く先が空白だった
                            maze[(int)p2.x, (int)p2.z] = Type.block;
                            maze[(int)p1.x, (int)p1.z] = Type.block;
                            MyBlocks.Add(((int)p2.x, (int)p2.y));
                            p = p2;
                            break;
                        }
                        // 行く先が壁だった


                        //if ((int)p2.x == 0 || (int)p2.z == 0 || (int)p2.x == MazeSize - 1 || (int)p2.z == MazeSize - 1) {
                        //    // 壁に接続
                        //    maze[(int)p1.x, (int)p1.z] = Type.block;
                        //    t = 4;  // 探索終了
                        //    break;
                        //}
                        if (!MyBlocks.Contains(((int)p2.x, (int)p2.y))) {
                            // 他の壁に衝突
                            maze[(int)p1.x, (int)p1.z] = Type.block;
                            t = 4;  // 探索終了
                            break;
                        }


                        // 方向転換
                    }
                    if (t >= 4)
                        break;  // 探索終了
                }
            }
        }
        int mh = (MazeSize - 1) / 2;
        PutObj(mh, mh, tmpFood1, Type.food1);
        PutObj(mh + 1, mh, tmpFood2, Type.food2);
        PutObj(mh, mh + 1, tmpFood2, Type.food2);
        PutObj(mh + 1, mh + 1, tmpFood1, Type.food1);
        for (int i = 0; i < 3; i++) {
            for (; ; ) {
                int x = (int)(UnityEngine.Random.value * mh) * 2 + 1;
                int y = (int)(UnityEngine.Random.value * mh) * 2 + 1;
                if (maze[x, y] != Type.space)
                    continue;
                PutObj(x, y, tmpFood1, Type.food2);
                break;
            }
        }

        for (int y = 0; y < MazeSize; y++) {
            for (int x = 0; x < MazeSize; x++) {
                GameObject tmp = null;
                if (maze[x, y] != Type.block)
                    continue;
                if (x == 0 || y == 0 || x == MazeSize - 1 || y == MazeSize - 1) {
                    tmp = tmpWall;
                } else {
                    tmp = tmpBlock;
                }
                PutObj(x, y, tmp);
            }
        }


        //for (int y = 2; y < MazeSize - 2; y += 2) {
        //    for (int x = 2; x < MazeSize - 2; x += 2) {
        //
        //    }
        //}
        int cx = (MazeSize - 1) / 2;
        int cy = 1;
        if (maze[cx, cy] != Type.space)
            cx++;
        transform.position = MtoW(cx, cy);
        SetCamera(0f);

        timer = 0f;
        Sound.PlayBgm(Sound.BGM.THEME, true);
    }

    private void PutObj(int x, int y, GameObject tmp, Type t) {
        maze[x, y] = t;
        var obj = PutObj(x, y, tmp);
        //if (t == Type.food1 || t == Type.food2) {
        //    Foods.Add((x, y), obj);
        //}
    }
    private GameObject PutObj(int x, int y, GameObject tmp) {
        var obj = Instantiate<GameObject>(tmp);
        obj.transform.SetParent(MazeRoot.transform, false);
        obj.transform.position = MtoW(x, y);
        Foods.Add((x, y), obj);
        return obj;
    }

    // 迷路上の位置をワールド座標に変換
    private Vector3 MtoW(int x, int y) {
        return new Vector3(x - (MazeSize + 1) / 2, 0.5f, y - (MazeSize + 1) / 2);
    }
    private (int x,int y) WtoM(Vector3 p) {
        float x = p.x + (MazeSize + 1) / 2 + 0.5f;
        float y = p.z + (MazeSize + 1) / 2 + 0.5f;
        return ((int)x, (int)y);
    }

    private bool busy=false;
    private float run = 0f;
    // Update is called once per frame
    void Update() {
        if (gameover)
            return;

        timer += Time.deltaTime;
        time.text = ((int)timer).ToString();

        if (run > 0f) {
            run -= Time.deltaTime;
            if (run <= 0) {
                Anim.SetBool("Run", false);
            }
        }

        //transform.rotation = Camera.main.transform.rotation;
        if (busy)
            return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            busy = true;
            float toy = Camera.main.transform.eulerAngles.y - 90f;
            DOTween.To(
               SetCamera,
                Camera.main.transform.eulerAngles.y,
                toy,
                0.3f).onComplete = () => busy = false;
            transform.DORotate(new Vector3(0f, toy, 0f), 0.3f);

        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            busy = true;
            float toy = Camera.main.transform.eulerAngles.y + 90f;
            DOTween.To(
               SetCamera,
                Camera.main.transform.eulerAngles.y,
                toy,
                0.3f).onComplete = () => busy = false;
            transform.DORotate(new Vector3(0f, toy, 0f), 0.3f);

        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            transform.DORotate(Camera.main.transform.eulerAngles, 0.3f);
            Move(1);
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            transform.DORotate(Camera.main.transform.eulerAngles + new Vector3(0f, 180f, 0f), 0.3f);
            Move(-1);
        }
    }

    private void Move(float dirc) {
        float a = Camera.main.transform.eulerAngles.y;
        Vector3 v = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad), 0f, Mathf.Cos(a * Mathf.Deg2Rad));
        Vector3 np = transform.position + v * dirc;
        (int x, int y) = WtoM(np);
        if (y >= MazeSize) {
            gameover = true;
            Anim.Play("Salute");
            Sound.PlayBgm(Sound.BGM.CLEAR);
            transform.DORotate(new Vector3(0f, 180f, 0f), 0.3f).onComplete = ()=> goal.SetActive(true);
            Debug.Log("EXIT");
            StartCoroutine(ReturnToTitle());
            return;
        }
        var floor = maze[x, y];
        if (floor == Type.block) {
            if (x == 0||y == 0 || x >=MazeSize-1||y >= MazeSize-1) {
                Sound.PlaySE(Sound.SE.CRUSH);
                Anim.Play("GoDown");
            } else if (foodCount > 0) {
                foodCount--;
                food.text = foodCount.ToString();
                var obj = Foods[(x, y)];
                obj.transform.DOMove(obj.transform.position + new Vector3(0, -1, 0), 0.5f).onComplete = () =>
                     Destroy(obj);
                maze[x, y] = Type.space;
                Sound.PlaySE(Sound.SE.DROP);
            } else {
                Sound.PlaySE(Sound.SE.CRUSH);
                Anim.Play("GoDown");
            }
        } else if (floor == Type.food1 || floor == Type.food2) {
            Destroy(Foods[(x, y)]);
            maze[x, y] = Type.space;
            foodCount++;
            food.text = foodCount.ToString();
            Sound.PlaySE(Sound.SE.ITEM);
        } else {
            busy = true;
            Anim.SetBool("Run",true);
            run = 1f;
            Sound.PlaySE(Sound.SE.STEP);
            var tw = transform.DOMove(np, 0.3f);
            tw.SetEase(Ease.Linear);
            tw.onUpdate = () => { SetCamera(a); };
            tw.onComplete = () => {
                busy = false;
            };
        }
    }
    private void SetCamera(float a) {
        Camera.main.transform.eulerAngles = new Vector3(0f, a, 0f);
        Vector3 p = transform.position;
        p.x += -Mathf.Sin(a * Mathf.Deg2Rad) * 0.5f;
        p.z += -Mathf.Cos(a * Mathf.Deg2Rad) * 0.5f;
        Camera.main.transform.position = p;
    }

    private IEnumerator ReturnToTitle() {
        yield return new WaitForSeconds(4f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("title");
    }
}

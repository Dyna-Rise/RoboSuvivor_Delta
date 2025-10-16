
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    public Animator animator;

    public float moveSpeed = 5.0f; //移動スピード
    public float jumpForce = 8.0f; //ジャンプパワー
    public float gravity = 20.0f; //重力
    public float damageTimeIni = 2.0f; //ダメージ時間
    float damageTime = 2.0f;
    bool isDead; //死亡フラグ



    Vector3 moveDirection = Vector3.zero; //移動成分

    public GameObject body; //点滅対象
    bool isDamege = false; //ダメージフラグ

    AudioSource audioSource;

    public AudioClip se_Walk;
    public AudioClip se_Damage;
    public AudioClip se_Explosion;
    public AudioClip se_Jump;

    //足音判定
    float footstepInterval = 0.6f; //足音間隔
    float footstepTimer; //時間計測

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        //ゲームステータスがplaying or gameclear であれば動かす
        if (!(GameManager.gameState == GameState.playing || GameManager.gameState == GameState.gameclear))
            return;

        //if (!isDead)
        //{
        if (GameManager.playerHP <= 0) return;


        //地面にいる状態であれば動かす
        if (controller.isGrounded)
        {
            //上下キーが押されたら動かす
            if (Input.GetAxis("Vertical") != 0.0f)
            {

                if (Input.GetAxis("Vertical") > 0.0f)
                {
                    animator.SetInteger("direction", 0);
                }
                else
                {
                    animator.SetInteger("direction", 2);
                }
                animator.SetBool("walk", true); //走るフラグをOn
                Debug.Log("上下キー　");
                moveDirection.z = Input.GetAxis("Vertical") * moveSpeed;
            }
            else
            {
                moveDirection.z = 0.0f; //キーが押されていないなら動かさない
                animator.SetBool("walk", false); //走るフラグをOFF
            }

            //左右キーが押されたら動かす
            if (Input.GetAxis("Horizontal") != 0.0f)
            {
                if (Input.GetAxis("Horizontal") > 0.0f)
                {
                    animator.SetInteger("direction", 3);
                }
                else
                {
                    animator.SetInteger("direction", 1);
                }
                animator.SetBool("walk", true); //走るフラグをOn
                moveDirection.x = Input.GetAxis("Horizontal") * moveSpeed;
            }
            else
            {
                moveDirection.x = 0.0f; //キーが押されていないなら動かさない
                animator.SetBool("walk", false); //走るフラグをOFF
            }


            //マウスクリックでShootアニメ起動
            if (Input.GetMouseButton(0))
            {
                animator.SetTrigger("shot");  //ショットのアニメクリップの発動
            }


            if (Input.GetButtonDown("Jump"))
            {
                //ジャンプボタンが押されたら
                moveDirection.y = jumpForce;

                audioSource.PlayOneShot(se_Jump);

                // アニメ
                animator.SetTrigger("jump");  //ジャンプのアニメクリップの発動
            }

        }

        //常に重力をかける
        moveDirection.y -= gravity * Time.deltaTime;
        //体の向きに合わせて前後左右をGlobal座標系に変換
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);
        //接地したらyは0に
        if (controller.isGrounded) moveDirection.y = 0.0f;

        if (isDamege)
        {
            Blinking();
            damageTime = damageTime - Time.deltaTime;
            if (damageTime < 0)
            {
                isDamege = false;
                damageTime = damageTimeIni;
                body.SetActive(true); //ダメージ終了後確実に表示する
            }
        }

        //足音
        HandleFootsteps();

    }

    //何かに当たったら時の処理
    private void OnTriggerEnter(Collider hit)
    {

        //ダメージ中は何もしない
        if (isDamege) return;
        //Debug.Log("何かに当たった   "+ hit.gameObject.CompareTag("Enemy"));

        // Enemy か EnemyBullet に当たったら
        if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet") || hit.gameObject.CompareTag("Barrier"))
        {
            //GameManager の  public static int playerHP = 10; を減らす
            GameManager.playerHP--;
            isDamege = true;

            audioSource.PlayOneShot(se_Damage);

            if (GameManager.playerHP <= 0 && !isDead)
            {
                animator.SetTrigger("die");  //死亡のアニメクリップの発動

                audioSource.PlayOneShot(se_Explosion);

                //playerHPが0になったら gameover
                Debug.Log("死んだ");
                moveDirection.x = 0.0f;
                moveDirection.z = 0.0f;
                //isDead = true;

                //死亡コルーチン
                StartCoroutine(Dead());

            }

            isDamege = true;
        }

    }

    //死亡コルーチン
    IEnumerator Dead()
    {
        isDead = true;
        yield return new WaitForSeconds(3); //３秒待つ
        Destroy(gameObject);
        GameManager.gameState = GameState.gameover;
    }

    void Blinking()
    {
        float val = Mathf.Sin(Time.time * 50);
        //Debug.Log("点滅処理  " + val);
        if (val >= 0)
        {
            body.SetActive(true);
        }
        else
        {
            body.SetActive(false);
        }
    }

    //足音
    void HandleFootsteps()
    {
        //プレイヤーが動いていれば
        if (moveDirection.x != 0 || moveDirection.z != 0)
        {
            footstepTimer += Time.deltaTime; //時間計測

            if (footstepTimer >= footstepInterval) //インターバルチェック
            {
                audioSource.PlayOneShot(se_Walk);
                footstepTimer = 0;
            }
        }
        else //動いていなければ時間計測リセット
        {
            footstepTimer = 0f;
        }
    }

}

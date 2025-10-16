
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
    float deadTime = 10.0f;
    bool isDead;



    Vector3 moveDirection = Vector3.zero; //移動成分

    public GameObject body; //点滅対象
    bool isDamege = false; //ダメージフラグ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        //animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        //ゲームステータスがplaying or gameclear であれば動かす
        if (GameManager.gameState == GameState.playing || GameManager.gameState == GameState.gameclear)
        {
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
                    moveDirection.z = 0.0f;　//キーが押されていないなら動かさない
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
                    moveDirection.x = 0.0f;　//キーが押されていないなら動かさない
                    //animator.SetBool("walk", false); //走るフラグをOFF
                }


                //マウスクリックでShootアニメ起動
                if (Input.GetMouseButton(0))
                {
                    animator.SetTrigger("shot");  //ショットのアニメクリップの発動
                }

            }

            if (Input.GetButtonDown("Jump"))
            {
                //ジャンプボタンが押されたら
                moveDirection.y = jumpForce;
                // アニメ
                animator.SetTrigger("jump");  //ジャンプのアニメクリップの発動
            }

            //常に重力をかける
            moveDirection.y -= gravity * Time.deltaTime;
            //体の向きに合わせて前後左右をGlobal座標系に変換
            Vector3 globalDirection = transform.TransformDirection(moveDirection);
            controller.Move(globalDirection * Time.deltaTime);
            //接地したらyは0に
            if (controller.isGrounded) moveDirection.y = 0.0f;

        }


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

        if (isDead)
        {
            deadTime = deadTime - Time.deltaTime;
            //Debug.Log("死亡タイマー　　" +  deadTime);
            if (deadTime < 0)
            {

                GameManager.gameState = GameState.gameover;
                Destroy(gameObject, 3.0f); //自分自身の消失
            }
        }




    }

    //何かに当たったら時の処理
    private void OnTriggerEnter(Collider hit)
    {

        //ダメージ中は何もしない
        if (isDamege) return;
        //Debug.Log("何かに当たった   "+ hit.gameObject.CompareTag("Enemy"));

        // Enemy か EnemyBullet に当たったら
        if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet"))
        {
            //GameManager の  public static int playerHP = 10; を減らす
            GameManager.playerHP--;
            isDamege = true;

            if (GameManager.playerHP <= 0)
            {
                //playerHPが0になったら gameover
                Debug.Log("死んだ");
                moveDirection.x = 0.0f;
                moveDirection.z = 0.0f;
                isDead = true;
                animator.SetTrigger("die");  //死亡のアニメクリップの発動

            }

            isDamege = true;
        }

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

}

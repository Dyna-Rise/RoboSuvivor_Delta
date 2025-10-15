
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    Animator animator;

    public float moveSpeed = 5.0f; //移動スピード
    public float jumpForce = 8.0f; //ジャンプパワー
    public float gravity = 20.0f; //重力
    public float damageTimeIni = 2.0f; //ダメージ時間
    float damageTime = 2.0f;
    float x = 2.0f;


    Vector3 moveDirection = Vector3.zero; //移動成分

    public GameObject body; //点滅対象
    bool isDamege = false; //ダメージフラグ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

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
                    moveDirection.z = Input.GetAxis("Vertical") * moveSpeed;
                    animator.SetBool("walk", true); //走るフラグをOn
                }
                else
                {
                    moveDirection.z = 0.0f;　//キーが押されていないなら動かさない
                    animator.SetBool("walk", false); //走るフラグをOFF
                }

                //左右キーが押されたら動かす
                if (Input.GetAxis("Horizontal") != 0.0f)
                {
                    moveDirection.x = Input.GetAxis("Horizontal") * moveSpeed;
                }
                else
                {
                    moveDirection.x = 0.0f;　//キーが押されていないなら動かさない
                }


                if (Input.GetButton("Jump"))
                {
                    //ジャンプボタンが押されたら
                    moveDirection.y = jumpForce;
                    // アニメ
                    animator.SetTrigger("jump");  //ジャンプのアニメクリップの発動
                }
            }

            moveDirection.y -= gravity * Time.deltaTime;

            Vector3 globalDirection = transform.TransformDirection(moveDirection);
            controller.Move(globalDirection * Time.deltaTime);

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


        if (GameManager.gameState == GameState.gameover)
        {
            //GameOver();
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
                GameManager.gameState = GameState.gameover;
            }

            isDamege = true;
        }

    }

    void Animation()
    {
        //入力がある場合
        if (moveDirection.z != 0 || moveDirection.x != 0)
        {
            //ひとまずRunアニメを走らせる
            //animator.SetTrigger("walk");

            //angleZを使って方角を決める　direction int型
            //int型のdirection 下:0 上:1 右:2 左:それ以外

            //if (moveDirection.z > -135f && moveDirection.z < -45f)  //下
            //{
            //    animator.SetInteger("direction", 0);
            //}
            //else if (moveDirection.z >= -45f && moveDirection.z <= 45f)  //右
            //{
            //    //animator.SetInteger("direction", 2);
            //}
            //else if (moveDirection.z > 45f && moveDirection.z < 135f)  //上
            //{
            //    animator.SetInteger("direction", 1);
            //}
            //else                                    //左
            //{
            //    animator.SetInteger("direction", 3);
            //}
        }
        else //何も入力がない場合
        {
            //animator.SetBool("walk", false); //走るフラグをOFF
        }
    }

    void Blinking()
    {
        float val = Mathf.Sin(Time.time * 50);
        Debug.Log("点滅処理  " + val);
        if (val >= 0)
        {
            body.SetActive(true);
        }
        else
        {
            body.SetActive(false);
        }
    }


    void GameOver()
    {

        animator.SetTrigger("die");  //死亡のアニメクリップの発動
                                     //自分は消滅
        Destroy(gameObject, 3.0f);
    }
}

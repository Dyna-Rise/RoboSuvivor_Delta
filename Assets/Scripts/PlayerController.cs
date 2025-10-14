using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    Animator animator;

    public float moveSpeed = 5.0f; //移動スピード
    public float jumpForce = 8.0f; //ジャンプパワー
    public float gravity = 20.0f; //重力

    Vector3 moveDirection = Vector3.zero; //移動成分

    public GameObject body; //点滅対象
    bool isDamege; //ダメージフラグ

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
                }
                else
                {
                    moveDirection.z = 0.0f;　//キーが押されていないなら動かさない
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

                }
            }

            moveDirection.y -= gravity * Time.deltaTime;

            Vector3 globalDirection = transform.TransformDirection(moveDirection);
            controller.Move(globalDirection * Time.deltaTime);

            if (controller.isGrounded) moveDirection.y = 0.0f;

            if (isDamege)
            {
                moveDirection.z = 0.0f;
                moveDirection.x = 0.0f;

                Blinking();
            }
        }
    }

    void Blinking()
    {
        float val = Mathf.Sin(Time.deltaTime * 50);
        if (val >= 0) body.SetActive(true);
        else body.SetActive(false);
    }

    //何かに当たったら時の処理
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //ダメージ中は何もしない
        if (isDamege) return;

        // Enemy か EnemyBullet に当たったら
        if ( hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet") )
        {
            //GameManager の  public static int playerHP = 10; を減らす
            GameManager.playerHP -- ;

            if (GameManager.playerHP <= 0)
            {

            //playerHPが0になったら gameover
            GameManager.gameState = GameState.gameover;
            //自分は消滅
            Destroy(gameObject, 1.0f);

            }
        }
       
    }
}

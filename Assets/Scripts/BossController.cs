using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public int bossHP = 30;
    bool isDamage;
    public GameObject body;
    float timer;

    public float closeRange = 3f; // 近接攻撃をする距離
    public float attackInterval = 5f; // 攻撃のクールダウン

    public float moveSpeed = 5f; // タックルの移動速度

    public GameObject bulletPrefab; // 飛ばす球のプレハブ
    public GameObject gate; // 球を生成する位置
    public float bulletSpeed = 100f; // 球の速度

    public GameObject barrierPrefab; // バリアのオブジェクト

    GameObject player; // Player
    bool isAttacking; // 攻撃中かどうか

    public GameObject explosionPrefab; //爆発エフェクト

    AudioSource audioSource;
    public AudioClip se_Shot;
    public AudioClip se_Damage;
    public AudioClip se_Tackle;
    public AudioClip se_Barrier;
    public AudioClip se_Explosion;

    void Start()
    {
        // Playerオブジェクトを探し、Transformを取得
        player = GameObject.FindGameObjectWithTag("Player");

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //ゲーム状態がプレイ中でなければ何もしない
        if (GameManager.gameState != GameState.playing) return;
        if (bossHP <= 0) return;
        if (player == null) return;

        // ダメージ中の点滅処理
        if (isDamage)
        {
            Blinking();
        }

        if (isAttacking) return; // 攻撃中は処理をスキップ


        timer += Time.deltaTime; //ゲームの経過時間

        //プレイヤーとの距離を測る
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 攻撃のクールダウン
        if (timer >= attackInterval)
        {
            isAttacking = true; //攻撃中フラグをON
            if (distanceToPlayer <= closeRange)　// Playerが近い時
            {
                // バリアを展開
                StartCoroutine(ActivateBarrier());
            }
            else　// Playerが遠い時 (detectionRange内)
            {
                // タックルか球を飛ばすかをランダムで決定
                int randomAction = Random.Range(0, 2); // 0: タックル, 1: 球を飛ばす

                if (randomAction == 0)
                {
                    StartCoroutine(Tackle());　//タックル
                }
                else
                {
                    StartCoroutine(ShootProjectile()); //シュート
                }
            }
            timer = 0; //リセット
        }
    }

    //タックルコルーチン
    IEnumerator Tackle()
    {
        audioSource.PlayOneShot(se_Tackle);
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = player.transform.position;
        float startTime = Time.time;
        float duration = Vector3.Distance(startPosition, targetPosition) / moveSpeed;

        // Playerの方向を向く
        transform.LookAt(player.transform);

        while (Time.time < startTime + duration)
        {
            // Playerに向かって移動
            transform.position = Vector3.Lerp(startPosition, targetPosition, (Time.time - startTime) / duration);
            yield return null; // 1フレーム待つ
        }
        transform.position = targetPosition; // 最終的に目標位置に到達させる

        isAttacking = false; //攻撃フラグをOFF
    }

    //シュートコルーチン
    IEnumerator ShootProjectile()
    {
        audioSource.PlayOneShot(se_Shot);
        // Playerの方向を向く
        transform.LookAt(player.transform);

        yield return new WaitForSeconds(1.0f); // 球を飛ばすアニメーションや演出の時間

        //EnemyBulletの生成
        Quaternion bulletRotation = gate.transform.rotation * Quaternion.Euler(90, 0, 0); // 必要であればコメント解除
        GameObject bulletObj = Instantiate(
            bulletPrefab,
            gate.transform.position,
            bulletRotation
        );

        Rigidbody rbody = bulletObj.GetComponent<Rigidbody>();

        // Playerへの方向ベクトルを計算 (firePointからPlayerへ)
        Vector3 directionToPlayer = (player.transform.position - gate.transform.position).normalized;

        // 弾を発射する
        rbody.AddForce(directionToPlayer * bulletSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f); // 球を飛ばすアニメーションや演出の時間

        isAttacking = false; //攻撃フラグをOFF
    }

    //バリアの展開
    IEnumerator ActivateBarrier()
    {
        audioSource.PlayOneShot(se_Barrier);
        // バリアをその場に生成
        GameObject obj = Instantiate(
            barrierPrefab,
            transform.position + new Vector3(0, 5.0f, 0),
            Quaternion.identity
        );

        float durationTime = obj.GetComponent<Barrier>().deleteTime;

        yield return new WaitForSeconds(durationTime); // 指定時間待つ

        isAttacking = false; //攻撃フラグをOFF
    }

    //接触したら
    private void OnTriggerEnter(Collider other)
    {
        //プレイヤーの攻撃に当たったら
        if (other.gameObject.CompareTag("PlayerBullet") || other.gameObject.CompareTag("PlayerSword"))
        {
            //ダメージ中なら何もしない
            if (isDamage) return;


            audioSource.PlayOneShot(se_Damage);

            //接近攻撃なら
            if (other.gameObject.CompareTag("PlayerSword"))
            {
                bossHP -= 3; //敵のHPを2つマイナス
            }
            else
            {
                bossHP--; //敵のHPをマイナス
            }
            isDamage = true;//ダメージ中フラグ

            //HPがなくなったら削除
            if (bossHP <= 0)
            {
                audioSource.PlayOneShot(se_Explosion);

                //爆発エフェクトの生成
                GameObject obj = Instantiate(
                    explosionPrefab,
                    transform.position,
                    Quaternion.identity
                    );

                obj.transform.SetParent(transform);

                GameManager.gameState = GameState.gameclear;
                Destroy(gameObject, 3.0f); //ボスの消失
            }

            //ダメージリセット
            StartCoroutine(DamageReset());
        }
    }

    //ダメージリセットのコルーチン
    IEnumerator DamageReset()
    {
        yield return new WaitForSeconds(1.0f); // 点滅時間

        isDamage = false; //ダメージ中の解除
        body.SetActive(true); //明確に姿を表示
    }

    //点滅メソッド
    void Blinking()
    {
        float val = Mathf.Sin(Time.time * 50);
        if (val > 0) body.SetActive(true);
        else body.SetActive(false);
    }

    // ギズモで範囲を表示（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRange);
    }
}
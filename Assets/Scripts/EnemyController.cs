using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public int enemyHP = 5;//Enemyの体力
    public float enemySpeed = 5.0f;//スピード

    public GameObject Body; // 点滅させるメッシュを持つGameObject

    public float detectionRange = 80f;//索敵距離
    public float attackRange = 30f;//攻撃距離
    public float stopRange = 5f;//最小距離

    public GameObject bulletPrefab;//弾
    public GameObject gate; // 弾の発射口
    public float bulletSpeed = 100f;//弾のスピード
    public float fireInterval = 2.0f;//発射間隔

    //  内部で使用する変数 
    private bool isDamage;//ダメージ処理
    private GameObject player;//プレイヤー情報
    private NavMeshAgent navMeshAgent;//プレイヤーに近づく変数
    private bool isAttack;//攻撃フラグ
    private bool lockOn = true; // プレイヤーの方向を向くかどうか
    private float timer;

    // ★ GameManagerのインスタンスを保持する変数は引き続き必要
    private GameManager gameMgr;

    private AudioSource audioSource;
    //private Animator animator;
    public Animator animator;
    private bool isDead = false;


    public AudioClip seShot;
    public AudioClip seDamage;
    public AudioClip seExplosion;

    public GameObject flamePrefab;


    void Start()
    {
        Time.timeScale = 1f;
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();

        // ★ GameManagerのインスタンスを取得して、リスト操作に備える
        // FindObjectOfTypeはシーンから指定した型のコンポーネントを一つ見つける
        gameMgr = FindAnyObjectByType<GameManager>();

        audioSource = GetComponent<AudioSource>();
        //animator = GetComponent<Animator>();

        navMeshAgent.speed = enemySpeed;
        navMeshAgent.stoppingDistance = stopRange;


    }

    void Update()
    {
        // ★ 変更：死亡していたら以降の処理を一切行わない
        if (GameManager.gameState != GameState.playing || player == null || enemyHP <= 0 || isDamage || isDead)
        {
            return;
        }

        // プレイヤーとの距離を常に計測
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // --- 状況に応じた行動分岐 ---
        // プレイヤーが検知範囲内にいるか？
        if (distanceToPlayer <= detectionRange)
        {
            navMeshAgent.isStopped = false; // 追跡開始

            // lockOnが有効な場合、常にプレイヤーの方向を向く
            if (lockOn)
            {
                // Y軸を固定してプレイヤーの方向を向く
                Vector3 targetDirection = player.transform.position - transform.position;
                targetDirection.y = 0;
                transform.rotation = Quaternion.LookRotation(targetDirection);
            }

            // プレイヤーが攻撃範囲内にいるか？
            if (distanceToPlayer <= attackRange)
            {
                // 攻撃範囲内では移動速度を半分にする
                navMeshAgent.speed = enemySpeed * 0.5f;

                // 攻撃中でなければ、攻撃タイマーを進める
                if (!isAttack)
                {
                    timer += Time.deltaTime;
                    if (timer >= fireInterval)
                    {
                        StartCoroutine(AttackCoroutine());
                    }
                }
            }
            else // 攻撃範囲外だが、検知範囲内の場合
            {
                // 通常のスピードでプレイヤーを追跡
                navMeshAgent.speed = enemySpeed;
                navMeshAgent.SetDestination(player.transform.position);
            }
        }
        else // プレイヤーが検知範囲外にいる場合
        {
            // NavMeshAgentの動きを止める
            navMeshAgent.isStopped = true;
        }
    }

    // 攻撃のシーケンスを管理するコルーチン
    IEnumerator AttackCoroutine()
    {
        isAttack = true;  // 攻撃中フラグON
        lockOn = false;   // 攻撃モーション中は向きを固定するためlockOnをOF

        timer = 0f;       // タイマーリセット
        yield return new WaitForSeconds(1.0f);
        // 弾を生成し、前方に飛ばす
        if (bulletPrefab != null && gate != null)
        {
            // ショットSEを再生
            audioSource.PlayOneShot(seShot);

            // 1. 発射口の向きを取得
            Quaternion initialRotation = gate.transform.rotation;

            // 2. 90度回転させるための補正Quaternionを作成
            //    モデルが上を向いている場合、X軸で90度回転させる
            Quaternion rotationCorrection = Quaternion.Euler(90, 0, 0);

            // 3. 元の向きに補正をかけて、最終的な向きを計算
            Quaternion finalRotation = initialRotation * rotationCorrection;

            

            // 4. 計算した最終的な向きで弾を生成する
            GameObject bullet = Instantiate(bulletPrefab, gate.transform.position, finalRotation);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(gate.transform.forward * bulletSpeed, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.5f); // 攻撃後の硬直時間（調整可能）

        isAttack = false; // 攻撃中フラグOFF
        lockOn = true;    // 再びプレイヤーを追従するようにlockOnをON
    }

    // ダメージ判定
    private void OnTriggerEnter(Collider other)
    {
        // 死亡状態もチェック
        if (isDamage || enemyHP <= 0 || isDead) return;


        int damage = 0;
        if (other.gameObject.CompareTag("PlayerSword"))
        {
            damage = 3;
        }
        else if (other.gameObject.CompareTag("PlayerBullet"))
        {
            damage = 1;
            Destroy(other.gameObject); // 弾の場合は弾だけ消す
        }


        // ダメージが有効な場合
        if (damage > 0)
        {
            // ダメージSEを再生
            audioSource.PlayOneShot(seDamage);

            // ① すぐに無敵状態にして、連続ヒットを防ぐ
            isDamage = true;

            // ② ダメージを一度だけ与える
            enemyHP -= damage;

            // ③ HPの状態に関わらず、必ず点滅コルーチンを呼び出す
            StartCoroutine(DamageFlash());

        }
    }

    // ダメージ時の点滅エフェクト
    IEnumerator DamageFlash()
    {

        // 念のため、複数のRendererに対応できるようにしておく
        Renderer[] renderers = Body.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length > 0)
        {
            // 点滅処理
            for (int i = 0; i < 3; i++)
            {
                foreach (var r in renderers) r.enabled = false;
                yield return new WaitForSeconds(0.1f);
                foreach (var r in renderers) r.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (enemyHP <= 0)
        {
            // 死亡処理を開始
            isDead = true; // 死亡状態にする（重要）
            navMeshAgent.isStopped = true; // 移動を完全に停止

            // 死亡アニメーションを再生

            //animator.SetTrigger("Die");
            animator.SetTrigger("die");

            // 死亡SEを再生（オブジェクトが消えても音が鳴り続ける方法）
            AudioSource.PlayClipAtPoint(seExplosion, transform.position);

            // 死亡エフェクトを生成
            if (flamePrefab != null)
            {
                //Instantiate(flamePrefab, transform.position, Quaternion.identity);
                Instantiate(flamePrefab, transform.position, flamePrefab.transform.rotation);
            }

            // アニメーションが終わるまで待つ（時間はアニメの長さに合わせて調整）
            yield return new WaitForSeconds(1.5f);

            // GameManagerのリストから自身を削除
            if (gameMgr != null)
            {
                gameMgr.enemyList.Remove(this.gameObject);
            }

            //自身のGameObjectをシーンから削除
            Destroy(gameObject);
        }
        else
        {
            // HPが残っていれば、無敵状態を解除する
            isDamage = false;
        }
       
    }

    // ギズモで範囲を表示（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

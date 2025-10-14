using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public int enemyHP = 5;//Enemyの体力
    public float enemySpeed = 5.0f;//スピード

    public GameObject body; // 点滅させるメッシュを持つGameObject

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
    private NavMeshAgent navMeshAgent;//プレイヤーに近づくメソッド
    private bool isAttack;//攻撃フラグ
    private bool lockOn = true; // プレイヤーの方向を向くかどうか
    private float timer;

    // ★ GameManagerのインスタンスを保持する変数は引き続き必要
    private GameManager gameMgr;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();

        // ★ GameManagerのインスタンスを取得して、リスト操作に備える
        // FindObjectOfTypeはシーンから指定した型のコンポーネントを一つ見つける
        gameMgr = FindAnyObjectByType<GameManager>();

        navMeshAgent.speed = enemySpeed;
        navMeshAgent.stoppingDistance = stopRange;

        // GameManagerのインスタンスが見つかっていれば、そのenemyListに自身を追加
        if (gameMgr != null)
        {
            gameMgr.enemyList.Add(this.gameObject);
        }
        else
        {
            Debug.LogError("GameManagerがシーン内に見つかりません。");
        }
    }

    void Update()
    {
        // --- 実行条件のチェック ---
        // ★ GameManagerのstatic変数 gameState を直接参照してチェック
        if (GameManager.gameState != GameState.playing) return;

        if (player == null) return;
        if (enemyHP <= 0) return;
        if (isDamage) return;

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
        lockOn = false;   // 攻撃モーション中は向きを固定するためlockOnをOFF
        timer = 0f;       // タイマーリセット

        // 弾を生成し、前方に飛ばす
        if (bulletPrefab != null && gate != null)
        {
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
    private void OnCollisionEnter(Collision collision)
    {
        if (isDamage || enemyHP <= 0) return;

        int damage = 0;
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            damage = 1;
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("PlayerSword"))
        {
            damage = 3;
        }

        if (damage > 0)
        {
            enemyHP -= damage;
            if (enemyHP <= 0)
            {
                // --- 死亡処理 ---
                // GameManagerのリストから自身を削除
                if (gameMgr != null)
                {
                    gameMgr.enemyList.Remove(this.gameObject);
                }
                // 自身のGameObjectをシーンから削除
                Destroy(gameObject);
            }
            else
            {
                // ダメージエフェクトを開始
                StartCoroutine(DamageFlash());
            }
        }
    }

    // ダメージ時の点滅エフェクト
    IEnumerator DamageFlash()
    {
        isDamage = true;
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                bodyRenderer.enabled = false;
                yield return new WaitForSeconds(0.1f);
                bodyRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }
        isDamage = false;
    }
}

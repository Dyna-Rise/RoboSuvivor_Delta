using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public GameObject swordCollider; // 当たり判定
    public GameObject swordPrefab;   // 斬撃エフェクト
    public float deleteTime = 0.5f;  // 攻撃継続時間
    bool isAttack = false;

    public AudioClip se_Sword; // ソードSE
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 右クリックが押されたら攻撃
        if (Input.GetMouseButtonDown(1))
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        if (isAttack) return; // 攻撃中なら何もしない

        isAttack = true;

        // ソードSE
        audioSource.PlayOneShot(se_Sword);

        // ソードコライダーを有効化
        swordCollider.SetActive(true);

        // 斬撃エフェクトをプレイヤーの前方に生成
        Quaternion slashRotation = transform.rotation * Quaternion.Euler(0, 200, 5);

        // ← 右から左に薙ぎ払う向き
        GameObject slash = Instantiate(
            swordPrefab,
            transform.position + Vector3.up * 1.2f,
            slashRotation
        );

        // エフェクトがプレイヤーの動きに追従するように親子付け
        slash.transform.parent = transform;

        // 一定時間後に終了処理
        StartCoroutine(EndAttack(slash));
    }

    IEnumerator EndAttack(GameObject slash)
    {
        yield return new WaitForSeconds(deleteTime);

        // コライダーを無効化
        swordCollider.SetActive(false);

        // エフェクト削除
        if (slash != null)
            Destroy(slash);

        // 攻撃フラグ解除
        isAttack = false;
    }
}
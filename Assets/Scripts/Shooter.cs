using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject gate; // Gateオブジェクト
    public GameObject bulletPrefab; // バレットのプレハブ
    public float shotPower = 100f; // ショットパワー
    public float upSpeed = 0.2f;   // 上方向への補正値（高さ）
    public float shotRecoverTime = 3.0f; // 弾の回復時間
    bool isAttack; // 攻撃中フラグ

    public AudioClip se_Shot; // ショットSE
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // プレイ中でなければ処理しない
        if (GameManager.gameState != GameState.playing) return;

        // 左クリックが押されたらショット
        if (Input.GetMouseButtonDown(0))
        {
            Shot();
        }
    }

    void Shot()
    {
        if (isAttack) return; // 連射防止
        if (GameManager.shotRemainingNum <= 0) return; // 残弾なし

        isAttack = true;
        GameManager.shotRemainingNum--;

        // ショットSE
        audioSource.PlayOneShot(se_Shot);

        // Gateの回転にX軸90度だけ回転を加える
        Quaternion bulletRotation = gate.transform.rotation * Quaternion.Euler(90, 0, 0);

        // 弾を生成（Gateの位置から）
        GameObject obj = Instantiate(bulletPrefab, gate.transform.position, bulletRotation);

        // Rigidbody取得
        Rigidbody rbody = obj.GetComponent<Rigidbody>();

        // カメラの前方向を基準にして、Y方向を少し上げる（upSpeed）
        Vector3 shootDir = Camera.main.transform.forward;
        shootDir.y += upSpeed; // 上方向補正（shotPowerに依存しない）

        // 力を加える
        rbody.AddForce(shootDir.normalized * shotPower, ForceMode.Impulse);

        // 残弾回復コルーチン
        StartCoroutine(RecoverShot());

        // 攻撃フラグ解除
        Invoke(nameof(ShootEnabled), 0.2f);
    }

    //弾のリロードコルーチン
    IEnumerator RecoverShot()
    {
        yield return new WaitForSeconds(shotRecoverTime);
        GameManager.shotRemainingNum++;
    }

    void ShootEnabled()
    {
        isAttack = false;
    }
}
using System.Collections;using UnityEngine;public class Shooter : MonoBehaviour{
    public GameObject gate;         // Gateオブジェクトの情報
    public GameObject bulletPrefab; // PlayerBulletのプレハブ情報
    public float shootSpeed = 100f; // 飛ばす力（前方向）
    public int shotPower = 10;      // 残弾数
    public float recoverySeconds = 3.0f; // 1発回復にかかる秒数
    public float upSpeed = 2.0f; //Bulletの高さ

    Transform player;   // プレイヤーのTransform情報
    bool possibleShoot; // 発射可能フラグ
    Camera cam;         // カメラ情報

    void Start()    {
        // プレイヤー情報の取得
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 子オブジェクト "Gate" を取得
        gate = player.Find("Gate").gameObject;

        // メインカメラ取得
        cam = Camera.main;

        // 少し遅れて発射可能にする
        Invoke(nameof(ShootEnabled), 0.5f);    }

    void Update()    {
        // ゲームプレイ中のみ反応
        if (GameManager.gameState != GameState.playing) return;        if (Input.GetMouseButtonDown(0) && possibleShoot)        {            Shot();        }    }

    // 発射可能にする
    void ShootEnabled()    {        possibleShoot = true;    }

    // 発射処理
    void Shot()    {        if (player == null || shotPower <= 0) return;

        // Gateの位置・回転から弾を生成
        Quaternion bulletRot = gate.transform.rotation * Quaternion.Euler(90f, 0f, 0f);        GameObject obj = Instantiate(bulletPrefab, gate.transform.position, bulletRot);

        // Rigidbody取得して前方へ発射
        Rigidbody rbody = obj.GetComponent<Rigidbody>();        Vector3 shootDir = cam.transform.forward * shootSpeed;
        shootDir.y += upSpeed;

        // Rigidbodyで弾を飛ばす
        rbody.AddForce(shootDir, ForceMode.Impulse);

        // 弾を消費
        ConsumePower();    }

    // 弾の消費
    void ConsumePower()    {        shotPower--;        StartCoroutine(RecoverPower());    }

    // 弾リロードコルーチン
    IEnumerator RecoverPower()    {        yield return new WaitForSeconds(recoverySeconds);        shotPower++;    }}
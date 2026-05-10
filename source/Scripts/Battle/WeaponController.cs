using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =======================================================
// WeaponController
// -------------------------------------------------------
// 武器制御用の基底クラス
//
// 各武器ごとの発射処理・停止処理を共通インタフェースとして定義する
// =======================================================

abstract public class WeaponController : MonoBehaviour
{
    // 攻撃開始
    abstract public void fire(Vector3 aiming_to, GameObject target);

    // 攻撃停止
    abstract public void stop();
}
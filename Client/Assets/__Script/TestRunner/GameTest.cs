using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

public class GameTest
{
    /// <summary>
    /// 
    /// </summary>
    public struct DamageContext
    {
        public int damage;
        public int senderHeartPoint;
        public int senderCurrHeartPoint;
        public int receiverHeartPoint;
        public int receiverCurrHeartPoint;

        // ctor.
        public DamageContext(int damage, int senderHeartPoint, int senderCurrHeartPoint, int receiverHeartPoint, int receiverCurrHeartPoint)
        {
            this.damage = damage;
            this.senderHeartPoint = senderHeartPoint;
            this.senderCurrHeartPoint = senderCurrHeartPoint;
            this.receiverHeartPoint = receiverHeartPoint;
            this.receiverCurrHeartPoint = receiverCurrHeartPoint;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="heartPoint"></param>
    /// <returns></returns>
    int TrimHeartPoint(int heartPoint)
    {
        if (heartPoint < 10)
            return heartPoint;

        var quotient = heartPoint;
        var divisor = 1;

        while (true)
        {
            quotient /= 10;
            divisor *= 10;

            if (quotient < 10)
                break;
        }

        return quotient * divisor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="heartPoint"></param>
    /// <param name="significand"></param>
    /// <param name="exponent"></param>
    int TrimHeartPointEx(int heartPoint, out int significand, out int exponent)
    {
        if (heartPoint < 10)
        {
            significand = heartPoint;
            exponent = 0;

            return heartPoint;
        }

        var quotient = heartPoint;
        var divisor = 1;

        while (true)
        {
            quotient /= 10;
            divisor *= 10;

            if (quotient < 10)
                break;
        }

        significand = quotient;
        exponent = divisor;

        return quotient * divisor;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageContext"></param>
    /// <returns></returns>
    int CalcDamage(ref DamageContext damageContext)
    {
        return Mathf.Max(1, damageContext.senderHeartPoint - damageContext.receiverHeartPoint);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageContext"></param>
    /// <returns></returns>
    int CalcDropHeartPoint(DamageContext damageContext)
    {
        //* HeartPoint가 한자릿수인 경우
        if (damageContext.receiverHeartPoint < 10)
            return Mathf.Clamp(damageContext.damage, 1, damageContext.receiverHeartPoint);

        var significand = 0;
        var exponent = 0;
        var max = TrimHeartPointEx(damageContext.receiverHeartPoint, out significand, out exponent);
        var postDamaged = damageContext.receiverCurrHeartPoint - damageContext.damage;

        var quotient = (max - postDamaged) / exponent;

        if (quotient > 0)
            return quotient * exponent + (damageContext.receiverHeartPoint - max);
        else
            return 0;
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TestSimplePasses()
    {
        Debug.Log($"@@ drop-point => {CalcDropHeartPoint(new DamageContext(13, 1, 1, 31, 30))}");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}

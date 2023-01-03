/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/28/2022 9:34:57 AM
 * 
 * Description: Calculates values on a normal distribution.
*********************************/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomNormalDistribution : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// A testing list for showing the min and max of a generation.
    /// </summary>
    private List<float> normalDistribution = new List<float>();

    /// <summary>
    /// An amount for testing in a generation test.
    /// </summary>
    private const float testAmount = 100;
    #endregion

    #region Functions
    #region Test
    /// <summary>
    /// Tests the function if there is an instance in the scene.
    /// </summary>
    private void Awake()
    {
        TestNormalDistribution();
    }

    /// <summary>
    /// A test for the normal distribution.
    /// </summary>
    private void TestNormalDistribution()
    {
        for (int i = 0; i < testAmount; i++)
        {
            normalDistribution.Add(Generate());
        }

        print(normalDistribution.Min());
        print(normalDistribution.Max());
    }
    #endregion

    /// <summary>
    /// Generates the normal distribution value to be used.
    /// </summary>
    /// <param name="minValue">The min value that can be found.</param>
    /// <param name="maxValue">The max value that can be found.</param>
    /// <returns></returns>
    public static float Generate(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
    #endregion
}

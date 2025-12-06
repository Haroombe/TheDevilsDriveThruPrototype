using UnityEngine;
using System;

namespace Assets._scripts.OrderAlgorithm
{
    public class DeterministicOrderComplexity : MonoBehaviour
    {
        // --- TUNING COEFFICIENTS (FIXED on this component) ---
        [SerializeField] private float BaseOrderComplexity = 3f;      // V_base
        [SerializeField] private float AlphaLinearCoefficient = 5f;   // alpha
        [SerializeField] private float ExponentialDivider = 3f;       // gamma

        // Flag to control logging, useful for turning logs off in release builds
        [SerializeField] private bool EnableDebugging = true;


        /// <summary>
        /// Calculates the Deterministic Mean Volume (mu) for a given order number, 
        /// including debug logging of all intermediate values.
        /// </summary>
        /// <param name="orderNumberN">The sequential index of the order (n), starting at 1.</param>
        /// <param name="ordersPerShift">The current X_shift value provided by the GameManager.</param>
        /// <returns>The expected mean volume (mu) as a float.</returns>
        public float CalculateMeanVolume(int orderNumberN, int ordersPerShift)
        {
            if (ordersPerShift <= 0)
            {
                Debug.LogError("OrdersPerShift must be positive. Returning Base Complexity.");
                return BaseOrderComplexity;
            }

            // --- 1. DERIVE DEPENDENT VARIABLES (S and O) ---

            // Shift Number (S) = ceil(n / X_shift)
            int shiftNumberS = Mathf.CeilToInt((float)orderNumberN / ordersPerShift);

            // Local Order Index (O) = (n - 1) % X_shift + 1
            int localOrderO = ((orderNumberN - 1) % ordersPerShift) + 1;


            // --- 2. CALCULATE FORMULA COMPONENTS ---

            // A. Quadratic Term: ( O(n) / X_shift )^2
            float localRatio = (float)localOrderO / ordersPerShift;
            float quadraticTerm = localRatio * localRatio;

            // B. Linear Term: alpha * ( S(n) - 1 )
            float linearTerm = AlphaLinearCoefficient * (shiftNumberS - 1);

            // C. Exponential Multiplier: e^( S(n) / gamma )
            double exponentialMultiplier = Math.Exp((double)shiftNumberS / ExponentialDivider);


            // --- 3. FINAL MEAN VOLUME (MU) ---

            float complexityFactor = (quadraticTerm + linearTerm) * (float)exponentialMultiplier;
            float meanVolumeMu = BaseOrderComplexity + complexityFactor;


            // --- 4. DEBUG LOGGING ---
            if (EnableDebugging)
            {
                Debug.Log($"--- Order {orderNumberN} Complexity Analysis (X_shift: {ordersPerShift}) ---");
                Debug.Log($"* Derived Variables: Shift (S)={shiftNumberS}, Local Order (O)={localOrderO}");
                Debug.Log($"* Local Ratio (O/X): {localRatio:F4}");
                Debug.Log($"");
                Debug.Log($"** Component Breakdown **");
                Debug.Log($"* 1. Quadratic Term (Climax) = {quadraticTerm:F4}");
                Debug.Log($"* 2. Linear Term (Growth)   = {linearTerm:F4}");
                Debug.Log($"* 3. Exponential Multiplier = {exponentialMultiplier:F4}");
                Debug.Log($"* Complexity Factor: [ (1) + (2) ] * (3) = {complexityFactor:F4}");
                Debug.Log($"");
                Debug.Log($"** FINAL RESULT (Mu) **");
                Debug.Log($"* Base ({BaseOrderComplexity}) + Factor ({complexityFactor:F4}) = **{meanVolumeMu:F4}**");
                Debug.Log($"-------------------------------------------------------------------");
            }

            return meanVolumeMu;
        }
    }
}
using UnityEngine;

public static class ScoreSystem
{
    public static void AddXP(int amount)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddXP(amount);
        }
        else
        {
            Debug.LogWarning("[ScoreSystem] ScoreManager not found!");
        }
    }
    
    public static void AddKillXP()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddKillXP();
        }
        else
        {
            Debug.LogWarning("[ScoreSystem] ScoreManager not found!");
        }
    }
    
    public static void AddBonusXP()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddBonusXP();
        }
        else
        {
            Debug.LogWarning("[ScoreSystem] ScoreManager not found!");
        }
    }
}
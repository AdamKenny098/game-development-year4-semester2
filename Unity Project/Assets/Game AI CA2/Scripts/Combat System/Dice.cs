using UnityEngine;

public class Dice : MonoBehaviour
{
    public static Dice Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int RollD20()
    {
        return Random.Range(1, 21);
    }

    public  int Roll(string expr, Stats stats)
    {
        if (string.IsNullOrWhiteSpace(expr) || stats == null)
            return 0;

        string s = expr.Replace(" ", "").ToUpperInvariant();

        int total = 0;
        int sign = 1;

        for (int i = 0; i < s.Length;)
        {
            char c = s[i];

            if (c == '+') { sign = 1; i++; continue; }
            if (c == '-') { sign = -1; i++; continue; }

            int start = i;
            while (i < s.Length && s[i] != '+' && s[i] != '-')
                i++;

            total += sign * EvalToken(s.Substring(start, i - start), stats);
        }

        return total;
    }

    public int EvalToken(string token, Stats s)
    {
        if (string.IsNullOrEmpty(token))
            return 0;

        int d = token.IndexOf('D');
        if (d > 0)
        {
            int count, sides;
            if (!int.TryParse(token.Substring(0, d), out count)) return 0;
            if (!int.TryParse(token.Substring(d + 1), out sides)) return 0;

            int sum = 0;
            for (int n = 0; n < count; n++)
                sum += Random.Range(1, sides + 1);

            return sum;
        }

        if (token == "STR") return s.StrMod;
        if (token == "DEX") return s.DexMod;
        if (token == "INT") return s.IntMod;
        if (token == "CHA") return s.ChaMod;
        if (token == "PROF") return s.ProficiencyBonus;

        int flat;
        return int.TryParse(token, out flat) ? flat : 0;
    }
}

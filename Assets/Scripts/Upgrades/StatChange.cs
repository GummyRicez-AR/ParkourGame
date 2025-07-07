[System.Serializable]
public class StatChange
{
    public Stat stat;
    public float additiveChangeAmount;
    public float multiplicativeChangeAmount;

    public StatChange(StatChange old)
    {
        stat = old.stat;
        additiveChangeAmount = old.additiveChangeAmount;
        multiplicativeChangeAmount = old.multiplicativeChangeAmount;
    }

    public StatChange()
    {
        stat = Stat.None;
        additiveChangeAmount = 0;
        multiplicativeChangeAmount = 0;
    }
}
 
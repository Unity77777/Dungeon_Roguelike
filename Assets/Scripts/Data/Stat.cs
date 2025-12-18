[System.Serializable]
public class Stat
{
    public float baseValue;
    public float addValue;
    public float mulValue = 1f;

    public float Value
    {
        get
        {
            return (baseValue + addValue) * mulValue;
        }
    }
    public void ResetModifier()
    {
        addValue = 0f;
        mulValue = 1f;
    }
}

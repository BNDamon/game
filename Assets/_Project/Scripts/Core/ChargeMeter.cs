namespace BlockMerge.Core
{
    public sealed class ChargeMeter
    {
        public const int Max = 100;

        public int Value { get; private set; }
        public bool IsFull => Value >= Max;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Value = System.Math.Min(Max, Value + amount);
        }

        public void Reset() => Value = 0;
    }
}

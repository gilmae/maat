namespace StrangeVanilla.Maat
{
    public class Incrementor
    {
        int value = 0;
        public Incrementor(int? currentVersion)
        {
            if (currentVersion.HasValue)
            {
                value = currentVersion.Value + 1;
            }
        }

        public int Next()
        {
            return value++;
        }
    }
}

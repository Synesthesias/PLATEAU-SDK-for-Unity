namespace PLATEAU.Util
{
    internal static class DigitsUtil
    {
        /// <summary>
        /// <paramref name="num"/> のうち最初の <paramref name="numDigits"/> 桁を取り出します。
        /// </summary>
        public static int PickFirstDigits(int num, int numDigits)
        {
            int numDigitsToRemove = NumDigits(num) - numDigits;
            for (int i = 0; i < numDigitsToRemove; i++)
            {
                num /= 10;
            }

            return num;
        }

        /// <summary>
        /// <paramref name="num"/> の桁数を数えます。
        /// </summary>
        public static int NumDigits(int num)
        {
            if (num < 0) num *= -1;

            int digits = 0;
            while (num > 0)
            {
                num /= 10;
                digits++;
            }

            return digits;
        }
    }
}
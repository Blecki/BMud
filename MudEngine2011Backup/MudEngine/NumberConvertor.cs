using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public static class NumericSpelling
    {
        private const long Million = Thousand * 1000;
        private const long Thousand = Hundred * 10;
        private const long Hundred = 100;

        public static string ToVerbal(this int value) { return ToVerbal((long)value); }

        public static string ToVerbal(this long value)
        {
            if (value == 0) return "zero";

            if (value < 0)
            {
                return "negative " + ToVerbal(Math.Abs(value));
            }

            System.Text.StringBuilder builder = new StringBuilder();

            int unit = 0;

            if (value >= Million)
            {
                unit = (int)(value / Million);
                value -= unit * Million;
                builder.AppendFormat("{0}{1} million", builder.Length > 0 ? " " : "", ToVerbal(unit));
            }

            if (value >= Thousand)
            {
                unit = (int)(value / Thousand);
                value -= unit * Thousand;
                builder.AppendFormat("{0}{1} thousand", builder.Length > 0 ? " " : "", ToVerbal(unit));
            }

            if (value >= Hundred)
            {
                unit = (int)(value / Hundred);
                value -= unit * Hundred;
                builder.AppendFormat("{0}{1} hundred", builder.Length > 0 ? " " : "", ToVerbal(unit));
            }

            if (value >= 90)
            {
                value -= 90;
                builder.AppendFormat("{0}ninety", builder.Length > 0 ? " " : "");
            }

            if (value >= 80)
            {
                value -= 80;
                builder.AppendFormat("{0}eighty", builder.Length > 0 ? " " : "");
            }

            if (value >= 70)
            {
                value -= 70;
                builder.AppendFormat("{0}seventy", builder.Length > 0 ? " " : "");
            }

            if (value >= 60)
            {
                value -= 60;
                builder.AppendFormat("{0}sixty", builder.Length > 0 ? " " : "");
            }

            if (value >= 50)
            {
                value -= 50;
                builder.AppendFormat("{0}fifty", builder.Length > 0 ? " " : "");
            }

            if (value >= 40)
            {
                value -= 40;
                builder.AppendFormat("{0}forty", builder.Length > 0 ? " " : "");
            }

            if (value >= 30)
            {
                value -= 30;
                builder.AppendFormat("{0}thirty", builder.Length > 0 ? " " : "");
            }

            if (value >= 20)
            {
                value -= 20;
                builder.AppendFormat("{0}twenty", builder.Length > 0 ? " " : "");
            }

            if (value == 19) builder.AppendFormat("{0}nineteen", builder.Length > 0 ? " " : "");
            if (value == 18) builder.AppendFormat("{0}eighteen", builder.Length > 0 ? " " : "");
            if (value == 17) builder.AppendFormat("{0}seventeen", builder.Length > 0 ? " " : "");
            if (value == 16) builder.AppendFormat("{0}sixteen", builder.Length > 0 ? " " : "");
            if (value == 15) builder.AppendFormat("{0}fifteen", builder.Length > 0 ? " " : "");
            if (value == 14) builder.AppendFormat("{0}fourteen", builder.Length > 0 ? " " : "");
            if (value == 13) builder.AppendFormat("{0}thirteen", builder.Length > 0 ? " " : "");
            if (value == 12) builder.AppendFormat("{0}twelve", builder.Length > 0 ? " " : "");
            if (value == 11) builder.AppendFormat("{0}eleven", builder.Length > 0 ? " " : "");
            if (value == 10) builder.AppendFormat("{0}ten", builder.Length > 0 ? " " : "");
            if (value == 9) builder.AppendFormat("{0}nine", builder.Length > 0 ? " " : "");
            if (value == 8) builder.AppendFormat("{0}eight", builder.Length > 0 ? " " : "");
            if (value == 7) builder.AppendFormat("{0}seven", builder.Length > 0 ? " " : "");
            if (value == 6) builder.AppendFormat("{0}six", builder.Length > 0 ? " " : "");
            if (value == 5) builder.AppendFormat("{0}five", builder.Length > 0 ? " " : "");
            if (value == 4) builder.AppendFormat("{0}four", builder.Length > 0 ? " " : "");
            if (value == 3) builder.AppendFormat("{0}three", builder.Length > 0 ? " " : "");
            if (value == 2) builder.AppendFormat("{0}two", builder.Length > 0 ? " " : "");
            if (value == 1) builder.AppendFormat("{0}one", builder.Length > 0 ? " " : "");

            return builder.ToString();
        }
    }
}

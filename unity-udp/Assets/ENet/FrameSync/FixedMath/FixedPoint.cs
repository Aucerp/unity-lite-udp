namespace MENet.FrameSync
{
    public struct FixedPoint
    {
        public const long PRECISION = 100;
        public const int DECIMAL_PLACES = 2;
        private readonly long rawValue;

        // 添加公開的 Raw 屬性
        public long Raw { get { return rawValue; } }

        // 常用常量
        public static readonly FixedPoint Zero = new FixedPoint(0);
        public static readonly FixedPoint One = new FixedPoint(PRECISION);
        public static readonly FixedPoint Half = new FixedPoint(PRECISION / 2);

        // 構造函數
        internal FixedPoint(long rawValue)
        {
            this.rawValue = rawValue;
        }

        public static FixedPoint FromRaw(long raw)
        {
            return new FixedPoint(raw);
        }

        public static FixedPoint FromInt(int value)
        {
            return new FixedPoint(value * PRECISION);
        }

        // 基本運算
        public static FixedPoint operator +(FixedPoint a, FixedPoint b)
        {
            return new FixedPoint(a.rawValue + b.rawValue);
        }

        public static FixedPoint operator -(FixedPoint a, FixedPoint b)
        {
            return new FixedPoint(a.rawValue - b.rawValue);
        }

        public static FixedPoint operator *(FixedPoint a, FixedPoint b)
        {
            return new FixedPoint((a.rawValue * b.rawValue) / PRECISION);
        }

        public static FixedPoint operator /(FixedPoint a, FixedPoint b)
        {
            return new FixedPoint((a.rawValue * PRECISION) / b.rawValue);
        }

        // 比較運算符
        public static bool operator >(FixedPoint a, FixedPoint b)
        {
            return a.rawValue > b.rawValue;
        }

        public static bool operator <(FixedPoint a, FixedPoint b)
        {
            return a.rawValue < b.rawValue;
        }

        public static bool operator >=(FixedPoint a, FixedPoint b)
        {
            return a.rawValue >= b.rawValue;
        }

        public static bool operator <=(FixedPoint a, FixedPoint b)
        {
            return a.rawValue <= b.rawValue;
        }

        public static bool operator ==(FixedPoint a, FixedPoint b)
        {
            return a.rawValue == b.rawValue;
        }

        public static bool operator !=(FixedPoint a, FixedPoint b)
        {
            return a.rawValue != b.rawValue;
        }

        // 添加一元負運算符
        public static FixedPoint operator -(FixedPoint a)
        {
            return new FixedPoint(-a.rawValue);
        }

        // 添加一元正運算符（可選）
        public static FixedPoint operator +(FixedPoint a)
        {
            return a;
        }

        // 只在需要顯示或調試時使用 float
        public float ToFloat()
        {
            return (float)rawValue / PRECISION;
        }

        public override bool Equals(object obj)
        {
            if (obj is FixedPoint)
            {
                return this == (FixedPoint)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }

        public override string ToString()
        {
            return ToFloat().ToString("F2");
        }

        public static FixedPoint FromString(string value, long precision = PRECISION)
        {
            if (string.IsNullOrEmpty(value)) return Zero;
            
            float floatValue;
            if (float.TryParse(value, out floatValue))
            {
                return new FixedPoint((long)(floatValue * precision));
            }
            return Zero;
        }
    }
} 
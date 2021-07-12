using System;

namespace SharpChat {
    public readonly struct Colour : IEquatable<Colour?> {
        public const int INHERIT = 0x40000000;

        public int Raw { get; }

        public Colour(int argb) {
            Raw = argb;
        }

        public static implicit operator Colour(int argb) => new(argb);

        public bool Equals(Colour? other)
            => other.HasValue && other.Value.Raw == Raw;

        public bool Inherit => (Raw & INHERIT) > 0;
        public int Red => (Raw >> 16) & 0xFF;
        public int Green => (Raw >> 8) & 0xFF;
        public int Blue => Raw & 0xFF;

        public override string ToString() {
            if (Inherit)
                return @"inherit";
            return string.Format(@"#{0:X6}", Raw);
        }
    }
}

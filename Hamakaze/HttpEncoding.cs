using System;
using System.Globalization;
using System.Text;

namespace Hamakaze {
    public readonly struct HttpEncoding : IComparable<HttpEncoding?>, IEquatable<HttpEncoding?> {
        public const string DEFLATE = @"deflate";
        public const string GZIP = @"gzip";
        public const string XGZIP = @"x-gzip";
        public const string BROTLI = @"br";
        public const string IDENTITY = @"identity";
        public const string CHUNKED = @"chunked";
        public const string ANY = @"*";

        public static readonly HttpEncoding Any = new(ANY);
        public static readonly HttpEncoding None = new(ANY, 0f);
        public static readonly HttpEncoding Deflate = new(DEFLATE);
        public static readonly HttpEncoding GZip = new(GZIP);
        public static readonly HttpEncoding Brotli = new(BROTLI);
        public static readonly HttpEncoding Identity = new(IDENTITY);

        public string Name { get; }
        public float Quality { get; }

        public HttpEncoding(string name, float quality = 1f) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Quality = quality;
        }

        public HttpEncoding WithQuality(float quality) {
            return new HttpEncoding(Name, quality);
        }

        public static HttpEncoding Parse(string encoding) {
            string[] parts = encoding.Split(';', StringSplitOptions.TrimEntries);
            float quality = 1f;
            encoding = parts[0];

            for(int i = 1; i < parts.Length; ++i)
                if(parts[i].StartsWith(@"q=")) {
                    if(!float.TryParse(parts[i], out quality))
                        quality = 1f;
                    break;
                }

            return new HttpEncoding(encoding, quality);
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.Append(Name);
            if(Quality is >= 0f and < 1f)
                sb.AppendFormat(CultureInfo.InvariantCulture, @";q={0:0.0}", Quality);
            return sb.ToString();
        }

        public int CompareTo(HttpEncoding? other) {
            if(!other.HasValue || other.Value.Quality < Quality)
                return -1;
            if(other.Value.Quality > Quality)
                return 1;
            return 0;
        }

        public bool Equals(HttpEncoding? other) {
            return other.HasValue && Name.Equals(other.Value.Name) && Quality.Equals(other.Value.Quality);
        }
    }
}

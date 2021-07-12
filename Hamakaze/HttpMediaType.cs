using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hamakaze {
    public readonly struct HttpMediaType : IComparable<HttpMediaType?>, IEquatable<HttpMediaType?> {
        public const string TYPE_APPLICATION = @"application";
        public const string TYPE_AUDIO = @"audio";
        public const string TYPE_IMAGE = @"image";
        public const string TYPE_MESSAGE = @"message";
        public const string TYPE_MULTIPART = @"multipart";
        public const string TYPE_TEXT = @"text";
        public const string TYPE_VIDEO = @"video";

        public static readonly HttpMediaType OctetStream = new(TYPE_APPLICATION, @"octet-stream");
        public static readonly HttpMediaType FWIF = new(TYPE_APPLICATION, @"x.fwif");
        public static readonly HttpMediaType JSON = new(TYPE_APPLICATION, @"json");
        public static readonly HttpMediaType HTML = new(TYPE_TEXT, @"html", args: new[] { Param.UTF8 });

        public string Type { get; }
        public string Subtype { get; }
        public string Suffix { get; }
        public IEnumerable<Param> Params { get; }

        public HttpMediaType(string type, string subtype, string suffix = null, IEnumerable<Param> args = null) {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Subtype = subtype ?? throw new ArgumentNullException(nameof(subtype));
            Suffix = suffix ?? string.Empty;
            Params = args ?? Enumerable.Empty<Param>();
        }

        public string GetParamValue(string name) {
            foreach(Param param in Params)
                if(param.Name.ToLowerInvariant() == name)
                    return param.Value;
            return null;
        }

        public static explicit operator HttpMediaType(string mediaTypeString) => Parse(mediaTypeString);

        public static HttpMediaType Parse(string mediaTypeString) {
            if(mediaTypeString == null)
                throw new ArgumentNullException(nameof(mediaTypeString));

            int slashIndex = mediaTypeString.IndexOf('/');
            if(slashIndex == -1)
                return OctetStream;

            string type = mediaTypeString[..slashIndex];
            string subtype = mediaTypeString[(slashIndex + 1)..];
            string suffix = null;
            IEnumerable<Param> args = null;

            int paramIndex = subtype.IndexOf(';');
            if(paramIndex != -1) {
                args = subtype[(paramIndex + 1)..]
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(Param.Parse);
                subtype = subtype[..paramIndex];
            }

            int suffixIndex = subtype.IndexOf('+');
            if(suffixIndex != -1) {
                suffix = subtype[(suffixIndex + 1)..];
                subtype = subtype[..suffixIndex];
            }

            return new HttpMediaType(type, subtype, suffix, args);
        }

        public override string ToString() {
            StringBuilder sb = new();
            sb.AppendFormat(@"{0}/{1}", Type, Subtype);
            if(!string.IsNullOrWhiteSpace(Suffix))
                sb.AppendFormat(@"+{0}", Suffix);
            if(Params.Any())
                sb.AppendFormat(@";{0}", string.Join(';', Params));
            return sb.ToString();
        }

        public int CompareTo(HttpMediaType? other) {
            if(!other.HasValue)
                return -1;
            int type = Type.CompareTo(other.Value.Type);
            if(type != 0)
                return type;
            int subtype = Subtype.CompareTo(other.Value.Subtype);
            if(subtype != 0)
                return subtype;
            int suffix = Suffix.CompareTo(other.Value.Suffix);
            if(suffix != 0)
                return suffix;
            int paramCount = Params.Count();
            int args = paramCount - other.Value.Params.Count();
            if(args != 0)
                return args;
            for(int i = 0; i < paramCount; ++i) {
                args = Params.ElementAt(i).CompareTo(other.Value.Params.ElementAt(i));
                if(args != 0)
                    return args;
            }
            return 0;
        }

        public bool Equals(HttpMediaType? other) {
            if(!other.HasValue)
                return false;
            if(!Type.Equals(other.Value.Type) || !Subtype.Equals(other.Value.Subtype) || !Suffix.Equals(other.Value.Suffix))
                return false;
            int paramCount = Params.Count();
            if(paramCount != other.Value.Params.Count())
                return false;
            for(int i = 0; i < paramCount; ++i)
                if(!Params.ElementAt(i).Equals(other.Value.Params.ElementAt(i)))
                    return false;
            return true;
        }

        public readonly struct Param : IComparable<Param?>, IEquatable<Param?> {
            public const string CHARSET = @"charset";

            public static readonly Param ASCII = new(CHARSET, @"us-ascii");
            public static readonly Param UTF8 = new(CHARSET, @"utf-8");

            public string Name { get; }
            public string Value { get; }

            public Param(string name, string value) {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Value = value ?? throw new ArgumentNullException(nameof(name));
            }

            public override string ToString() {
                return string.Format(@"{0}={1}", Name, Value);
            }

            public static explicit operator Param(string paramStr) => Parse(paramStr);

            public static Param Parse(string paramStr) {
                string[] parts = (paramStr ?? throw new ArgumentNullException(nameof(paramStr))).Split('=', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return new Param(parts[0], parts[1]);
            }

            public int CompareTo(Param? other) {
                if(!other.HasValue)
                    return -1;
                int name = Name.CompareTo(other.Value.Name);
                return name != 0
                    ? name
                    : Value.CompareTo(other.Value.Value);
            }

            public bool Equals(Param? other) {
                return other.HasValue && Name.Equals(other.Value.Name) && Value.Equals(other.Value.Value);
            }
        }
    }
}

using SharpChat.Configuration;
using System;
using System.Data.SQLite;

namespace SharpChat.Database.SQLite {
    [DatabaseBackend(@"sqlite")]
    public class SQLiteDatabaseBackend : IDatabaseBackend {
        private string DSN { get; }

        private const string DEFAULT_PATH = @"sharpchat.db";

        public SQLiteDatabaseBackend(IConfig config) : this(
            config.ReadValue(@"path", DEFAULT_PATH)
        ) { }

        public SQLiteDatabaseBackend(string path = DEFAULT_PATH) {
            DSN = new SQLiteConnectionStringBuilder {
                DataSource = path,
                DateTimeFormat = SQLiteDateFormats.UnixEpoch,
                DateTimeKind = DateTimeKind.Utc,
                ForeignKeys = true,
                LegacyFormat = false,
                Pooling = true,
                Version = 3,
            }.ToString();
        }

        public IDatabaseConnection CreateConnection()
            => new SQLiteDatabaseConnection(DSN);

        public IDatabaseParameter CreateParameter(string name, object value)
            => new SQLiteDatabaseParameter(name, value);

        public IDatabaseParameter CreateParameter(string name, DatabaseType type)
            => new SQLiteDatabaseParameter(name, type);

        public string TimestampType
            => @"INTEGER";
        public string TextType
            => @"TEXT";
        public string BlobType
            => @"BLOB";
        public string VarCharType(int size)
            => @"TEXT";
        public string VarBinaryType(int size)
            => @"BLOB";
        public string BigIntType(int length)
            => @"INTEGER";
        public string BigUIntType(int length)
            => @"INTEGER";
        public string IntType(int length)
            => @"INTEGER";
        public string UIntType(int length)
            => @"INTEGER";
        public string TinyIntType(int length)
            => @"INTEGER";
        public string TinyUIntType(int length)
            => @"INTEGER";

        public string FromUnixTime(string param)
            => param;
        public string ToUnixTime(string param)
            => param;
        public string DateTimeNow()
            => @"strftime('%s', 'now')";

        public bool SupportsJson => false;
        public string JsonValue(string field, string path)
            => string.Empty;

        public string Concat(params string[] args)
            => string.Join(@" || ", args);
        public string ToLower(string param)
            => string.Format(@"LOWER({0})", param);

        public bool SupportsAlterTableCollate => false;

        public string AsciiCollation => @"NOCASE";
        public string UnicodeCollation => @"NOCASE";
    }
}

﻿using MySql.Data.MySqlClient;
using SharpChat.Configuration;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Database.MariaDB {
    [DatabaseBackend(@"mariadb")]
    public class MariaDBDatabaseBackend : IDatabaseBackend {
        private string DSN { get; }

        private const string DEFAULT_CHARSET = @"utf8mb4";

        public MariaDBDatabaseBackend(IConfig config) : this(
            config.ReadValue(@"host", string.Empty),
            config.ReadValue(@"user", string.Empty),
            config.ReadValue(@"pass", string.Empty),
            config.ReadValue(@"db", string.Empty),
            config.ReadValue(@"charset", DEFAULT_CHARSET)
        ) {}

        public MariaDBDatabaseBackend(string host, string username, string password, string database, string charset = DEFAULT_CHARSET) {
            DSN = new MySqlConnectionStringBuilder {
                Server = host,
                UserID = username,
                Password = password,
                Database = database,
                IgnorePrepare = false,
                OldGuids = false,
                TreatTinyAsBoolean = false,
                CharacterSet = charset,
                TreatBlobsAsUTF8 = false,
            }.ToString();
        }

        public IDatabaseConnection CreateConnection()
            => new MariaDBDatabaseConnection(DSN);

        public IDatabaseParameter CreateParameter(string name, object value)
            => new MariaDBDatabaseParameter(name, value);

        public IDatabaseParameter CreateParameter(string name, DatabaseType type)
            => new MariaDBDatabaseParameter(name, type);

        public string TimestampType
            => @"TIMESTAMP";
        public string TextType
            => @"TEXT";
        public string BlobType
            => @"BLOB";
        public string VarCharType(int size)
            => string.Format(@"VARCHAR({0})", size);
        public string VarBinaryType(int size)
            => string.Format(@"VARBINARY({0})", size);
        public string BigIntType(int length)
            => string.Format(@"BIGINT({0})", length);
        public string BigUIntType(int length)
            => string.Format(@"BIGINT({0}) UNSIGNED", length);
        public string IntType(int length)
            => string.Format(@"INT({0})", length);
        public string UIntType(int length)
            => string.Format(@"INT({0}) UNSIGNED", length);
        public string TinyIntType(int length)
            => string.Format(@"TINYINT({0})", length);
        public string TinyUIntType(int length)
            => string.Format(@"TINYINT({0}) UNSIGNED", length);

        public string FromUnixTime(string param)
            => string.Format(@"FROM_UNIXTIME({0})", param);
        public string ToUnixTime(string param)
            => string.Format(@"UNIX_TIMESTAMP({0})", param);
        public string DateTimeNow()
            => @"NOW()";

        public bool SupportsJson => true;
        public string JsonValue(string field, string path) // yes this is fucked, no i don't care
            => string.Format(@"JSON_UNQUOTE(JSON_EXTRACT({0}, '{1}'))", field, path);

        public string Concat(params string[] values)
            => string.Format(@"CONCAT({0})", string.Join(@", ", values));
        public string ToLower(string param)
            => string.Format(@"LOWER({0})", param);

        public bool SupportsAlterTableCollate => true;

        public string AsciiCollation => @"'ascii_general_ci'";
        public string UnicodeCollation => @"'utf8mb4_unicode_520_ci'";
    }
}

﻿using System;
using System.Linq;

namespace SharpChat.Database {
    public class DatabaseWrapper : IDisposable {
        private IDatabaseBackend Backend { get; }

        public bool IsNullBackend
            => Backend is Null.NullDatabaseBackend;

        public DatabaseWrapper(IDatabaseBackend backend) {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public IDatabaseParameter CreateParam(string name, object value)
            => Backend.CreateParameter(name, value);

        public void RunCommand(object query, Action<IDatabaseCommand> action, params IDatabaseParameter[] @params) {
            using IDatabaseConnection conn = Backend.CreateConnection();
            using IDatabaseCommand comm = conn.CreateCommand(query);
            if(@params.Any())
                comm.AddParameters(@params);
            action.Invoke(comm);
        }

        public int RunCommand(object query, params IDatabaseParameter[] @params) {
            int affected = 0;
            RunCommand(query, comm => affected = comm.Execute(), @params);
            return affected;
        }

        public object RunQueryValue(object query, params IDatabaseParameter[] @params) {
            object value = null;
            RunCommand(query, comm => value = comm.ExecuteScalar(), @params);
            return value;
        }

        public IDatabaseReader RunQuery(object query, params IDatabaseParameter[] @params) {
            IDatabaseConnection conn = Backend.CreateConnection();
            IDatabaseCommand comm = conn.CreateCommand(query);
            if(@params.Any())
                comm.AddParameters(@params);
            return comm.ExecuteReader();
        }

        public void RunQuery(object query, Action<IDatabaseReader> action, params IDatabaseParameter[] @params) {
            using IDatabaseConnection conn = Backend.CreateConnection();
            using IDatabaseCommand comm = conn.CreateCommand(query);
            if(@params.Any())
                comm.AddParameters(@params);
            using IDatabaseReader reader = comm.ExecuteReader();
            action.Invoke(reader);
        }

        private bool IsDisposed;
        ~DatabaseWrapper()
            => Dispose(false);
        public void Dispose()
            => Dispose(true);
        private void Dispose(bool disposing) {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Backend.Dispose();

            if(disposing)
                GC.SuppressFinalize(this);
        }
    }
}

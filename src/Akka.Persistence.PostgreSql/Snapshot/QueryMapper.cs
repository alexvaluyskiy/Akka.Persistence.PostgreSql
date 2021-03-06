﻿using System;
using System.Data.Common;
using Akka.Persistence.Sql.Common.Snapshot;

namespace Akka.Persistence.PostgreSql.Snapshot
{
    internal class PostgreSqlSnapshotQueryMapper : ISnapshotQueryMapper
    {
        private readonly Akka.Serialization.Serialization _serialization;

        public PostgreSqlSnapshotQueryMapper(Akka.Serialization.Serialization serialization)
        {
            _serialization = serialization;
        }

        public SelectedSnapshot Map(DbDataReader reader)
        {
            var persistenceId = reader.GetString(0);
            var sequenceNr = reader.GetInt64(1);

            var timestamp = new DateTime(reader.GetInt64(2));

            var metadata = new SnapshotMetadata(persistenceId, sequenceNr, timestamp);
            var snapshot = GetSnapshot(reader);

            return new SelectedSnapshot(metadata, snapshot);
        }

        private object GetSnapshot(DbDataReader reader)
        {
            var type = Type.GetType(reader.GetString(3), true);
            var serializer = _serialization.FindSerializerForType(type);
            var binary = (byte[])reader[4];

            var obj = serializer.FromBinary(binary, type);

            return obj;
        }
    }
}
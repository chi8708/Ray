﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ray.Core.Serialization;
using Ray.Core.Storage;
using Ray.Storage.SQLCore.Configuration;

namespace Ray.Storage.PostgreSQL
{
    public class StorageFactory : IStorageFactory
    {
        readonly IServiceProvider serviceProvider;
        readonly ISerializer serializer;
        public StorageFactory(
            IServiceProvider serviceProvider,
            ISerializer serializer)
        {
            this.serializer = serializer;
            this.serviceProvider = serviceProvider;
        }
        readonly ConcurrentDictionary<IStorageOptions, object> eventStorageDict = new ConcurrentDictionary<IStorageOptions, object>();
        public ValueTask<IEventStorage<PrimaryKey>> CreateEventStorage<PrimaryKey>(IStorageOptions config, PrimaryKey grainId)
        {
            if (config.Singleton)
            {
                var storage = eventStorageDict.GetOrAdd(config, key =>
                {
                    return new EventStorage<PrimaryKey>(serviceProvider, config as StorageOptions);
                });
                return new ValueTask<IEventStorage<PrimaryKey>>(storage as EventStorage<PrimaryKey>);
            }
            else
            {
                return new ValueTask<IEventStorage<PrimaryKey>>(new EventStorage<PrimaryKey>(serviceProvider, config as StorageOptions));
            }
        }
        readonly ConcurrentDictionary<IStorageOptions, object> stateStorageDict = new ConcurrentDictionary<IStorageOptions, object>();
        public ValueTask<ISnapshotStorage<PrimaryKey, State>> CreateSnapshotStorage<PrimaryKey, State>(IStorageOptions config, PrimaryKey grainId)
            where State : class, new()
        {
            if (config.Singleton)
            {
                var storage = stateStorageDict.GetOrAdd(config, key =>
                {
                    return new SnapshotStorage<PrimaryKey, State>(serializer, config as StorageOptions);
                });
                return new ValueTask<ISnapshotStorage<PrimaryKey, State>>(storage as SnapshotStorage<PrimaryKey, State>);
            }
            else
            {
                return new ValueTask<ISnapshotStorage<PrimaryKey, State>>(new SnapshotStorage<PrimaryKey, State>(serializer, config as StorageOptions));
            }
        }
        readonly ConcurrentDictionary<IStorageOptions, object> ArchiveStorageDict = new ConcurrentDictionary<IStorageOptions, object>();
        public ValueTask<IArchiveStorage<PrimaryKey, State>> CreateArchiveStorage<PrimaryKey, State>(IStorageOptions config, PrimaryKey grainId)
             where State : class, new()
        {
            if (config.Singleton)
            {
                var storage = ArchiveStorageDict.GetOrAdd(config, key =>
                {
                    return new ArchiveStorage<PrimaryKey, State>(serviceProvider, serializer, config as StorageOptions);
                });
                return new ValueTask<IArchiveStorage<PrimaryKey, State>>(storage as IArchiveStorage<PrimaryKey, State>);
            }
            else
            {
                return new ValueTask<IArchiveStorage<PrimaryKey, State>>(new ArchiveStorage<PrimaryKey, State>(serviceProvider, serializer, config as StorageOptions));
            }
        }
        readonly ConcurrentDictionary<IFollowStorageOptions, object> FollowSnapshotStorageDict = new ConcurrentDictionary<IFollowStorageOptions, object>();
        public ValueTask<IFollowSnapshotStorage<PrimaryKey>> CreateFollowSnapshotStorage<PrimaryKey>(IFollowStorageOptions config, PrimaryKey grainId)
        {
            if (config.Config.Singleton)
            {
                var storage = FollowSnapshotStorageDict.GetOrAdd(config, key =>
                {
                    return new FollowSnapshotStorage<PrimaryKey>(config as FollowStorageOptions);
                });
                return new ValueTask<IFollowSnapshotStorage<PrimaryKey>>(storage as IFollowSnapshotStorage<PrimaryKey>);
            }
            else
            {
                return new ValueTask<IFollowSnapshotStorage<PrimaryKey>>(new FollowSnapshotStorage<PrimaryKey>(config as FollowStorageOptions));
            }
        }
    }
}

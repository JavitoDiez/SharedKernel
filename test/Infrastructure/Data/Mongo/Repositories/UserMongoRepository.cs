﻿using MongoDB.Bson.Serialization;
using SharedKernel.Domain.Tests.Users;
using SharedKernel.Infrastructure.Mongo.Data.Repositories;

namespace SharedKernel.Integration.Tests.Data.Mongo.Repositories;

public class UserMongoRepository : MongoRepository<User, Guid>
{
    static UserMongoRepository()
    {
        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
            cm.MapField("_emails");
            cm.MapField("_addresses");
        });
    }

    public UserMongoRepository(SharedKernelMongoUnitOfWork mongoUnitOfWork) : base(mongoUnitOfWork)
    {
    }
}

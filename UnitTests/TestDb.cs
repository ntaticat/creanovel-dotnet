using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;

namespace UnitTests;

internal static class TestDb
{
    public static CreanovelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CreanovelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CreanovelDbContext(options);
    }
}

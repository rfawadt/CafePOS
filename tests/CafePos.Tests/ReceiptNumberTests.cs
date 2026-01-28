using CafePos.Domain.Entities;
using CafePos.Infrastructure.Persistence;
using CafePos.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePos.Tests;

public class ReceiptNumberTests
{
    [Fact]
    public async Task GeneratesSequentialReceiptNumbersPerDate()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<CafePosDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new CafePosDbContext(options);
        db.Database.EnsureCreated();

        var store = new Store { StoreId = Guid.NewGuid(), Name = "Test", IsActive = true };
        var terminal = new Terminal { TerminalId = Guid.NewGuid(), StoreId = store.StoreId, ReceiptPrefix = "T1", Name = "Term" };
        db.Stores.Add(store);
        db.Terminals.Add(terminal);
        db.SaveChanges();

        var service = new ReceiptNumberService(db);
        var date = new DateOnly(2026, 1, 28);
        var r1 = await service.NextReceiptNumberAsync(store.StoreId, terminal.TerminalId, date);
        var r2 = await service.NextReceiptNumberAsync(store.StoreId, terminal.TerminalId, date);

        Assert.Equal("T1-20260128-0001", r1);
        Assert.Equal("T1-20260128-0002", r2);

        var nextDate = date.AddDays(1);
        var r3 = await service.NextReceiptNumberAsync(store.StoreId, terminal.TerminalId, nextDate);
        Assert.Equal("T1-20260129-0001", r3);
    }
}

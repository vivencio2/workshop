﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;

class RemoveItemHandler : IHandleMessages<RemoveItem>
{
    public async Task Handle(RemoveItem message,
        IMessageHandlerContext context)
    {
        var dbContext = context.Extensions.Get<OrdersDataContext>();

        var order = await dbContext.Orders
            .FirstAsync(o => o.OrderId == message.OrderId);

        var lineToRemove = order.Lines
            .FirstOrDefault(x => x.Filling == message.Filling);

        if (lineToRemove != null)
        {
            order.Lines.Remove(lineToRemove);
            dbContext.OrderLines.Remove(lineToRemove);

            log.Info($"Item {message.Filling} removed.");

            dbContext.Publish(
                new ItemRemoved(message.OrderId, message.Filling));
        }
    }

    static readonly ILog log = LogManager.GetLogger<RemoveItemHandler>();
}
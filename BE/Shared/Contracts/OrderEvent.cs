namespace Shared.Contracts
{
    public static class RoutingKeys
    {
        public const string OrderCreated = "order.created";
        public const string OrderCancelled = "order.cancelled";
    }
    public record OrderCreatedEvent(string OrderId, string UserId, decimal Amount, DateTimeOffset CreatedAt);
    public record OrderCancelledEvent(string OrderId, string Reason, DateTimeOffset CancelledAt);
}

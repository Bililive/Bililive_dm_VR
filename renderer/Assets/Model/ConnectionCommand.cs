using BinarySerialization;

public class ConnectionCommand : Command
{
    [FieldOrder(0)]
    public bool Connect { get; set; }

    [FieldOrder(1)]
    public int RoomId { get; set; } = 0;
}

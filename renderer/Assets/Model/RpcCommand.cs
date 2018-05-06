using BinarySerialization;

public class RpcCommand
{
    [FieldOrder(0)]
    public CommandType CommandType { get; set; }

    [FieldOrder(1)]
    public int CommandSize { get; set; }

    [FieldOrder(2)]
    [FieldLength(nameof(CommandSize))]
    [Subtype(nameof(CommandType), CommandType.Connection, typeof(ConnectionCommand))]
    [Subtype(nameof(CommandType), CommandType.Profile, typeof(ProfileCommand))]
    public Command Command { get; set; }
}

public enum CommandType : int
{
    Default = 0,
    Connection = 1,
    Profile = 2,
}

public class Command { }

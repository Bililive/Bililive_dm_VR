using BinarySerialization;

public class ProfileCommand : Command
{
    [FieldOrder(0)]
    public Profile Profile { get; set; }
}

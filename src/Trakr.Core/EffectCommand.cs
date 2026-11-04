namespace Trakr.Core;

public sealed record EffectCommand
{
    public EffectCommand(string code, byte parameter)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Effect code cannot be empty.", nameof(code));
        }

        Code = code.Trim().ToUpperInvariant();
        Parameter = parameter;
    }

    public string Code { get; }

    public byte Parameter { get; }
}
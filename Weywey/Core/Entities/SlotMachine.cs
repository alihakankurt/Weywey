using System;

namespace Weywey.Core.Entities;

/// <summary>
/// Developed by Weylyn
/// </summary>
public class SlotMachine
{
    /// <summary>
    /// Raw value of the machine
    /// </summary>
    private int Value { get; set; }

    /// <summary>
    /// Symbols to show in state of slot machine
    /// </summary>
    private string[] Symbols { get; set; }


    /// <summary>
    /// Instansiate with default symbols
    /// </summary>
    public SlotMachine()
    {
        Value = 0;
        Symbols = new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };
    }

    /// <summary>
    /// Instansiate with custom symbols
    /// </summary>
    /// <param name="symbols"></param>
    public SlotMachine(params string[] symbols) : this()
    {
        if (symbols.Length != 8)
            throw new Exception("Symbols' lenght must be 8");

        Symbols = symbols;
    }

    /// <summary>
    /// Re-roll all the values of machine
    /// </summary>
    public void Slot()
    {
        Value = 0;
        var rd = new Random();
        Value += rd.Next(0, 8);
        Value += rd.Next(0, 8) << 3;
        Value += rd.Next(0, 8) << 6;
    }

    /// <summary>
    /// Returns the match count of slot machine
    /// </summary>
    public int MatchCount
    {
        get
        {
            int a = (Value >> 6) & 0b111;
            int b = (Value >> 3) & 0b111;
            int c = (Value) & 0b111;

            if (a == b && b == c)
                return 2;

            if (a == b || b == c || a == c)
                return 1;

            return 0;
        }
    }

    /// <summary>
    /// Returns the state of slot machine with symbols
    /// </summary>
    public string State
    {
        get
        {
            return $"[{Symbols[(Value >> 6) & 0b111]} {Symbols[(Value >> 3) & 0b111]} {Symbols[(Value) & 0b111]}]";
        }
    }
}

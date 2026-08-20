using System.Globalization;

namespace AoC.API;

/// <summary>Which half of a puzzle an answer belongs to.</summary>
public enum Part
{
    /// <summary>The first part.</summary>
    One = 1,

    /// <summary>The second part, unlocked by solving the first.</summary>
    Two = 2,
}

/// <summary>Reading and writing a <see cref="Part"/> the way the site does.</summary>
public static class Parts
{
    /// <summary>The part number as the Advent of Code API understands it.</summary>
    /// <param name="part">The part.</param>
    /// <returns><c>1</c> or <c>2</c>.</returns>
    /// <exception cref="PuzzleException"><paramref name="part"/> is neither.</exception>
    public static int Number(this Part part) => (int)Validated(part, nameof(part));

    /// <summary>The part's position among a puzzle's answers, counting from zero.</summary>
    /// <param name="part">The part.</param>
    /// <returns><c>0</c> or <c>1</c>.</returns>
    /// <exception cref="PuzzleException"><paramref name="part"/> is neither.</exception>
    public static int Index(this Part part) => part.Number() - 1;

    /// <summary>The part as the site names it, as in <c>part 1</c>.</summary>
    /// <param name="part">The part.</param>
    /// <returns>The part, written out.</returns>
    /// <exception cref="PuzzleException"><paramref name="part"/> is neither part.</exception>
    public static string Describe(this Part part) =>
        string.Create(CultureInfo.InvariantCulture, $"part {part.Number()}");

    /// <summary>Creates a part from the number the API uses.</summary>
    /// <param name="number">The part number.</param>
    /// <returns>The part.</returns>
    /// <exception cref="PuzzleException"><paramref name="number"/> is neither <c>1</c> nor <c>2</c>.</exception>
    public static Part FromNumber(int number) =>
        TryFromNumber(number, out var part) ? part : throw PuzzleException.ForPart(number, nameof(number));

    /// <summary>Creates a part, reporting rather than throwing when there is none.</summary>
    /// <param name="number">The part number.</param>
    /// <param name="part">The part, or the default when there is none.</param>
    /// <returns><see langword="true"/> if <paramref name="number"/> is <c>1</c> or <c>2</c>.</returns>
    public static bool TryFromNumber(int number, out Part part)
    {
        part = (Part)number;
        return part is Part.One or Part.Two;
    }

    /// <summary>
    /// The part itself, once it is known to be one. An enum can hold a value
    /// its type never declared, so this is where a cast one is caught - before
    /// it can be posted as a level the site has no idea what to do with.
    /// </summary>
    private static Part Validated(Part part, string paramName) =>
        part is Part.One or Part.Two ? part : throw PuzzleException.ForPart((int)part, paramName);
}

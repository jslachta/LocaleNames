namespace LocaleNames.Viewer
{
    /// <summary>
    /// A single entry of a resource file.
    /// </summary>
    /// <param name="Index">Position in the file (the order is part of the data).</param>
    /// <param name="Key">The code.</param>
    /// <param name="Value">The translation.</param>
    public sealed record LnbRow(int Index, string Key, string Value);
}

namespace AoC.API.Http;

/// <summary>The HTTP method of a <see cref="TransportRequest"/>.</summary>
public enum RequestMethod
{
    /// <summary>Reads a page.</summary>
    Get,

    /// <summary>Submits a form.</summary>
    Post,
}

/// <summary>
/// A request this library makes to Advent of Code.
/// </summary>
/// <remarks>
/// Plain data, with no <see cref="HttpClient"/> types in it, so a transport of
/// your own can serve one without going near the network.
/// </remarks>
public sealed record TransportRequest
{
    private TransportRequest(RequestMethod method, string url, IReadOnlyList<KeyValuePair<string, string>> form)
    {
        Method = method;
        Url = url;
        Form = form;
    }

    /// <summary>The method.</summary>
    public RequestMethod Method { get; }

    /// <summary>The absolute URL.</summary>
    public string Url { get; }

    /// <summary>Fields sent as <c>application/x-www-form-urlencoded</c>; empty for a GET.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Form { get; }

    /// <summary>A request that reads <paramref name="url"/>.</summary>
    /// <param name="url">The URL to read.</param>
    /// <returns>The request.</returns>
    public static TransportRequest Get(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return new TransportRequest(RequestMethod.Get, url, []);
    }

    /// <summary>A request that posts a form to <paramref name="url"/>.</summary>
    /// <param name="url">The URL to post to.</param>
    /// <param name="form">The fields to send.</param>
    /// <returns>The request.</returns>
    public static TransportRequest PostForm(string url, IReadOnlyList<KeyValuePair<string, string>> form)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(form);

        return new TransportRequest(RequestMethod.Post, url, form);
    }

    /// <summary>Whether this request is the same as <paramref name="other"/>, fields and all.</summary>
    /// <param name="other">The request to compare against.</param>
    /// <returns><see langword="true"/> if the two would send the same bytes.</returns>
    public bool Equals(TransportRequest? other) =>
        other is not null
        && Method == other.Method
        && string.Equals(Url, other.Url, StringComparison.Ordinal)
        && Form.SequenceEqual(other.Form);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Method);
        hash.Add(Url, StringComparer.Ordinal);

        foreach (var field in Form)
        {
            hash.Add(field.Key, StringComparer.Ordinal);
            hash.Add(field.Value, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}

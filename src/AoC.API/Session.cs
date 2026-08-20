using System.Globalization;
using AoC.API.Http;
using AoC.API.Parsing;

namespace AoC.API;

/// <summary>
/// An authenticated conversation with Advent of Code.
/// </summary>
/// <remarks>
/// A session holds the session cookie and the one HTTP client built from it.
/// Which puzzle a call is about is an argument, so one session serves a whole
/// event without rebuilding a client per call.
/// <para>
/// Nothing here is throttled or cached. A library cannot know how a program is
/// being driven, and a hidden delay inside someone else's process is a poor
/// surprise, so spacing out calls and keeping downloaded inputs on disk are
/// both the caller's job - see the automation guidelines in the readme.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var session = new Session("53616c7465645f5f...", "github.com/my-username/my-repo by me@example.com");
/// var puzzle = Puzzle.At(2024, 7);
///
/// var input = await session.GetInputTextAsync(puzzle);
/// var verdict = await session.SubmitAnswerAsync(puzzle, Part.One, "3749");
/// </code>
/// </example>
public sealed class Session : IDisposable
{
    private readonly bool _ownsTransport;

    /// <summary>Initializes a new instance of the <see cref="Session"/> class.</summary>
    /// <param name="cookie">
    /// The value of the <c>session</c> cookie on <c>adventofcode.com</c> while
    /// logged in, with or without a leading <c>session=</c>. It is a credential:
    /// treat it like a password.
    /// </param>
    /// <param name="identification">
    /// How every request identifies you, as the Advent of Code automation
    /// guidelines ask - <c>github.com/my-username/my-repo by me@example.com</c>
    /// or similar.
    /// </param>
    /// <exception cref="TransportException">Neither value can be sent in a header.</exception>
    public Session(string cookie, string identification) : this(new ClientOptions(cookie, identification)) { }

    /// <summary>Initializes a new instance of the <see cref="Session"/> class.</summary>
    /// <param name="options">The cookie, the identification and the timeout.</param>
    /// <exception cref="TransportException">The options cannot produce a client.</exception>
    public Session(ClientOptions options) : this(new HttpClientTransport(options), ownsTransport: true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Session"/> class on a
    /// transport of your own, usually a <see cref="FakeTransport"/> in a test.
    /// </summary>
    /// <remarks>The transport is not disposed with the session; you own it.</remarks>
    /// <param name="transport">The transport every request goes out through.</param>
    public Session(ITransport transport) : this(transport, ownsTransport: false) { }

    private Session(ITransport transport, bool ownsTransport)
    {
        ArgumentNullException.ThrowIfNull(transport);

        Transport = transport;
        _ownsTransport = ownsTransport;
    }

    /// <summary>The transport every request goes out through.</summary>
    public ITransport Transport { get; }

    /// <summary>Downloads a puzzle's personal input, without its trailing newline.</summary>
    /// <param name="puzzle">The puzzle to download.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The input.</returns>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string> GetInputTextAsync(Puzzle puzzle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        var body = await GetAsync(puzzle.InputUrl, puzzle, cancellationToken).ConfigureAwait(false);

        return body.TrimEnd('\n');
    }

    /// <summary>Downloads a puzzle's personal input as lines.</summary>
    /// <param name="puzzle">The puzzle to download.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The input, split on newlines.</returns>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string[]> GetInputLinesAsync(Puzzle puzzle, CancellationToken cancellationToken = default) =>
        Lines(await GetInputTextAsync(puzzle, cancellationToken).ConfigureAwait(false));

    /// <summary>Every sample block on a puzzle's page, in the order they appear.</summary>
    /// <param name="puzzle">The puzzle whose page to read.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The sample blocks, which may be none.</returns>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string[]> GetSamplesAsync(Puzzle puzzle, CancellationToken cancellationToken = default) =>
        Pages.Samples(await GetPageAsync(puzzle, cancellationToken).ConfigureAwait(false));

    /// <summary>The <paramref name="nth"/> sample block on a puzzle's page, counting from one.</summary>
    /// <param name="puzzle">The puzzle whose page to read.</param>
    /// <param name="nth">Which sample block, counting from one.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The sample.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Sample blocks count from one, so there is no sample 0.</exception>
    /// <exception cref="ParseException">The page has fewer sample blocks than that.</exception>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string> GetSampleTextAsync(
        Puzzle puzzle,
        int nth,
        CancellationToken cancellationToken = default)
    {
        // Sample blocks count from one, so no page can answer a zero, and
        // fetching one to find that out spends a request that could never have
        // succeeded.
        ArgumentOutOfRangeException.ThrowIfLessThan(nth, 1);

        return Pages.Sample(await GetPageAsync(puzzle, cancellationToken).ConfigureAwait(false), nth);
    }

    /// <summary>The <paramref name="nth"/> sample block on a puzzle's page, as lines.</summary>
    /// <param name="puzzle">The puzzle whose page to read.</param>
    /// <param name="nth">Which sample block, counting from one.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The sample, split on newlines.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Sample blocks count from one, so there is no sample 0.</exception>
    /// <exception cref="ParseException">The page has fewer sample blocks than that.</exception>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string[]> GetSampleLinesAsync(
        Puzzle puzzle,
        int nth,
        CancellationToken cancellationToken = default) =>
        Lines(await GetSampleTextAsync(puzzle, nth, cancellationToken).ConfigureAwait(false));

    /// <summary>How many stars this account has earned in each event.</summary>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The stars earned, by event, earliest first.</returns>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="ParseException">The page listed no events.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<IReadOnlyDictionary<Year, int>> GetStarsAsync(
        CancellationToken cancellationToken = default)
    {
        var body = await GetAsync($"{Puzzle.BaseUrl}/events", puzzle: null, cancellationToken).ConfigureAwait(false);

        return Pages.Stars(body);
    }

    /// <summary>The answer a puzzle's page shows as accepted for <paramref name="part"/>.</summary>
    /// <param name="puzzle">The puzzle whose page to read.</param>
    /// <param name="part">The part to look up.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The accepted answer.</returns>
    /// <exception cref="ParseException">That part is not solved yet.</exception>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<string> GetAcceptedAnswerAsync(
        Puzzle puzzle,
        Part part,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPageAsync(puzzle, cancellationToken).ConfigureAwait(false);

        return Pages.TryAcceptedAnswer(page, part, out var accepted)
            ? accepted
            : throw new ParseException($"the puzzle page does not show an accepted answer for {part.Describe()}");
    }

    /// <summary>Submits an answer and reports how the site judged it.</summary>
    /// <remarks>
    /// A rejected answer is a <see cref="Verdict"/>, not an exception. If the
    /// part turns out to be solved already the site refuses to judge anything,
    /// so the answer is compared against the accepted one on the puzzle page,
    /// which costs a second request. If that page shows no accepted answer for
    /// the part, the site was never asking for one: that is
    /// <see cref="Verdict.WrongLevel"/>, not a failure to read the page.
    /// </remarks>
    /// <param name="puzzle">The puzzle being answered.</param>
    /// <param name="part">The part being answered.</param>
    /// <param name="answer">The answer.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>How the site judged the answer.</returns>
    /// <exception cref="CooldownException">An answer was submitted too recently for this one to be judged.</exception>
    /// <exception cref="UnauthorizedException">The cookie was not accepted.</exception>
    /// <exception cref="PuzzleLockedException">The puzzle has not unlocked yet.</exception>
    /// <exception cref="ParseException">The reply was not one this library knows.</exception>
    /// <exception cref="TransportException">The request failed.</exception>
    public async Task<Verdict> SubmitAnswerAsync(
        Puzzle puzzle,
        Part part,
        string answer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(answer);

        var request = TransportRequest.PostForm(
            puzzle.AnswerUrl,
            [
                new KeyValuePair<string, string>("level", part.Number().ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("answer", answer),
            ]);

        var body = await SendAsync(request, puzzle, cancellationToken).ConfigureAwait(false);

        return Pages.ReadSubmission(body) switch
        {
            Submission.Correct => new Verdict.Correct(),
            Submission.Incorrect rejected => new Verdict.Incorrect(rejected.Hint, rejected.Wait),
            Submission.TooRecent cooling => throw new CooldownException(cooling.Wait),
            Submission.LoggedOut => throw new UnauthorizedException(),
            _ => await CompareWithAcceptedAsync(puzzle, part, answer, cancellationToken).ConfigureAwait(false),
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsTransport && Transport is IDisposable disposable) { disposable.Dispose(); }
    }

    /// <summary>
    /// What an answer to an already-solved part was worth, which only the puzzle
    /// page can say.
    /// </summary>
    private async Task<Verdict> CompareWithAcceptedAsync(
        Puzzle puzzle,
        Part part,
        string answer,
        CancellationToken cancellationToken)
    {
        var page = await GetPageAsync(puzzle, cancellationToken).ConfigureAwait(false);

        return Pages.TryAcceptedAnswer(page, part, out var accepted)
            ? new Verdict.AlreadyComplete(string.Equals(accepted.Trim(), answer.Trim(), StringComparison.Ordinal))
            : new Verdict.WrongLevel();
    }

    /// <summary>A puzzle's page.</summary>
    private Task<string> GetPageAsync(Puzzle puzzle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        return GetAsync(puzzle.Url, puzzle, cancellationToken);
    }

    /// <summary>Reads a URL.</summary>
    private Task<string> GetAsync(string url, Puzzle? puzzle, CancellationToken cancellationToken) =>
        SendAsync(TransportRequest.Get(url), puzzle, cancellationToken);

    /// <summary>
    /// Sends a request and returns the body, once the reply is known to be one
    /// worth reading.
    /// </summary>
    private async Task<string> SendAsync(
        TransportRequest request,
        Puzzle? puzzle,
        CancellationToken cancellationToken)
    {
        var response = await Transport.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        Check(response, puzzle);

        return response.Body;
    }

    /// <summary>Turns a reply that is not worth reading into the reason it is not.</summary>
    /// <remarks>
    /// The site distinguishes very little by status code, so the body has the
    /// last word - and it has it first, because a rejected cookie is not always
    /// a rejected request. The input endpoint refuses with a <c>400</c>, while
    /// the puzzle and events pages answer <c>200</c> with a perfectly ordinary
    /// page that happens to offer a log-in link. Both mean the same thing, and
    /// saying so beats failing later on a page that turned out to have no puzzle
    /// in it.
    /// </remarks>
    private static void Check(TransportResponse response, Puzzle? puzzle)
    {
        if (Pages.IsLoggedOut(response.Body)) { throw new UnauthorizedException(); }

        if (response.IsSuccess) { return; }

        throw response.StatusCode switch
        {
            401 or 403 => new UnauthorizedException(),
            404 when puzzle is not null => (AdventOfCodeException)new PuzzleLockedException(puzzle),
            var status => new UnexpectedStatusException(status),
        };
    }

    /// <summary>The lines of a body, which is what the <c>Lines</c> calls hand back.</summary>
    private static string[] Lines(string body) => [.. body.Split('\n').Select(line => line.TrimEnd('\r'))];
}

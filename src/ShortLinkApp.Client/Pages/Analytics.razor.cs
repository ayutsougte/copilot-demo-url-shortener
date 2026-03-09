using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ShortLinkApp.Client.Pages;

public partial class Analytics
{
    // ── Injected services ─────────────────────────────────────────────────────

    [Inject] private HttpClient Http { get; set; } = default!;

    // ── API DTOs ──────────────────────────────────────────────────────────────

    private record LinkResponse(
        int Id,
        string ShortCode,
        string OriginalUrl,
        string? CustomAlias,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        bool IsActive);

    private record DailyClickCount(DateOnly Date, int Count);

    private record AnalyticsResponse(
        int LinkId,
        int TotalClicks,
        IReadOnlyList<DailyClickCount> ClicksByDate);

    // ── Component state ───────────────────────────────────────────────────────

    private List<LinkResponse> _links = [];
    private bool _linksLoading = true;
    private string? _linksLoadError;

    private int? _selectedLinkId;
    private AnalyticsResponse? _analytics;
    private bool _analyticsLoading;
    private string? _analyticsError;

    private string _searchText = string.Empty;
    private bool _dropdownOpen;
    private List<LinkResponse> _filteredLinks = [];

    private const int MaxDropdownItems = 50;
    private const int MinLinksForCountBadge = 10;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await LoadLinksAsync();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    private async Task LoadLinksAsync()
    {
        _linksLoading = true;
        _linksLoadError = null;

        try
        {
            var links = await Http.GetFromJsonAsync<LinkResponse[]>("api/links");
            _links = (links ?? []).ToList();
            _filteredLinks = _links;
        }
        catch (HttpRequestException)
        {
            _linksLoadError = "Unable to reach the server. Please check your connection.";
        }
        catch (Exception)
        {
            _linksLoadError = "An unexpected error occurred while loading links.";
        }
        finally
        {
            _linksLoading = false;
        }
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _dropdownOpen = true;
        ApplyFilter();
        // _selectedLinkId and _analytics are intentionally kept so the analytics
        // panel stays visible while the user browses. They are replaced when a
        // new link is chosen via SelectLink, or cleared via ClearSelection.
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _filteredLinks = _links;
            return;
        }

        var q = _searchText.Trim();
        _filteredLinks = _links
            .Where(l =>
                l.ShortCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (l.CustomAlias?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                l.OriginalUrl.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void OpenDropdown()
    {
        _dropdownOpen = true;
        ApplyFilter();
    }

    private async Task CloseDropdown()
    {
        // Wait briefly so a click on a dropdown item fires before we hide it.
        await Task.Delay(150);
        _dropdownOpen = false;
        // Restore the input text to the current selection (if any) so the field
        // doesn't show a stale partial search string after the user clicks away.
        RestoreSearchText();
    }

    private async Task SelectLink(LinkResponse link)
    {
        _selectedLinkId = link.Id;
        _searchText = GetLinkDisplayText(link);
        _dropdownOpen = false;
        await LoadAnalyticsAsync(link.Id);
    }

    private void ClearSelection()
    {
        _selectedLinkId = null;
        _searchText = string.Empty;
        _analytics = null;
        _analyticsError = null;
        _filteredLinks = _links;
    }

    private void OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            _dropdownOpen = false;
            RestoreSearchText();
        }
    }

    private static string GetLinkDisplayText(LinkResponse link) =>
        link.CustomAlias is not null
            ? $"{link.ShortCode} ({link.CustomAlias})"
            : link.ShortCode;

    private void RestoreSearchText()
    {
        var selected = _selectedLinkId.HasValue
            ? _links.Find(l => l.Id == _selectedLinkId.Value)
            : null;
        _searchText = selected is not null ? GetLinkDisplayText(selected) : string.Empty;
        ApplyFilter();
    }

    private async Task LoadAnalyticsAsync(int linkId)
    {
        _analyticsLoading = true;
        _analyticsError = null;
        _analytics = null;
        StateHasChanged();

        try
        {
            _analytics = await Http.GetFromJsonAsync<AnalyticsResponse>($"api/links/{linkId}/analytics");
        }
        catch (HttpRequestException)
        {
            _analyticsError = "Unable to reach the server. Please check your connection.";
        }
        catch (Exception)
        {
            _analyticsError = "An unexpected error occurred while loading analytics.";
        }
        finally
        {
            _analyticsLoading = false;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TruncateUrl(string url, int max = 60) =>
        url.Length <= max ? url : url[..max] + "…";

    // ── Chart constants ───────────────────────────────────────────────────────

    private const double ChartViewWidth  = 800;
    private const double ChartViewHeight = 320;
    private const double PadLeft         = 55;
    private const double PadRight        = 20;
    private const double PadTop          = 20;
    private const double PadBottom       = 80;
    private const double PlotW              = ChartViewWidth  - PadLeft  - PadRight;   // 725
    private const double PlotH              = ChartViewHeight - PadTop   - PadBottom;  // 220
    private const int    KiloThreshold      = 1000;     // FormatTickValue: values >= this use "k" suffix
    private const double TickLoopEpsilon    = 0.01;     // Prevents floating-point rounding from dropping the last tick

    // ── Chart models ──────────────────────────────────────────────────────────

    private sealed record BarSlice(
        double X,
        double Y,
        double Width,
        double Height,
        string Label,
        int Count,
        bool ShowLabel,
        string LabelTransform);

    private sealed record YAxisTick(double Y, int Value);

    // ── Chart computation ─────────────────────────────────────────────────────

    private static IReadOnlyList<BarSlice> GetBars(IReadOnlyList<DailyClickCount> data)
    {
        if (data.Count == 0) return [];

        var maxCount = data.Max(d => d.Count);
        if (maxCount == 0) maxCount = 1;

        var slotW    = PlotW / data.Count;
        var padding  = Math.Max(1.0, slotW * 0.15);
        var barW     = Math.Max(2.0, slotW - padding * 2);

        // Skip some X-axis labels when there are many bars to avoid overlap.
        var labelStep = data.Count switch
        {
            <= 14 => 1,
            <= 30 => 2,
            <= 60 => 7,
            _     => 14,
        };

        var labelY = PadTop + PlotH + 14;

        return data.Select((d, i) =>
        {
            var h  = (double)d.Count / maxCount * PlotH;
            var x  = PadLeft + i * slotW + padding;
            var y  = PadTop + PlotH - h;
            var cx = x + barW / 2;
            return new BarSlice(
                x, y, barW, h,
                d.Date.ToString("MMM d"),
                d.Count,
                i % labelStep == 0,
                $"rotate(-45 {cx:F1} {labelY:F1})");
        }).ToList();
    }

    private static IReadOnlyList<YAxisTick> GetYAxisTicks(IReadOnlyList<DailyClickCount> data)
    {
        var maxCount = data.Count > 0 ? data.Max(d => d.Count) : 0;
        if (maxCount == 0)
            return [new YAxisTick(PadTop + PlotH, 0)];

        const int targetTicks = 5;
        var rawStep = Math.Ceiling((double)maxCount / targetTicks);

        // Round step up to the nearest "nice" number.
        var step = rawStep switch
        {
            <= 1    => 1,
            <= 2    => 2,
            <= 5    => 5,
            <= 10   => 10,
            <= 20   => 20,
            <= 25   => 25,
            <= 50   => 50,
            <= 100  => 100,
            <= 200  => 200,
            <= 500  => 500,
            <= 1000 => 1000,
            _       => Math.Ceiling(rawStep / 1000) * 1000,
        };

        var niceMax = Math.Ceiling((double)maxCount / step) * step;

        var ticks = new List<YAxisTick>();
        for (double v = 0; v <= niceMax + step * TickLoopEpsilon; v += step)
        {
            var y = PadTop + PlotH - v / niceMax * PlotH;
            ticks.Add(new YAxisTick(y, (int)v));
        }

        return ticks;
    }

    private static string FormatTickValue(int value) =>
        value >= KiloThreshold ? $"{value / (double)KiloThreshold:0.#}k" : value.ToString();

    // ── SVG chart builder ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds the complete SVG bar chart as a <see cref="MarkupString"/> so that
    /// SVG <c>&lt;text&gt;</c> elements with attributes are not mis-parsed by
    /// Razor's special <c>&lt;text&gt;</c> directive handling.
    /// </summary>
    private MarkupString BuildChartSvg(IReadOnlyList<DailyClickCount> data)
    {
        var bars       = GetBars(data);
        var yTicks     = GetYAxisTicks(data);
        var plotBottom = PadTop  + PlotH;
        var plotRight  = ChartViewWidth - PadRight;

        var sb = new System.Text.StringBuilder();
        sb.Append($"<svg viewBox=\"0 0 {ChartViewWidth} {ChartViewHeight}\" " +
                  "xmlns=\"http://www.w3.org/2000/svg\" " +
                  "class=\"analytics-chart\" " +
                  "role=\"img\" " +
                  "aria-label=\"Bar chart showing clicks per day\">");

        sb.Append("<desc>Daily click counts for the selected short link.</desc>");

        // Scoped hover style embedded in the SVG so it works regardless of
        // whether Blazor's CSS scoping applies to MarkupString content.
        sb.Append("<style>.chart-bar{transition:fill .1s ease}.chart-bar:hover{fill:#1558a0}</style>");

        // Plot area background
        sb.Append($"<rect x=\"{PadLeft}\" y=\"{PadTop}\" " +
                  $"width=\"{PlotW}\" height=\"{PlotH}\" fill=\"#f9fafb\" rx=\"4\"/>");

        // Y-axis gridlines and labels
        foreach (var tick in yTicks)
        {
            sb.Append($"<line x1=\"{PadLeft}\" y1=\"{tick.Y:F1}\" " +
                      $"x2=\"{plotRight:F1}\" y2=\"{tick.Y:F1}\" " +
                      "stroke=\"#e5e7eb\" stroke-width=\"1\"/>");
            sb.Append($"<text x=\"{PadLeft - 6:F1}\" y=\"{tick.Y + 4:F1}\" " +
                      "text-anchor=\"end\" font-size=\"11\" fill=\"#6b7280\">" +
                      $"{System.Net.WebUtility.HtmlEncode(FormatTickValue(tick.Value))}</text>");
        }

        // Axes
        sb.Append($"<line x1=\"{PadLeft}\" y1=\"{PadTop}\" " +
                  $"x2=\"{PadLeft}\" y2=\"{plotBottom:F1}\" " +
                  "stroke=\"#d1d5db\" stroke-width=\"1.5\"/>");
        sb.Append($"<line x1=\"{PadLeft}\" y1=\"{plotBottom:F1}\" " +
                  $"x2=\"{plotRight:F1}\" y2=\"{plotBottom:F1}\" " +
                  "stroke=\"#d1d5db\" stroke-width=\"1.5\"/>");

        // Bars and X-axis labels
        foreach (var bar in bars)
        {
            if (bar.Height > 0)
            {
                var clickWord = bar.Count == 1 ? "click" : "clicks";
                var titleText = System.Net.WebUtility.HtmlEncode($"{bar.Label}: {bar.Count} {clickWord}");
                sb.Append($"<rect x=\"{bar.X:F1}\" y=\"{bar.Y:F1}\" " +
                          $"width=\"{bar.Width:F1}\" height=\"{bar.Height:F1}\" " +
                          "fill=\"#1b6ec2\" rx=\"2\" class=\"chart-bar\">" +
                          $"<title>{titleText}</title></rect>");
            }

            if (bar.ShowLabel)
            {
                var cx = bar.X + bar.Width / 2;
                sb.Append($"<text x=\"{cx:F1}\" y=\"{plotBottom + 14:F1}\" " +
                          "text-anchor=\"end\" font-size=\"10\" fill=\"#6b7280\" " +
                          $"transform=\"{bar.LabelTransform}\">" +
                          $"{System.Net.WebUtility.HtmlEncode(bar.Label)}</text>");
            }
        }

        sb.Append("</svg>");
        return new MarkupString(sb.ToString());
    }
}

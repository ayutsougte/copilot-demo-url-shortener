using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ShortLinkApp.Client.Pages;

public partial class Links
{
    // ── Injected services ─────────────────────────────────────────────────────

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ── API DTOs ──────────────────────────────────────────────────────────────

    private record LinkResponse(
        int Id,
        string ShortCode,
        string OriginalUrl,
        string? CustomAlias,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        bool IsActive);

    private record AnalyticsResponse(
        int LinkId,
        int TotalClicks);

    // ── View model ────────────────────────────────────────────────────────────

    private sealed class LinkRow
    {
        public int Id { get; init; }
        public string ShortCode { get; init; } = string.Empty;
        public string OriginalUrl { get; init; } = string.Empty;
        public string? CustomAlias { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public bool IsActive { get; init; }
        public int TotalClicks { get; set; }
        public bool ClicksLoading { get; set; } = true;

        public bool IsExpired =>
            ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    // ── Component state ───────────────────────────────────────────────────────

    private List<LinkRow> _rows = [];
    private bool _isLoading = true;
    private string? _loadError;
    private string? _deleteError;

    private string _filterText = string.Empty;
    private string _filterStatus = "all";

    private string _sortColumn = "shortcode";
    private bool _sortAscending = true;

    private int? _deleteConfirmId;
    private bool _isDeleting;
    private int? _copiedId;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await LoadLinksAsync();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    private async Task LoadLinksAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            var links = await Http.GetFromJsonAsync<LinkResponse[]>("api/links");

            _rows = (links ?? [])
                .Select(l => new LinkRow
                {
                    Id = l.Id,
                    ShortCode = l.ShortCode,
                    OriginalUrl = l.OriginalUrl,
                    CustomAlias = l.CustomAlias,
                    CreatedAt = l.CreatedAt,
                    ExpiresAt = l.ExpiresAt,
                    IsActive = l.IsActive,
                    ClicksLoading = true,
                    TotalClicks = 0,
                })
                .ToList();
        }
        catch (HttpRequestException)
        {
            _loadError = "Unable to reach the server. Please check your connection.";
        }
        catch (Exception)
        {
            _loadError = "An unexpected error occurred while loading links.";
        }
        finally
        {
            _isLoading = false;
        }

        // Load click counts in the background after the table is rendered.
        if (_rows.Count > 0)
            _ = LoadClickCountsAsync();
    }

    private async Task LoadClickCountsAsync()
    {
        var tasks = _rows.Select(async row =>
        {
            try
            {
                var analytics = await Http.GetFromJsonAsync<AnalyticsResponse>(
                    $"api/links/{row.Id}/analytics");
                row.TotalClicks = analytics?.TotalClicks ?? 0;
            }
            catch
            {
                row.TotalClicks = 0;
            }
            finally
            {
                row.ClicksLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        });

        await Task.WhenAll(tasks);
    }

    // ── Filtering & sorting ───────────────────────────────────────────────────

    private List<LinkRow> FilteredAndSorted
    {
        get
        {
            IEnumerable<LinkRow> result = _rows;

            // Text filter
            if (!string.IsNullOrWhiteSpace(_filterText))
            {
                var term = _filterText.Trim();
                result = result.Where(r =>
                    r.ShortCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.OriginalUrl.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (r.CustomAlias != null && r.CustomAlias.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            // Status filter
            result = _filterStatus switch
            {
                "active"   => result.Where(r => r.IsActive && !r.IsExpired),
                "inactive" => result.Where(r => !r.IsActive),
                "expired"  => result.Where(r => r.IsExpired),
                _          => result,
            };

            // Sorting
            result = (_sortColumn, _sortAscending) switch
            {
                ("shortcode", true)  => result.OrderBy(r => r.ShortCode, StringComparer.OrdinalIgnoreCase),
                ("shortcode", false) => result.OrderByDescending(r => r.ShortCode, StringComparer.OrdinalIgnoreCase),
                ("url", true)        => result.OrderBy(r => r.OriginalUrl, StringComparer.OrdinalIgnoreCase),
                ("url", false)       => result.OrderByDescending(r => r.OriginalUrl, StringComparer.OrdinalIgnoreCase),
                ("clicks", true)     => result.OrderBy(r => r.TotalClicks),
                ("clicks", false)    => result.OrderByDescending(r => r.TotalClicks),
                ("expires", true)    => result.OrderBy(r => r.ExpiresAt ?? DateTime.MaxValue),
                ("expires", false)   => result.OrderByDescending(r => r.ExpiresAt ?? DateTime.MinValue),
                ("status", true)     => result.OrderBy(r => StatusOrder(r)),
                ("status", false)    => result.OrderByDescending(r => StatusOrder(r)),
                _                    => result.OrderBy(r => r.ShortCode, StringComparer.OrdinalIgnoreCase),
            };

            return result.ToList();
        }
    }

    private static int StatusOrder(LinkRow r)
    {
        if (r.IsExpired) return 2;
        if (!r.IsActive) return 1;
        return 0;
    }

    private void SetSort(string column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
    }

    private string SortClass(string column) =>
        _sortColumn == column ? "sort-btn--active" : string.Empty;

    private string SortIcon(string column)
    {
        if (_sortColumn != column) return "⇅";
        return _sortAscending ? "↑" : "↓";
    }

    // ── Copy to clipboard ─────────────────────────────────────────────────────

    private async Task CopyToClipboardAsync(LinkRow row)
    {
        var url = ShortUrl(row.ShortCode);
        try
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", url);
            _copiedId = row.Id;
            StateHasChanged();
            await Task.Delay(2000);
            _copiedId = null;
        }
        catch (JSException)
        {
            // Clipboard API unavailable — silently ignore.
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    private void RequestDelete(int id)
    {
        _deleteConfirmId = id;
        _deleteError = null;
    }

    private void CancelDelete()
    {
        _deleteConfirmId = null;
    }

    private async Task ConfirmDeleteAsync(int id)
    {
        _isDeleting = true;
        _deleteError = null;

        try
        {
            var response = await Http.DeleteAsync($"api/links/{id}");

            if (response.IsSuccessStatusCode)
                _rows.RemoveAll(r => r.Id == id);
            else
                _deleteError = $"Failed to delete link (HTTP {(int)response.StatusCode}). Please try again.";
        }
        catch (HttpRequestException)
        {
            _deleteError = "Unable to reach the server. Please check your connection.";
        }
        catch (Exception)
        {
            _deleteError = "An unexpected error occurred while deleting the link.";
        }
        finally
        {
            _isDeleting = false;
            _deleteConfirmId = null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string ShortUrl(string shortCode)
    {
        var apiBase = (Configuration["ApiBaseUrl"] ?? string.Empty).TrimEnd('/');
        return $"{apiBase}/{shortCode}";
    }
}

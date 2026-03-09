using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace ShortLinkApp.Client.Pages;

public partial class Home
{
    // ── Injected services ─────────────────────────────────────────────────────

    [Inject] private HttpClient Http { get; set; } = default!;

    // ── API DTO ───────────────────────────────────────────────────────────────

    private record DashboardStatsResponse(int TotalLinks, int TotalClicks, int ActiveLinks);

    // ── Component state ───────────────────────────────────────────────────────

    private DashboardStatsResponse? _stats;
    private bool _statsLoading = true;
    private string? _statsError;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await LoadStatsAsync();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    private async Task LoadStatsAsync()
    {
        _statsLoading = true;
        _statsError = null;

        try
        {
            _stats = await Http.GetFromJsonAsync<DashboardStatsResponse>("api/links/stats");
        }
        catch (HttpRequestException)
        {
            _statsError = "Unable to reach the server.";
        }
        catch (Exception)
        {
            _statsError = "An unexpected error occurred.";
        }
        finally
        {
            _statsLoading = false;
        }
    }
}

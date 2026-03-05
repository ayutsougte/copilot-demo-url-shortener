using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ShortLinkApp.Client.Pages;

public partial class CreateLink
{
    // ── Injected services ─────────────────────────────────────────────────────

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ── Form model ────────────────────────────────────────────────────────────

    private sealed class FormModel
    {
        [Required(ErrorMessage = "Destination URL is required.")]
        [Url(ErrorMessage = "Please enter a valid URL (e.g. https://example.com).")]
        public string OriginalUrl { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9_-]*$",
            ErrorMessage = "Custom alias may only contain letters, digits, hyphens, and underscores.")]
        public string? CustomAlias { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    // ── API DTOs ──────────────────────────────────────────────────────────────

    private record CreateLinkRequest(
        string OriginalUrl,
        string? CustomAlias,
        DateTime? ExpiresAt);

    private record LinkResponse(
        int Id,
        string ShortCode,
        string OriginalUrl,
        string? CustomAlias,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        bool IsActive);

    private sealed class ValidationProblemDetails
    {
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    private sealed class ConflictError
    {
        public string? Error { get; set; }
    }

    // ── Component state ───────────────────────────────────────────────────────

    private FormModel _model = new();
    private LinkResponse? _createdLink;
    private string _shortUrl = string.Empty;
    private bool _isSubmitting;
    private bool _copied;
    private string _serverError = string.Empty;
    private Dictionary<string, string[]> _apiErrors = new(StringComparer.OrdinalIgnoreCase);

    // ── Event handlers ────────────────────────────────────────────────────────

    private async Task HandleSubmitAsync()
    {
        _isSubmitting = true;
        _serverError = string.Empty;
        _apiErrors = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            var alias = string.IsNullOrWhiteSpace(_model.CustomAlias) ? null : _model.CustomAlias.Trim();

            // The API requires ExpiresAt to have DateTimeKind.Utc.
            // InputDateType.DateTimeLocal returns Unspecified kind; we re-tag it as UTC here.
            // The field label already instructs the user to enter times in UTC.
            DateTime? expiresAt = _model.ExpiresAt.HasValue
                ? DateTime.SpecifyKind(_model.ExpiresAt.Value, DateTimeKind.Utc)
                : null;

            var request = new CreateLinkRequest(_model.OriginalUrl.Trim(), alias, expiresAt);
            var response = await Http.PostAsJsonAsync("api/links", request);

            if (response.IsSuccessStatusCode)
            {
                _createdLink = await response.Content.ReadFromJsonAsync<LinkResponse>();
                var apiBase = (Configuration["ApiBaseUrl"] ?? string.Empty).TrimEnd('/');
                _shortUrl = $"{apiBase}/{_createdLink!.ShortCode}";
            }
            else
            {
                await HandleErrorResponseAsync(response);
            }
        }
        catch (HttpRequestException)
        {
            _serverError = "Unable to reach the server. Please check your connection and try again.";
        }
        catch (Exception)
        {
            _serverError = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage response)
    {
        switch ((int)response.StatusCode)
        {
            case 400:
            case 422:
                var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (problem?.Errors is { Count: > 0 })
                    _apiErrors = new Dictionary<string, string[]>(problem.Errors, StringComparer.OrdinalIgnoreCase);
                else
                    _serverError = "Validation failed. Please review your input.";
                break;

            case 401:
            case 403:
                _serverError = "Authentication error. Please check the API key configuration.";
                break;

            case 409:
                var conflict = await response.Content.ReadFromJsonAsync<ConflictError>();
                _apiErrors["CustomAlias"] = [conflict?.Error ?? "This alias is already taken. Please choose a different one."];
                break;

            case 503:
                _serverError = "The service is temporarily unavailable. Please try again later.";
                break;

            default:
                _serverError = $"An unexpected error occurred (HTTP {(int)response.StatusCode}). Please try again.";
                break;
        }
    }

    private async Task CopyToClipboardAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", _shortUrl);
            _copied = true;
            StateHasChanged();
            await Task.Delay(2000);
            _copied = false;
        }
        catch (JSException)
        {
            // Clipboard API unavailable (e.g. non-HTTPS context, permission denied) — silently ignore.
        }
    }

    private void ResetForm()
    {
        _model = new();
        _createdLink = null;
        _shortUrl = string.Empty;
        _copied = false;
        _serverError = string.Empty;
        _apiErrors = new(StringComparer.OrdinalIgnoreCase);
    }
}

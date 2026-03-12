using Serilog;

namespace ZNXHelpers;

public class NdiHelper
{
    private  readonly ILogger _logger = Log.ForContext<NdiHelper>();
    
    public async Task<string?> CallNdiEndpoint(string baseUrl, string endpointPath, string accessToken,
        bool lambdaHeaderMode, bool isPostMethod = true)
    {
        var apiKey = EnvHelper.GetString("API_GATEWAY_KEY", "");
        var isDev = EnvHelper.GetString("ASPNETCORE_ENVIRONMENT", "Development") == "Development";

        using var httpClientHandler = new HttpClientHandler();
        if (isDev)
        {
            _logger.Debug("Disable HTTPS self-signed cert validation for development");
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, sslErr) =>
            {
                if (isDev)
                {
                    return true; // Only disable for dev environment
                }

                return sslErr == System.Net.Security.SslPolicyErrors.None;
            };
        }

        using var client = new HttpClient(httpClientHandler);
        client.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrEmpty(apiKey))
        {
            _logger.Debug("Appending API Key");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        var dict = new Dictionary<string, string>();

        if (lambdaHeaderMode)
        {
            _logger.Debug("Set via Auth Post Data");
            dict.Add("auth", accessToken);
        }
        else
        {
            _logger.Debug("Set under Authorization Header");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        var data = new FormUrlEncodedContent(dict);

        _logger.Debug("Querying: {ClientBaseAddress}{EndpointPath}", client.BaseAddress, endpointPath);

        try
        {
            HttpResponseMessage httpResponseMessage = isPostMethod
                ? await client.PostAsync($"{endpointPath}", data)
                : await client.GetAsync($"{endpointPath}");
            string msg = await httpResponseMessage.Content.ReadAsStringAsync();
            string statusCode = httpResponseMessage.StatusCode.ToString();
            _logger.Debug("Status Code: {StatusCode}, Response: {Msg}", statusCode, msg);

            return msg;
        }
        catch (HttpRequestException e)
        {
            _logger.Error(e, "Failed to get response from GW. Error: {EMessage}", e.Message);
            _logger.Debug("{Err}", e.ToString());

            return null;
        }
        catch (ArgumentNullException e)
        {
            _logger.Error(e, "Failed to get response from GW. Error: {EMessage}", e.Message);
            _logger.Debug("{Err}", e.ToString());

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;

namespace SET.Core.Services;

public class RequestProvider : IRequestProvider
{
    private readonly Lazy<HttpClient> m_httpClient;
    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly ISettingsService m_settingsService;
    private readonly ILoggingService m_loggingService;

    public IUrlBuilder UrlBuilder { get; }

    public RequestProvider( IHttpClientFactory httpClientFactory, ISettingsService settingsService, ILoggingService loggingService, IUrlBuilder urlBuilder )
    {
        m_httpClientFactory = httpClientFactory;
        m_settingsService = settingsService;
        m_loggingService = loggingService;
        UrlBuilder = urlBuilder;
        m_httpClient = new( 
            valueFactory: () =>
                {
                    HttpClient httpClient = m_httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( MediaTypeNames.Text.Plain ) );
                    return httpClient;
                },
            mode: LazyThreadSafetyMode.ExecutionAndPublication 
        );
    }

    public async Task<TResult> GetAsync<TResult>( string uri, string token = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );
        HttpResponseMessage response = await httpClient.GetAsync( uri ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();

        TResult result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();

        return result;
    }

    public async Task<HttpResponseMessage> PostAsync<TRequest>( string uri, TRequest data, string token = "", string header = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        if (!string.IsNullOrEmpty( header ))
        {
            AddHeaderParameter( httpClient, header );
        }

        string contentAsStr = JsonSerializer.Serialize( data );

        var content = new StringContent( contentAsStr, Encoding.UTF8, MediaTypeNames.Application.Json );

        HttpResponseMessage response = await httpClient.PostAsync( uri, content ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();

        return response;
    }

    public async Task<TResult> PostAsync<TRequest, TResult>(
        string uri,
        TRequest data,
        string token = "",
        string header = "" )
    {
        HttpResponseMessage response = await PostAsync<TRequest>( uri, data, token, header ).DefaultConfigureAwait();
        TResult result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();
        return result;
    }

    public async Task<TResult> PostAsync<TResult>( string uri, string token = "", string header = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        if (!string.IsNullOrEmpty( header ))
        {
            AddHeaderParameter( httpClient, header );
        }
        //var content = new StringContent( data );
        //content.Headers.ContentType = new MediaTypeHeaderValue( mediaType: "application/x-www-form-urlencoded" );

        HttpResponseMessage response = await httpClient.PostAsync( uri, null ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();
        TResult result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();

        return result;
    }

    public async Task<TResult> PostAsync<TResult>( string uri, string data, string clientId, string clientSecret )
    {
        HttpClient httpClient = GetOrCreateHttpClient( string.Empty );

        if (!string.IsNullOrWhiteSpace( clientId ) && !string.IsNullOrWhiteSpace( clientSecret ))
        {
            AddBasicAuthenticationHeader( httpClient, clientId, clientSecret );
        }

        var content = new StringContent( data );
        content.Headers.ContentType = new MediaTypeHeaderValue( mediaType: "application/x-www-form-urlencoded" );

        HttpResponseMessage response = await httpClient.PostAsync( uri, content ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();
        TResult result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();

        return result;
    }

    public async Task<HttpResponseMessage> PutAsync<TRequest>( string uri, TRequest data, string token = "", string header = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        if (!string.IsNullOrEmpty( header ))
        {
            AddHeaderParameter( httpClient, header );
        }

        string dataAsJson = JsonSerializer.Serialize( data );
        var content = new StringContent( dataAsJson );
        content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

        HttpResponseMessage response = await httpClient.PutAsync( uri, content ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();
        return response;
    }

    public async Task<TResult> PutAsync<TRequest, TResult>( string uri, TRequest data, string token = "", string header = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        if (!string.IsNullOrEmpty( header ))
        {
            AddHeaderParameter( httpClient, header );
        }

        string dataAsJson = JsonSerializer.Serialize( data );
        var content = new StringContent( dataAsJson );
        content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

        HttpResponseMessage response = await httpClient.PutAsync( uri, content ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();
        TResult result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();

        return result;
    }

    public async Task<TResult> PutAsync<TResult>( string uri, string data, string token = "", string header = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        if (!string.IsNullOrEmpty( header ))
        {
            AddHeaderParameter( httpClient, header );
        }

        var content = new StringContent( data );
        content.Headers.ContentType = new MediaTypeHeaderValue( mediaType: "application/x-www-form-urlencoded" );

        HttpResponseMessage response = await httpClient.PutAsync( uri, content ).DefaultConfigureAwait();

        await HandleResponse( response ).DefaultConfigureAwait();
        TResult? result = await response.Content.ReadFromJsonAsync<TResult>().DefaultConfigureAwait();

        return result;
    }

    public async Task<HttpResponseMessage> DeleteAsync( string uri, string token = "" )
    {
        HttpClient httpClient = GetOrCreateHttpClient( token );

        HttpResponseMessage response = await httpClient.DeleteAsync( uri ).DefaultConfigureAwait();
        await HandleResponse( response ).DefaultConfigureAwait();
        return response;
    }

    private HttpClient GetOrCreateHttpClient( string token = "" )
    {
        HttpClient httpClient = m_httpClient.Value;

        httpClient.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty( token )
                ? new AuthenticationHeaderValue( scheme: "Bearer", token )
                : null;

        return httpClient;
    }

    private static void AddHeaderParameter( HttpClient httpClient, string parameter )
    {
        if (httpClient != null && !string.IsNullOrEmpty( parameter ))
        {
            httpClient.DefaultRequestHeaders.Add( parameter, Guid.NewGuid().ToString() );
        }
    }

    private static void AddBasicAuthenticationHeader( HttpClient httpClient, string clientId, string clientSecret )
    {
        if (httpClient == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace( clientId ) || string.IsNullOrWhiteSpace( clientSecret ))
        {
            return;
        }

        //httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue( clientId, clientSecret );
    }

    private async Task HandleResponse( HttpResponseMessage response )
    {
        string content;
        if (m_settingsService.IsDebug && response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync().DefaultConfigureAwait();
            m_loggingService.LogInfo( logRecord: $"Response content: {content}" );
        }
        else if (!response.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync().DefaultConfigureAwait();

            if (m_settingsService.IsDebug)
            {
                m_loggingService.LogInfo( $"Response content: {content}" );
            }

            if (response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceAuthenticationException( content );
            }
            else
            {
                throw new ExtendedHttpRequestException( response.StatusCode, content );
            }
        }
    }
}

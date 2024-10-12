namespace Principles.Core.Services;

public interface IRequestProvider
{
    IUrlBuilder UrlBuilder { get; }
    Task<TResult> GetAsync<TResult>( string uri, string token = "" );
    Task<HttpResponseMessage> PostAsync<TRequest>( string uri, TRequest data, string token = "", string header = "" );
    Task<TResult> PostAsync<TRequest, TResult>( string uri, TRequest data, string token = "", string header = "" );
    Task<TResult> PostAsync<TResult>( string uri, string token = "", string header = "" );
    Task<TResult> PostAsync<TResult>( string uri, string data, string clientId, string clientSecret );
    Task<HttpResponseMessage> PutAsync<TRequest>( string uri, TRequest data, string token = "", string header = "" );
    Task<TResult> PutAsync<TResult>( string uri, string data, string token = "", string header = "" );
    Task<TResponse> PutAsync<TRequest, TResponse>( string uri, TRequest data, string token = "", string header = "" );
    Task<HttpResponseMessage> DeleteAsync( string uri, string token = "" );
}
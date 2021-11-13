using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace QQBot.Utils
{

    /// <summary>
    /// HttpClient请求帮助类
    /// </summary>
    public sealed class HttpClientHelper
    {
        public static string FastGet(string url, IDictionary<string, string> headers = null)
        {
            HttpResponseMessage response = null;
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                UseProxy = false,
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }
                response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                if (headers != null)
                {
                    httpClient.DefaultRequestHeaders.Clear();
                }
                httpClient?.Dispose();
                response?.Dispose();
            }
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static T Post<T>(string url, string body = null, string token = null, int timeOut = 30)
        {
            Encoding encoding = Encoding.UTF8;
            HttpContent httpContent = null;
            HttpResponseMessage response = null;
            CookieContainer cookieContainer = new CookieContainer();

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                UseProxy = false,
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
                catch (Exception)
                {

                }
            }
            try
            {
                httpContent = new StringContent(body, encoding, "application/json");
                httpClient.Timeout = new TimeSpan(0, 0, timeOut);
                response = httpClient.PostAsync(url, httpContent).Result;
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                return default;
            }
            finally
            {
                httpClient?.Dispose();
                httpContent?.Dispose();
                response?.Dispose();
            }
        }
        public static string Get(string url, string token = null, IDictionary<string, string> headers = null)
        {
            HttpResponseMessage response = null;
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                UseProxy = false,
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            //httpClient.Timeout = 
            try
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }

                response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (headers != null)
                {
                    httpClient.DefaultRequestHeaders.Clear();
                }
                httpClient?.Dispose();
                response?.Dispose();
            }
        }


        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T Get<T>(string url, string token = null, IDictionary<string, string> headers = null)
        {
            var d = Get(url, token, headers);
            if (string.IsNullOrEmpty(d))
            {
                return default(T);
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(d);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T Put<T>(string url, object data, string token = null)
        {
            HttpResponseMessage response = null;
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true,
                UseProxy = false,
            };
            var httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            HttpClient httpClient = new HttpClient(httpClientHandler);
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }

                response = httpClient.PutAsync(url, httpContent).Result;
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                return default;
            }
            finally
            {
                httpClient?.Dispose();
                response?.Dispose();
            }
        }


        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static T Delete<T>(string url, object data, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            CookieContainer cookieContainer = new CookieContainer();

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                UseProxy = false,
                CookieContainer = cookieContainer
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
            var responseTask = httpClient.SendAsync(request).Result;

            if (responseTask.IsSuccessStatusCode)
            {
                var resultStr = responseTask.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(resultStr);
            }
            return default;
        }
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}

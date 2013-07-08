// <copyright file="AlgoliaClient.cs" company="Christopher MANEU">
// Copyright (c) 2013 Christopher MANEU - Under MIT License
// </copyright>
// <author>Christopher MANEU</author>
// <date>05/07/2013</date>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Algolia.Search
{

    /// <summary>
    /// Client for the Algolia Search cloud API.
    /// Based on the PHP version (https://github.com/algolia/algoliasearch-client-php/blob/master/algoliasearch.php).
    /// </summary>
    public class AlgoliaClient
    {
        private IEnumerable<string> _hosts;
        private string _applicationId;
        private string _apiKey;

        private HttpClient _httpClient;

        protected HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }
                return _httpClient;
            }
        }

        public AlgoliaClient(string applicationId, string apiKey, IEnumerable<string> hosts)
        {
            if(string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentOutOfRangeException("applicationId","An application Id is requred.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentOutOfRangeException("apiKey", "An API key is required.");

            IEnumerable<string> allHosts = hosts as string[] ?? hosts.ToArray();
            if (hosts == null || !allHosts.Any())
                throw new ArgumentOutOfRangeException("hosts", "At least one host is requred");

            _applicationId = applicationId;
            _apiKey = apiKey;

            // randomize elements of hostsArray (act as a kind of load-balancer)
            _hosts = allHosts.OrderBy(s => Guid.NewGuid());

            HttpClient.DefaultRequestHeaders.Add("X-Algolia-Application-Id", applicationId);
            HttpClient.DefaultRequestHeaders.Add("X-Algolia-API-Key", apiKey);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<string> ListIndexes()
        {
            return ExecuteRequest("GET", "/1/indexes/");
        }

        public Task DeleteIndex(string indexName)
        {
            return ExecuteRequest("DELETE", "/1/indexes/" + Uri.EscapeDataString(indexName));
        }

        // The user management of the API is not yet implemented.








        private async Task<string> ExecuteRequest(string method, string requestUrl)
        {
            foreach (string host in _hosts)
            {
                try
                {
                    string url = string.Format("https://{0}{1}", host, requestUrl);

                    switch (method)
                    {
                        case "GET":
                            HttpResponseMessage getResponseMessage = await HttpClient.GetAsync(url);
                            if (getResponseMessage.IsSuccessStatusCode)
                            {
                                return await getResponseMessage.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                if (getResponseMessage.StatusCode == HttpStatusCode.Forbidden)
                                {
                                    throw new AlgoliaException("Invalid application ID or API Key");
                                }
                                if (getResponseMessage.StatusCode == HttpStatusCode.NotFound)
                                {
                                    throw new AlgoliaException("Resource does not exist.");
                                }

                                // if the error code is not 403/404 or 503
                                // parse the json and send the message value
                                // PHP code: 
                                 //if (intval(floor($http_status / 100)) !== 2) {
                                 //       throw new AlgoliaException($answer["message"]);
                                 //   }

                            }
                            
                            break;

                        case "DELETE":
                            HttpResponseMessage deleteResponseMessage = await HttpClient.DeleteAsync(url);
                            if (deleteResponseMessage.IsSuccessStatusCode)
                            {
                                return await deleteResponseMessage.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                if (deleteResponseMessage.StatusCode == HttpStatusCode.Forbidden)
                                {
                                    throw new AlgoliaException("Invalid application ID or API Key");
                                }
                                if (deleteResponseMessage.StatusCode == HttpStatusCode.NotFound)
                                {
                                    throw new AlgoliaException("Resource does not exist.");
                                }

                                // if the error code is not 403/404 or 503
                                // parse the json and send the message value
                                // PHP code: 
                                //if (intval(floor($http_status / 100)) !== 2) {
                                //       throw new AlgoliaException($answer["message"]);
                                //   }

                            }

                            break;
                    }

                }
                catch (AlgoliaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AlgoliaClient exception: " + ex.ToString());
                }
            }

            throw new AlgoliaException("Hosts unreachable.");
        }
    }
}

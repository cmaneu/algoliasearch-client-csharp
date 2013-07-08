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
using System.Threading.Tasks;

namespace Algolia.Search
{
    public class Index
    {
        private IEnumerable<string> _hosts;
        private string _applicationId;
        private string _apiKey;
        private string _indexName;
        private string _urlIndexName;

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


        public Index(string applicationId, string apiKey, IEnumerable<string> hosts, string indexName)
        {

            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentOutOfRangeException("applicationId", "An application Id is requred.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentOutOfRangeException("apiKey", "An API key is required.");

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentOutOfRangeException("indexName", "An index name is required.");

            IEnumerable<string> allHosts = hosts as string[] ?? hosts.ToArray();
            if (hosts == null || !allHosts.Any())
                throw new ArgumentOutOfRangeException("hosts", "At least one host is requred");

            _applicationId = applicationId;
            _apiKey = apiKey;
            _indexName = indexName;
            _urlIndexName = Uri.EscapeDataString(indexName);

            // randomize elements of hostsArray (act as a kind of load-balancer)
            _hosts = allHosts.OrderBy(s => Guid.NewGuid());

            HttpClient.DefaultRequestHeaders.Add("X-Algolia-Application-Id", applicationId);
            HttpClient.DefaultRequestHeaders.Add("X-Algolia-API-Key", apiKey);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public Task AddObject(IDictionary<string, string> content, string objectId = null)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                return ExecuteRequest("POST", string.Format("/1/indexes/{0}",_urlIndexName),content);
            }
            else
            {
                return ExecuteRequest("PUT", string.Format("/1/indexes/{0}/{1}", _urlIndexName, Uri.EscapeDataString(objectId)),content);
            }
        }

        public Task<string> Search(string query)
        {
            return ExecuteRequest("GET", string.Format("/1/indexes/{0}?query={1}",_urlIndexName,Uri.EscapeDataString(query)));
        }

        public Task GetObject(string objectId, IEnumerable<string> attributesToRetrieve = null)
        {
            throw new NotImplementedException();
        }

        // UpdateObject
        // SaveObject
        // SaveObjects
        // DeleteObject
        // GetSettings
        // SetSettings



        private async Task<string> ExecuteRequest(string method, string requestUrl, object content =null)
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

                        case "POST":
                            HttpContent postcontent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(content));
                            HttpResponseMessage postResponseMessage = await HttpClient.PostAsync(url, postcontent);
                            if (postResponseMessage.IsSuccessStatusCode)
                            {
                                return await postResponseMessage.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                if (postResponseMessage.StatusCode == HttpStatusCode.Forbidden)
                                {
                                    throw new AlgoliaException("Invalid application ID or API Key");
                                }
                                if (postResponseMessage.StatusCode == HttpStatusCode.NotFound)
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
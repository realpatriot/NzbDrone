﻿using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NLog;
using NzbDrone.Api.REST;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class ClientBase<TResource> where TResource : RestResource, new()
    {
        private readonly IRestClient _restClient;
        private readonly string _resource;

        private readonly Logger _logger;

        public ClientBase(IRestClient restClient, string resource = null)
        {
            if (resource == null)
            {
                resource = new TResource().ResourceName;
            }

            _restClient = restClient;
            _resource = resource;
            _logger = LogManager.GetLogger("REST");
        }

        public List<TResource> All()
        {
            var request = BuildRequest();
            return Get<List<TResource>>(request);
        }

        public TResource Post(TResource body)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Post<TResource>(request);
        }

        public void Delete(int id)
        {
            var request = BuildRequest(id.ToString());
            Delete(request);
        }

        public List<string> InvalidPost(TResource body)
        {
            var request = BuildRequest();
            request.AddBody(body);
            return Post<List<string>>(request, HttpStatusCode.BadRequest);
        }

        protected RestRequest BuildRequest(string command = "")
        {
            return new RestRequest(_resource + "/" + command.Trim('/'))
                {
                    RequestFormat = DataFormat.Json
                };
        }

        protected T Get<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK) where T : new()
        {
            request.Method = Method.GET;
            return Execute<T>(request, statusCode);
        }

        public T Post<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.Created) where T : new()
        {
            request.Method = Method.POST;
            return Execute<T>(request, statusCode);
        }

        public void Delete(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            request.Method = Method.DELETE;
            Execute<object>(request, statusCode);
        }

        private T Execute<T>(IRestRequest request, HttpStatusCode statusCode) where T : new()
        {
            _logger.Info("{0}: {1}", request.Method, _restClient.BuildUri(request));

            var response = _restClient.Execute<T>(request);
            _logger.Info("Response: {0}", response.Content);

            response.StatusCode.Should().Be(statusCode);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            response.ErrorMessage.Should().BeBlank();

            return response.Data;
        }

    }
}
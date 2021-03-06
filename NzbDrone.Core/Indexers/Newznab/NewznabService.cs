using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Indexers.Newznab
{
    public interface INewznabService
    {
        List<NewznabDefinition> All();
        List<NewznabDefinition> Enabled();
        NewznabDefinition Insert(NewznabDefinition definition);
        void Update(NewznabDefinition definition);
    }

    public class NewznabService : INewznabService, IHandle<ApplicationStartedEvent>
    {
        private readonly INewznabRepository _newznabRepository;
        private readonly Logger _logger;

        public NewznabService(INewznabRepository newznabRepository, Logger logger)
        {
            _newznabRepository = newznabRepository;
            _logger = logger;
        }

        public List<NewznabDefinition> All()
        {
            return _newznabRepository.All().ToList();
        }

        public List<NewznabDefinition> Enabled()
        {
            return _newznabRepository.Enabled().ToList();
        }

        public NewznabDefinition Insert(NewznabDefinition definition)
        {
            definition.Url = definition.Url.ToLower();
            _logger.Debug("Adding Newznab definition for {0}", definition.Name);
            return _newznabRepository.Insert(definition);
        }

        public void Update(NewznabDefinition definition)
        {
            definition.Url = definition.Url.ToLower();
            _logger.Debug("Updating Newznab definition for {0}", definition.Name);
            _newznabRepository.Update(definition);
        }

        public void Delete(int id)
        {

            _newznabRepository.Delete(id);
        }

        public void CheckHostname(string url)
        {
            try
            {
                var uri = new Uri(url);
                var hostname = uri.DnsSafeHost;

                Dns.GetHostEntry(hostname);
            }
            catch (Exception ex)
            {
                _logger.Error("Invalid address {0}, please correct the site URL.", url);
                _logger.TraceException(ex.Message, ex);
                throw;
            }

        }

        public void Handle(ApplicationStartedEvent message)
        {
            var newznabIndexers = new List<NewznabDefinition>
                                      {
                                              new NewznabDefinition { Enable = false, Name = "Nzbs.org", Url = "http://nzbs.org" },
                                              new NewznabDefinition { Enable = false, Name = "Nzb.su", Url = "https://nzb.su" },
                                              new NewznabDefinition { Enable = false, Name = "Dognzb.cr", Url = "https://dognzb.cr" }
                                      };

            _logger.Debug("Initializing Newznab indexers. Count {0}", newznabIndexers);

            try
            {
                var currentIndexers = All();

                _logger.Debug("Deleting broken Newznab indexer");
                var brokenIndexers = currentIndexers.Where(i => String.IsNullOrEmpty(i.Name) || String.IsNullOrWhiteSpace(i.Url)).ToList();
                brokenIndexers.ForEach(e => Delete(e.Id));

                currentIndexers = All();

                foreach (var feedProvider in newznabIndexers)
                {
                    try
                    {
                        NewznabDefinition indexerLocal = feedProvider;
                        var currentIndexer = currentIndexers
                                .FirstOrDefault(c => new Uri(c.Url.ToLower()).Host == new Uri(indexerLocal.Url.ToLower()).Host);

                        if (currentIndexer == null)
                        {
                            var definition = new NewznabDefinition
                            {
                                Enable = false,
                                Name = indexerLocal.Name,
                                Url = indexerLocal.Url,
                                ApiKey = indexerLocal.ApiKey,
                            };

                            Insert(definition);
                        }

                        else
                        {
                            currentIndexer.Url = indexerLocal.Url;
                            Update(currentIndexer);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("An error occurred while setting up indexer: " + feedProvider.Name, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("An Error occurred while initializing Newznab Indexers", ex);
            }
        }
    }
}
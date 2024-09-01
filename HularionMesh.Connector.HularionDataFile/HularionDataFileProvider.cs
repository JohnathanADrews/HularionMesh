#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HularionMesh.Domain;
using HularionMesh.DomainAggregate;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using HularionMesh.Injector;
using HularionMesh.Memory;
using HularionMesh.Standard;
using HularionMesh.Structure;
using HularionMesh.Response;
using HularionCore.Injector;
using HularionCore.Pattern.Functional;
using HularionMesh.User;
using HularionMesh.Repository;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Implements IMeshServiceProvider for storing to a HularionDataFile.
    /// </summary>
    public class HularionDataFileProvider : IMeshServiceProvider
    {
        /// <summary>
        /// The communicators for a domain service.
        /// </summary>
        public IDomainServiceCommunicator DomainServiceCommunicator { get; private set; }

        /// <summary>
        /// Provides a domain aggregate service.
        /// </summary>
        public IProvider<IDomainAggregateService> AggregateServiceProvider { get; private set; }

        /// <summary>
        /// If true, the mesh will be checked at a regular interval for updates. If it is updated and this is true, the file will be updates.
        /// </summary>
        public bool DoAutomaticUpdates { get { return fileAccessor.DoAutomaticUpdates; } set { fileAccessor.DoAutomaticUpdates = value; } }

        private static IParameterizedCreator<MeshDomain, IMeshKey> domainValueKeyCreator =
            ParameterizedCreator.FromSingle<MeshDomain, IMeshKey>(domain =>
            {
                return domain.Key.Clone().SetUniquePart();
            });


        private MemoryMeshServiceProvider memoryProvider;

        private FileAccessor fileAccessor;

        private HularionDataFileSerializer hularionSerializer = new HularionDataFileSerializer();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileAccessor">Manages access to the file.</param>
        public HularionDataFileProvider(FileAccessor fileAccessor)
        {
            this.fileAccessor = fileAccessor;
            this.memoryProvider = new MemoryMeshServiceProvider(StandardLinkForm.LinkKeyFormProvider, domainValueKeyCreator);

            var injectorCommunicator = new InjectorDomainServiceCommunicator(memoryProvider.DomainServiceCommunicator);
            DomainServiceCommunicator = injectorCommunicator;
            injectorCommunicator.ValuesServiceByDomainProvider = ParameterizedProvider.FromSingle<MeshDomain, ServiceResponse<IDomainValueService>>(domain =>
            {
                var memoryService = memoryProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response;
                var service = new InjectorDomainValueService(memoryService);
                service.AffectProcessor = ParameterizedFacade.FromSingle<DomainValueAffectRequest, DomainValueAffectResponse>(request =>
                {
                    var result = memoryService.AffectProcessor.Process(request);
                    fileAccessor.CurrentFileStatus.FileIsUpdated = true;
                    return result;
                });
                return new ServiceResponse<IDomainValueService>() { Request = domain, Response = service };
            });
            injectorCommunicator.DomainCreator.ProcessSuffixes.AddPrefix(() => fileAccessor.CurrentFileStatus.FileIsUpdated = true);

            var aggregateService = new InjectorDomainAggregateService();
            aggregateService.SetMembers(InjectorOverwriteMode.AnyOverAny, memoryProvider.AggregateServiceProvider.Provide());
            aggregateService.AffectProcessor = ParameterizedFacade.FromSingle<DomainAggregateAffectorRequest, DomainAggregateAffectorResponse>(aggregateRequest => {
                var result = memoryProvider.AggregateServiceProvider.Provide().AffectProcessor.Process(aggregateRequest);
                fileAccessor.CurrentFileStatus.FileIsUpdated = true;
                return result;
            });
            AggregateServiceProvider = new ProviderFunction<IDomainAggregateService>(() => aggregateService);


            fileAccessor.FileProvider = new ProviderFunction<string>(() => GetHularionFile(memoryProvider));
        }

        /// <summary>
        /// Loads the file into mesh services.
        /// </summary>
        public void LoadFile()
        {
            //Read or Create file.
            var content = fileAccessor.ReadEntireFile();
            var file = hularionSerializer.Deserialize(content);
            if (file == null)
            {
                file = new MeshServicesFile();
                var serialized = hularionSerializer.Serialize(file);
                fileAccessor.WriteEntireFile(serialized, false);
            }

            var domainProvider = ParameterizedProvider.FromSingle<IMeshKey, MeshDomain>(key => memoryProvider.DomainServiceCommunicator.DomainByKeyProvider.Provide(key).Response);
            memoryProvider.AggregateServiceProvider.Provide().ImportObjectsAndLinks(domainProvider, file.Objects, file.Links);

            //fileAccessor.ProcessUpdates = true;
        }

        /// <summary>
        /// Gets the file content from mesh services.
        /// </summary>
        /// <param name="memoryProvider">The underlying memory provider.</param>
        /// <returns>The file content from mesh services.</returns>
        public string GetHularionFile(MemoryMeshServiceProvider memoryProvider)
        {
            var file = GetFile(memoryProvider);
            return hularionSerializer.Serialize(file);
        }

        /// <summary>
        /// Gets the mesh content from mesh services.
        /// </summary>
        /// <param name="memoryProvider">The underlying memory provider.</param>
        /// <returns>The mesh content.</returns>
        private MeshServicesFile GetFile(MemoryMeshServiceProvider memoryProvider)
        {
            lock (memoryProvider)
            {
                var file = new MeshServicesFile();
                var domains = memoryProvider.DomainServiceCommunicator.AllValueDomainsProvider.Provide().Response.ToList();
                file.Domains = domains.Select(x => new FileMeshDomain(x)).ToList();
                var objects = new List<DomainObject>();
                foreach (var domain in domains)
                {
                    var service = memoryProvider.DomainServiceCommunicator.ValuesServiceByDomainProvider.Provide(domain).Response;
                    var reads = service.QueryProcessor.Process(new DomainValueQueryRequest() { Reads = DomainReadRequest.ReadAll, Where = WhereExpressionNode.ReadAll });
                    objects.AddRange(reads.Values);
                }
                file.Objects = objects;
                var linkDomains = memoryProvider.DomainServiceCommunicator.AllLinkDomainsProvider.Provide().Response;
                var links = new List<DomainLinker>();
                foreach (var domain in linkDomains)
                {
                    var service = memoryProvider.DomainServiceCommunicator.LinkServiceByLinkedDomainsProvider.Provide(domain.GetLinkedDomains()).Response;
                    links.AddRange(service.AllLinksProvider.Provide());
                }
                var uniqueLinks = new Dictionary<IMeshKey, DomainLinker>();
                foreach(var link in links)
                {
                    if (uniqueLinks.ContainsKey(link.DomainKey)) { continue; }
                    uniqueLinks.Add(link.DomainKey, link);
                }
                file.Links = uniqueLinks.Values.ToList();
                return file;
            }
        }

        /// <summary>
        /// Flushes the current state of mesh services to a file.
        /// </summary>
        /// <param name="stopProcessing">if true (default), automatic updates will be turned off.</param>
        public void Flush(bool stopProcessing = true)
        {
            if (stopProcessing) { DoAutomaticUpdates = false; }
            var content = GetHularionFile(this.memoryProvider);
            fileAccessor.WriteEntireFile(content, stopProcessing);
        }

    }
}

#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.Response;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Standard
{
    /// <summary>
    /// Implements IDomainServiceCommunicator as specified by the constructor(s).
    /// </summary>
    public class StandardDomainServiceCommunicator : IDomainServiceCommunicator
    {
        /// <summary>
        /// Creates the given domain.
        /// </summary>
        public IParameterizedFacade<MeshDomain, ServiceResponse> DomainCreator { get; private set; }
        /// <summary>
        /// Updates the given domain.
        /// </summary>
        public IParameterizedFacade<MeshDomain, ServiceResponse> DomainUpdater { get; private set; }
        /// <summary>
        /// Deletes the domain with the given key.
        /// </summary>
        public IParameterizedFacade<IMeshKey, ServiceResponse> DomainDeleter { get; private set; }
        /// <summary>
        /// Provides the MeshDomain given the domain key.
        /// </summary>
        public IParameterizedProvider<IMeshKey, ServiceResponse<MeshDomain>> DomainByKeyProvider { get; private set; }
        /// <summary>
        /// Provides the MeshDomain given the domain's friendly name.
        /// </summary>
        public IParameterizedProvider<string, ServiceResponse<MeshDomain>> DomainByNameProvider { get; private set; }
        /// <summary>
        /// Provides all the MeshDomains.
        /// </summary>
        public IProvider<ServiceResponse<IEnumerable<MeshDomain>>> AllValueDomainsProvider { get; private set; }
        /// <summary>
        /// Provides all the LinkedDomains.
        /// </summary>
        public IProvider<ServiceResponse<IEnumerable<MeshDomainLink>>> AllLinkDomainsProvider { get; private set; }
        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain.
        /// </summary>
        public IParameterizedProvider<MeshDomain, ServiceResponse<IDomainValueService>> ValuesServiceByDomainProvider { get; private set; }
        /// <summary>
        /// Provides the IDomainValueService given the MeshDomain's key.
        /// </summary>
        public IParameterizedProvider<IMeshKey, ServiceResponse<IDomainValueService>> ValueServiceByDomainKeyProvider { get; private set; }
        /// <summary>
        /// Provides the IDomainLinkService given the LinkedDomains.
        /// </summary>
        public IParameterizedProvider<LinkedDomains, ServiceResponse<IDomainLinkService>> LinkServiceByLinkedDomainsProvider { get; private set; }

        /// <summary>
        /// Implements IDomainServiceCommunicator using an IDomainService.
        /// </summary>
        /// <param name="service">The service that will affect the domains.</param>
        public StandardDomainServiceCommunicator(IDomainService service)
        {
            DomainCreator = ParameterizedFacade.FromSingle<MeshDomain, ServiceResponse>(domain =>
            {
                var response = new ServiceResponse() { Request = domain };
                try
                {
                    service.CreateDomain(domain);
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.DomainCreator encountered an error - [qCvmDur34kigrIyevuOOpQ].\n\n {0}", e.ToString()) });
                }
                return response;
            });
            DomainUpdater = ParameterizedFacade.FromSingle<MeshDomain, ServiceResponse>(domain =>
            {
                var response = new ServiceResponse() { Request = domain };
                try
                {
                    service.UpdateDomain(domain);
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.DomainUpdater encountered an error - [t5Pe6ywgjk20xlW6EtOjIQ].\n\n {0}", e.ToString()) });
                }
                return response;
            });
            DomainDeleter = ParameterizedFacade.FromSingle<IMeshKey, ServiceResponse>(domainKey =>
            {
                var response = new ServiceResponse() { Request = domainKey };
                try
                {
                    service.DeleteDomain(domainKey);
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.DomainDeleter encountered an error - [Dy5R7XeIA0quWW3KwzszHw].\n\n {0}", e.ToString()) });
                }
                return response;
            });
            DomainByKeyProvider = ParameterizedProvider.FromSingle<IMeshKey, ServiceResponse<MeshDomain>>(domainKey =>
            {
                var response = new ServiceResponse<MeshDomain>() { Request = domainKey };
                try
                {
                    response.Response = service.GetDomain(domainKey);
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.DomainByKeyProvider encountered an error - [TgGSH5llf0GcAWhtoH2R8A].\n\n {0}", e.ToString()) });
                }
                return response;
            });
            AllValueDomainsProvider = new ProviderFunction<ServiceResponse<IEnumerable<MeshDomain>>>(() =>
            {
                var response = new ServiceResponse<IEnumerable<MeshDomain>>() { };
                //try
                {
                    response.Response = service.GetAllValueDomains();
                }
                //catch (Exception e)
                //{
                //    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.AllValueDomainsProvider encountered an error - [OEYlMPOntEyFapvSnp2i4Q].\n\n {0}", e.ToString()) });
                //}
                return response;
            });
            AllLinkDomainsProvider = new ProviderFunction<ServiceResponse<IEnumerable<MeshDomainLink>>>(() =>
            {
                var response = new ServiceResponse<IEnumerable<MeshDomainLink>>() { };
                try
                {
                    response.Response = service.GetAllLinkDomains();
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.AllLinkDomainsProvider encountered an error - [TNXa3D6vBkiQcYVOpN1khA].\n\n {0}", e.ToString()) });
                }
                return response;
            });
            ValuesServiceByDomainProvider = ParameterizedProvider.FromSingle<MeshDomain, ServiceResponse<IDomainValueService>>(domain =>
            {
                var response = new ServiceResponse<IDomainValueService>() { Request = domain };
                //try
                {
                    response.Response = service.GetDomainValueService(domain);
                }
                //catch (Exception e)
                //{
                //    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.ServiceByDomainProvider encountered an error - [PN2lVuRRvkKdIoeW4ZcetQ].\n\n {0}", e.ToString()) });
                //}
                return response;
            });
            ValueServiceByDomainKeyProvider = ParameterizedProvider.FromSingle<IMeshKey, ServiceResponse<IDomainValueService>>(domainKey =>
            {
                var response = new ServiceResponse<IDomainValueService>() { Request = domainKey };
                //try
                {
                    response.Response = service.GetDomainValueService(domainKey);
                }
                //catch (Exception e)
                //{
                //    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.ServiceByDomainKeyProvider encountered an error - [9IiiqUsJakGMm5GQQdiVTw].\n\n {0}", e.ToString()) });
                //}
                return response;
            });
            LinkServiceByLinkedDomainsProvider = ParameterizedProvider.FromSingle<LinkedDomains, ServiceResponse<IDomainLinkService>>(linkedDomains =>
            {
                var response = new ServiceResponse<IDomainLinkService>() { Request = linkedDomains };
                try
                {
                    response.Response = service.GetDomainLinkService(linkedDomains);
                }
                catch (Exception e)
                {
                    response.Messages.Add(new ServiceResponseMessage() { IsError = true, Message = String.Format("StandardDomainServiceCommunicator.LinkServiceByLinkedDomainsProvider encountered an error - [oe5QesnJ8UW8BNMsZMKdbw].\n\n {0}", e.ToString()) });
                }
                return response;
            });
        }

    }
}

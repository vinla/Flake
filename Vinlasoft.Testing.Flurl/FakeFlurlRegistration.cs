using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Flurl.Http.Configuration;
using Moq;

namespace AcmeSite.Tests.Faking
{
    public static class FakeFlurlRegistration
    {        
        public static Mock<IFlurlClientFactory> UseFakeApis(this IServiceCollection container)
        {
            var fakeFlurlFactory = new Mock<IFlurlClientFactory>();
            container.AddSingleton<IFlurlClientFactory>(fakeFlurlFactory.Object);
            return fakeFlurlFactory;
        }

        public static void RegisterFakeApi(this Mock<IFlurlClientFactory> fakeFactory, FakeFlurlClient fakeClient)
        {            
            fakeFactory.Setup(x => x.Get(It.Is<Flurl.Url>(url => url.ToString() == fakeClient.BaseUrl))).Returns(fakeClient);
        }
    }
}

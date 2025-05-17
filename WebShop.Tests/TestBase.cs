using Ninject;
using NUnit.Framework;
using WebShop.Infrastructure;

namespace WebShopBLL.Tests
{
    public abstract class TestBase
    {
        protected IKernel Kernel;

        [SetUp]
        public void SetUp()
        {
            Kernel = new StandardKernel(new ServiceModule());
        }

        [TearDown]
        public void TearDown()
        {
            Kernel.Dispose();
        }

        protected void Rebind<TInterface, TImplementation>() where TImplementation : TInterface
        {
            Kernel.Rebind<TInterface>().To<TImplementation>();
        }

        protected void Rebind<TInterface>(TInterface instance)
        {
            Kernel.Rebind<TInterface>().ToConstant(instance);
        }
    }
}
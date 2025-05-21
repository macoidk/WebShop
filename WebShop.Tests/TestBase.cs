using AutoFixture;
using Ninject;
using NSubstitute;
using NUnit.Framework;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.Interfaces;

namespace WebShop.Tests
{
    public class TestBase
    {
        protected IKernel Kernel { get; private set; }
        protected IUnitOfWork UnitOfWork { get; private set; }
        protected Fixture Fixture { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            Kernel = new StandardKernel();
            UnitOfWork = Substitute.For<IUnitOfWork>();
            Fixture = new Fixture();

            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Kernel.Bind<IUnitOfWork>().ToConstant(UnitOfWork);
        }

        protected void Rebind<T>(T instance)
        {
            Kernel.Rebind<T>().ToConstant(instance);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Kernel?.Dispose();
        }
    }
}
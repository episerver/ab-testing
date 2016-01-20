using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EPiServer.Marketing.Multivariate.Messaging
{
    /// <summary>
    /// The message handler simply handles the messages and passes them on the registered
    /// IMultivariateTestRepository which in turn handles the cache and database layer.
    /// </summary>
    class MultiVariateMessageHandler : IMultiVariateMessageHandler
    {
        private IServiceLocator _serviceLocator;
        internal IRepository _repository;

        [ExcludeFromCodeCoverage]
        public MultiVariateMessageHandler(IRepository repository)
        {
            _serviceLocator = ServiceLocator.Current;
            _repository = repository;
        }

        /// <summary>
        /// Used specifically for unit tests.
        /// </summary>
        /// <param name="locator"></param>
        internal MultiVariateMessageHandler(IServiceLocator locator)
        {
            _serviceLocator = locator;
            _repository = locator.GetInstance<IRepository>();
        }

        public void Handle(UpdateViewsMessage message)
        {
            IncrementCount(message.TestId, message.VariantId, CountType.View);
        }

        public void Handle(UpdateConversionsMessage message)
        {
            IncrementCount(message.TestId, message.VariantId, CountType.Conversion);
        }

        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var test = _repository.GetById(testId);
            var result = test.MultivariateTestResults.FirstOrDefault(v => v.ItemId == testItemId);

            if (resultType == CountType.View)
            {
                result.Views++;
            }
            else
            {
                result.Conversions++;
            }

            _repository.Save(test);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using AgentR.Server;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;

namespace UnitTests
{
    public class ServerTests
    {
        IHubContext<AgentHub> hub;
        IRequestCallbackCordinator storage;
        IClientProxy clientProxy;

        [SetUp]
        public void Setup()
        {
            hub = A.Fake<IHubContext<AgentHub>>();
            storage = A.Fake<IRequestCallbackCordinator>();
            clientProxy = A.Fake<IClientProxy>();

            A.CallTo(() => hub.Clients.Group(A<string>.Ignored)).Returns(clientProxy);
        }

        [Test(Description = "Client completes request successfully", TestOf = typeof(AgentHandler<,>)), Timeout(500)]
        public async Task TestAgentHandlerSuccess()
        { 
            // Arrange
            var handler = new AgentHandler<TestRequest, Unit>(hub, storage);

            var request = new TestRequest();

            SendingARequestToTheClientWillBeSuccessfull();

            TheRequestWillBeAccceptedByTheClient();

            TheClientWillReturnAResultForTheRequest(request, Unit.Value);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.AreEqual(Unit.Value, result);

            TheServerShouldHaveAllocatedACallbackForTheClient();

            TheServerShouldHaveSentTheRequestToTheClient();

            TheServerShouldHaveCheckedTheReuestWasAccepted();
        }

        class TestRequest : IRequest
        {

        }

        #region Setup Helpers

        FakeItEasy.Configuration.IReturnValueArgumentValidationConfiguration<Task> ClientProxySendRequest => A.CallTo(() => clientProxy.SendCoreAsync(A<string>.Ignored, A<object[]>.Ignored, CancellationToken.None));
        FakeItEasy.Configuration.IReturnValueArgumentValidationConfiguration<Task<bool>> StorageIsAccepted => A.CallTo(() => storage.IsAccepted(A<int>.Ignored));
        FakeItEasy.Configuration.IReturnValueArgumentValidationConfiguration<Task<int>> ConfigureCallbackFor<TRequest, TResponse>(TRequest request) => A.CallTo(() => storage.CreateCallback(request, A<TaskCompletionSource<Unit>>.Ignored));

        void ConfigureCompletionFor<TRequest, TResponse>(TRequest request, Action<TaskCompletionSource<TResponse>> callback) => ConfigureCallbackFor<TRequest, TResponse>(request).ReturnsLazily(c => {
            var completionSource = c.Arguments.Get<TaskCompletionSource<TResponse>>(1);

            callback(completionSource);

            return Task.FromResult(0);
        });

        void SendingARequestToTheClientWillBeSuccessfull() => ClientProxySendRequest.Returns(Task.CompletedTask);

        void TheRequestWillBeAccceptedByTheClient() => StorageIsAccepted.Returns(Task.FromResult(true));

        void TheClientWillReturnAResultForTheRequest<TRequest, TResponse>(TRequest request, TResponse response) =>
             ConfigureCompletionFor<TRequest, TResponse>(request, c => c.SetResult(response));

        void WhenTheClientWillReturnAnExceptionForTheRequest<TRequest, TResponse>(TRequest request, Exception ex) =>
            ConfigureCompletionFor<TRequest, TResponse>(request, c => c.SetException(ex));

        void TheServerShouldHaveSentTheRequestToTheClient() => ClientProxySendRequest.MustHaveHappened();

        void TheServerShouldHaveCheckedTheReuestWasAccepted() => StorageIsAccepted.MustHaveHappened();

        void TheServerShouldHaveAllocatedACallbackForTheClient() => ConfigureCallbackFor<TestRequest, Unit>(request).MustHaveHappened();
        #endregion
    }
}
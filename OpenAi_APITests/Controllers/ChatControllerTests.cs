using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using OpenAI.ObjectModels.SharedModels;

namespace OpenAi_API.Controllers.Tests
{
    [TestClass()]
    public class ChatControllerTests
    {
        static CancellationTokenSource _cancellationToken = new();
        [TestMethod()]
        public void _CancelOperationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task Get_TestAsync()
        {
            //// Arrange
            //var mockService = new Mock<IOpenAIService>();
            //var mockLogger = new Mock<ILogger<ChatController>>();
            //var expectedResponse = new ChatCompletionCreateResponse();
            //var userId = "testUser";
            //var str=new StringBuilder();

            //mockService.Setup(service => service.ChatCompletion.CreateCompletionAsStream(It.IsAny<ChatCompletionCreateRequest>(), null, It.IsAny<CancellationToken>()))
            //    .Returns(FetchItems());

            //var controller = new ChatController(mockService.Object, mockLogger.Object);

            //// Act
            //var results = await controller.Get_(userId);
            //_cancellationToken=controller.GetCancellationToken(userId);
            //Task.Factory.StartNew(async () =>
            //{
            //    await Task.Delay(500);
            //    controller.CancelOperation(userId);
            //});
            //await foreach (var result in results)
            //{
            //    str.Append(result.Choices.First().Message.Content);
            //}

            //// Assert
            //Assert.AreEqual(20, str.Length);
        }

        private static async IAsyncEnumerable<ChatCompletionCreateResponse> FetchItems()
        {
            for (int i = 1; i <= 10; i++)
            {
                if(_cancellationToken.Token.IsCancellationRequested)yield break;
                await Task.Delay(100);
                yield return ConstantResponse();
            }
        }
        private static ChatCompletionCreateResponse ConstantResponse()
        {
            return new ChatCompletionCreateResponse()
            {
                Choices =
                [
                    new ChatChoiceResponse()
                    {
                        Message = new ChatMessage()
                        {
                            Content = "Test"
                        }
                    }
                ],
                Id = "Test",
                Model = "Test",
                ObjectTypeName = "Test",
                HttpStatusCode = HttpStatusCode.OK
            };
        }

    }
}
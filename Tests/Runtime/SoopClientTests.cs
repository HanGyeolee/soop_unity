using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SoopExtension;

namespace SoopExtension.Tests
{
    public class SoopClientTests
    {
        private SoopClient client;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestSoopClient");
            client = go.AddComponent<SoopClient>();
        }

        [TearDown]
        public void TearDown()
        {
            if (client != null)
            {
                Object.DestroyImmediate(client.gameObject);
            }
        }

        [Test]
        public void SoopClient_Initialization_Success()
        {
            Assert.IsNotNull(client);
            Assert.IsNotNull(client.Options);
            Assert.IsNotNull(client.Auth);
            Assert.IsNotNull(client.Live);
            Assert.IsNotNull(client.Channel);
        }

        [Test]
        public void SoopClient_DefaultOptions_AreSet()
        {
            Assert.IsNotNull(client.Options.baseUrls);
            Assert.IsFalse(string.IsNullOrEmpty(client.Options.userAgent));
            Assert.AreEqual(SoopConstants.DEFAULT_USER_AGENT, client.Options.userAgent);
        }

        [Test]
        public void SoopClient_CreateChat_ReturnsValidInstance()
        {
            var chatOptions = new SoopChatOptions
            {
                streamerId = "testStreamer"
            };

            var chat = client.CreateChat(chatOptions);

            Assert.IsNotNull(chat);
        }

        [UnityTest]
        public IEnumerator SoopClient_SendWebRequest_GET_Success()
        {
            bool requestCompleted = false;
            string result = null;
            string error = null;

            client.SendWebRequest("https://httpbin.org/get",
                onSuccess: (response) =>
                {
                    result = response;
                    requestCompleted = true;
                },
                onError: (err) =>
                {
                    error = err;
                    requestCompleted = true;
                });

            yield return new WaitUntil(() => requestCompleted);

            Assert.IsNull(error, $"Request failed with error: {error}");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("httpbin.org"));
        }
    }
}

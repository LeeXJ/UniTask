using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Cysharp.Threading.TasksTests
{
    public class AsyncOperationTest
    {
        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator ResourcesLoad_Completed() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 异步加载资源
            var asyncOperation = Resources.LoadAsync<Texture2D>("sample_texture");
            // 将异步操作转换为UniTask以便在异步测试中使用
            await asyncOperation.ToUniTask();
            // 断言异步操作是否完成
            asyncOperation.isDone.Should().BeTrue();
            // 断言加载的资源类型是否为Texture2D
            asyncOperation.asset.GetType().Should().Be(typeof(Texture2D));
        });
        
        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator ResourcesLoad_CancelOnPlayerLoop() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 创建一个取消标记源
            var cts = new CancellationTokenSource();
            // 使用取消标记在后台线程上异步加载资源，但不立即取消任务
            var task = Resources.LoadAsync<Texture>("sample_texture").ToUniTask(cancellationToken: cts.Token, cancelImmediately: false);
            
            // 取消任务
            cts.Cancel();
            // 断言任务状态是否为Pending
            task.Status.Should().Be(UniTaskStatus.Pending);

            // 等待一帧
            await UniTask.NextFrame();
            // 断言任务状态是否为Canceled
            task.Status.Should().Be(UniTaskStatus.Canceled);
        });

        [Test]
        // 使用Test标记，表示这是一个测试方法
        public void ResourcesLoad_CancelImmediately()
        {
            {
                // 创建一个取消标记源
                var cts = new CancellationTokenSource();
                // 使用取消标记在后台线程上异步加载资源，并立即取消任务
                var task = Resources.LoadAsync<Texture>("sample_texture").ToUniTask(cancellationToken: cts.Token, cancelImmediately: true);

                // 取消任务
                cts.Cancel();
                // 断言任务状态是否为Canceled
                task.Status.Should().Be(UniTaskStatus.Canceled);
            }
        }

#if ENABLE_UNITYWEBREQUEST && (!UNITY_2019_1_OR_NEWER || UNITASK_WEBREQUEST_SUPPORT)
        [UnityTest]
        public IEnumerator UnityWebRequest_Completed() => UniTask.ToCoroutine(async () =>
        {
            var filePath = System.IO.Path.Combine(Application.dataPath, "Tests", "Resources", "sample_texture.png");
            var asyncOperation = UnityWebRequest.Get($"file://{filePath}").SendWebRequest();
            await asyncOperation.ToUniTask();

            asyncOperation.isDone.Should().BeTrue();
            asyncOperation.webRequest.result.Should().Be(UnityWebRequest.Result.Success);
        });
        
        [UnityTest]
        public IEnumerator UnityWebRequest_CancelOnPlayerLoop() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            var filePath = System.IO.Path.Combine(Application.dataPath, "Tests", "Resources", "sample_texture.png");
            var task = UnityWebRequest.Get($"file://{filePath}").SendWebRequest().ToUniTask(cancellationToken: cts.Token);
            
            cts.Cancel();
            task.Status.Should().Be(UniTaskStatus.Pending);
            
            await UniTask.NextFrame();
            task.Status.Should().Be(UniTaskStatus.Canceled);
        });
        
        [Test]
        public void UnityWebRequest_CancelImmediately()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var filePath = System.IO.Path.Combine(Application.dataPath, "Tests", "Resources", "sample_texture.png");
            var task = UnityWebRequest.Get($"file://{filePath}").SendWebRequest().ToUniTask(cancellationToken: cts.Token, cancelImmediately: true);
            
            task.Status.Should().Be(UniTaskStatus.Canceled);
        }
#endif
    }
}
#endif

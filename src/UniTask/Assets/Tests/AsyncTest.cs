﻿#if !(UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Scripting;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
using System.Threading.Tasks;
#endif
using UnityEngine.Networking;

#if !UNITY_2019_3_OR_NEWER
using UnityEngine.Experimental.LowLevel;
#else
using UnityEngine.LowLevel;
#endif

#if !UNITY_WSA
using Unity.Jobs;
#endif
using Unity.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine.TestTools;
using FluentAssertions;

namespace Cysharp.Threading.TasksTests
{
    public class AsyncTest
    {
#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
#if !UNITY_WSA

        public struct MyJob : IJob
        {
            // 定义结构体字段

            // 循环次数
            public int loopCount;
            // 用于传入和传出数据的 NativeArray
            public NativeArray<int> inOut;
            // 结果值
            public int result;

            // 实现 IJob 接口的 Execute 方法，用于执行并行计算任务
            public void Execute()
            {
                // 初始化结果值为0
                result = 0;

                // 循环执行一定次数，递增结果值
                for (int i = 0; i < loopCount; i++)
                {
                    result++;
                }

                // 将结果值写入传入传出的 NativeArray 中的第一个元素
                inOut[0] = result;
            }
        }

#if !UNITY_WEBGL

        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator DelayAnd() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 在下一个 PostLateUpdate 阶段暂停执行，等待渲染帧结束
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            // 记录开始时间
            var time = Time.realtimeSinceStartup;

            // 缩小时间缩放系数以减慢时间流逝
            Time.timeScale = 0.5f;
            try
            {
                // 等待3秒钟
                await UniTask.Delay(TimeSpan.FromSeconds(3));

                // 计算经过的时间
                var elapsed = Time.realtimeSinceStartup - time;
                
                // 断言实际经过的时间是否为6秒钟
                // 这里使用了 Math.Round 方法将经过的时间四舍五入到最接近的整数秒，并将其与6进行比较
                ((int)Math.Round(TimeSpan.FromSeconds(elapsed).TotalSeconds, MidpointRounding.ToEven)).Should().Be(6);
            }
            finally
            {
                // 恢复时间缩放系数为正常值
                Time.timeScale = 1.0f;
            }
        });

#endif

        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator DelayIgnore() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 记录开始时间
            var time = Time.realtimeSinceStartup;

            // 缩小时间缩放系数以减慢时间流逝
            Time.timeScale = 0.5f;
            try
            {
                // 等待3秒钟，忽略时间缩放
                await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: true);

                // 计算经过的时间
                var elapsed = Time.realtimeSinceStartup - time;

                // 断言实际经过的时间是否为3秒钟
                // 这里使用了 Math.Round 方法将经过的时间四舍五入到最接近的整数秒，并将其与3进行比较
                ((int)Math.Round(TimeSpan.FromSeconds(elapsed).TotalSeconds, MidpointRounding.ToEven)).Should().Be(3);
            }
            finally
            {
                // 恢复时间缩放系数为正常值
                Time.timeScale = 1.0f;
            }
        });

        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator WhenAll() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 创建三个不同的UniTask，分别表示任务a、b、c
            var a = UniTask.FromResult(999); // 创建一个已完成的UniTask，结果为999
            var b = UniTask.Yield(PlayerLoopTiming.Update, CancellationToken.None).AsAsyncUnitUniTask(); // 创建一个在Update阶段暂停的UniTask
            var c = UniTask.DelayFrame(99).AsAsyncUnitUniTask(); // 创建一个在99帧后完成的UniTask

            // 使用UniTask.WhenAll同时等待多个任务的完成
            var (a2, b2, c2) = await UniTask.WhenAll(a, b, c);

            // 断言每个任务的结果是否符合预期
            a2.Should().Be(999); // 任务a的结果应为999
            b2.Should().Be(AsyncUnit.Default); // 任务b应为已完成状态
            c2.Should().Be(AsyncUnit.Default); // 任务c应为已完成状态
        });

        [UnityTest]
        // 使用UnityTest标记，表示这是一个Unity测试方法
        public IEnumerator WhenAny() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            // 创建三个不同的UniTask，分别表示任务a、b、c
            var a = UniTask.FromResult(999); // 创建一个已完成的UniTask，结果为999
            var b = UniTask.Yield(PlayerLoopTiming.Update, CancellationToken.None).AsAsyncUnitUniTask(); // 创建一个在Update阶段暂停的UniTask
            var c = UniTask.DelayFrame(99).AsAsyncUnitUniTask(); // 创建一个在99帧后完成的UniTask

            // 使用UniTask.WhenAny等待其中任意一个任务完成
            var (win, a2, b2, c2) = await UniTask.WhenAny(a, b, c);

            // 断言返回的结果是否符合预期
            win.Should().Be(0); // win应该为0，表示任务a完成
            a2.Should().Be(999); // 任务a的结果应为999
        });

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator BothEnumeratorCheck() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            await ToaruCoroutineEnumerator(); // 调用 ToaruCoroutineEnumerator 方法，等待其完成
        });

        //[UnityTest]
        //public IEnumerator JobSystem() => UniTask.ToCoroutine(async () =>
        //{
        //    var job = new MyJob() { loopCount = 999, inOut = new NativeArray<int>(1, Allocator.TempJob) };
        //    JobHandle.ScheduleBatchedJobs();
        //    await job.Schedule();
        //    job.inOut[0].Should().Be(999);
        //    job.inOut.Dispose();
        //});

        class MyMyClass
        {
            public int MyProperty { get; set; }
        }

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator WaitUntil() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            bool t = false;

            // 延迟10帧后设置t为true
            UniTask.DelayFrame(10, PlayerLoopTiming.PostLateUpdate).ContinueWith(() => t = true).Forget();

            // 记录开始帧数
            var startFrame = Time.frameCount;
            // 等待直到 t 变为 true，等待期间检查 EarlyUpdate 阶段
            await UniTask.WaitUntil(() => t, PlayerLoopTiming.EarlyUpdate);

            // 计算帧数差异
            var diff = Time.frameCount - startFrame;
            // 断言帧数差异是否为11
            diff.Should().Be(11);
        });

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator WaitWhile() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            bool t = true;

            // 延迟10帧后将 t 设置为 false
            UniTask.DelayFrame(10, PlayerLoopTiming.PostLateUpdate).ContinueWith(() => t = false).Forget();

            // 记录开始帧数
            var startFrame = Time.frameCount;
            // 等待直到 t 变为 false，等待期间检查 EarlyUpdate 阶段
            await UniTask.WaitWhile(() => t, PlayerLoopTiming.EarlyUpdate);

            // 计算帧数差异
            var diff = Time.frameCount - startFrame;
            // 断言帧数差异是否为11
            diff.Should().Be(11);
        });

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator WaitUntilValueChanged() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            var v = new MyMyClass { MyProperty = 99 }; // 创建一个 MyMyClass 实例，并设置 MyProperty 初始值为 99

            // 延迟10帧后将 MyProperty 设置为 1000
            UniTask.DelayFrame(10, PlayerLoopTiming.PostLateUpdate).ContinueWith(() => v.MyProperty = 1000).Forget();

            // 记录开始帧数
            var startFrame = Time.frameCount;

            // 等待 MyProperty 的值发生改变，等待期间检查 EarlyUpdate 阶段
            await UniTask.WaitUntilValueChanged(v, x => x.MyProperty, PlayerLoopTiming.EarlyUpdate);

            // 计算帧数差异
            var diff = Time.frameCount - startFrame;
            // 断言帧数差异是否为11
            diff.Should().Be(11);
        });

#if !UNITY_WEBGL

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator SwitchTo() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            await UniTask.Yield(); // 等待一帧，确保在主线程执行

            var currentThreadId = Thread.CurrentThread.ManagedThreadId; // 记录当前线程的 ID

            await UniTask.SwitchToThreadPool(); // 切换到线程池执行任务

            var switchedThreadId = Thread.CurrentThread.ManagedThreadId; // 记录切换后的线程 ID

            currentThreadId.Should().NotBe(switchedThreadId); // 断言当前线程和切换后的线程不一致

            await UniTask.Yield(); // 再次等待一帧，确保任务执行回到了主线程

            var switchedThreadId2 = Thread.CurrentThread.ManagedThreadId; // 再次记录当前线程的 ID

            currentThreadId.Should().Be(switchedThreadId2); // 断言当前线程和切换后的线程一致
        });

#endif

        //[UnityTest]
        //public IEnumerator ObservableConversion() => UniTask.ToCoroutine(async () =>
        //{
        //    var v = await Observable.Range(1, 10).ToUniTask();
        //    v.Is(10);

        //    v = await Observable.Range(1, 10).ToUniTask(useFirstValue: true);
        //    v.Is(1);

        //    v = await UniTask.DelayFrame(10).ToObservable().ToTask();
        //    v.Is(10);

        //    v = await UniTask.FromResult(99).ToObservable();
        //    v.Is(99);
        //});

        //[UnityTest]
        //public IEnumerator AwaitableReactiveProperty() => UniTask.ToCoroutine(async () =>
        //{
        //    var rp1 = new ReactiveProperty<int>(99);

        //    UniTask.DelayFrame(100).ContinueWith(x => rp1.Value = x).Forget();

        //    await rp1;

        //    rp1.Value.Is(100);

        //    // var delay2 = UniTask.DelayFrame(10);
        //    // var (a, b ) = await UniTask.WhenAll(rp1.WaitUntilValueChangedAsync(), delay2);

        //});

        //[UnityTest]
        //public IEnumerator AwaitableReactiveCommand() => UniTask.ToCoroutine(async () =>
        //{
        //    var rc = new ReactiveCommand<int>();

        //    UniTask.DelayFrame(100).ContinueWith(x => rc.Execute(x)).Forget();

        //    var v = await rc;

        //    v.Is(100);
        //});

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator ExceptionlessCancellation() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            var cts = new CancellationTokenSource(); // 创建 CancellationTokenSource 对象

            // 延迟10帧后取消 CancellationTokenSource 对象
            UniTask.DelayFrame(10).ContinueWith(() => cts.Cancel()).Forget();

            var first = Time.frameCount; // 记录当前帧数
            // 等待100帧，同时传入 CancellationToken，但忽略取消异常的抛出
            var canceled = await UniTask.DelayFrame(100, cancellationToken: cts.Token).SuppressCancellationThrow();

            var r = (Time.frameCount - first); // 计算帧数差
            // 断言差值在9到11之间
            (9 < r && r < 11).Should().BeTrue();
            // 断言任务是否被取消
            canceled.Should().Be(true);
        });

        [UnityTest]
        // 使用 UnityTest 标记，表示这是一个 Unity 测试方法
        public IEnumerator ExceptionCancellation() => UniTask.ToCoroutine(async () =>
        {
            // 在异步方法中执行测试
            var cts = new CancellationTokenSource(); // 创建 CancellationTokenSource 对象

            // 延迟10帧后取消 CancellationTokenSource 对象
            UniTask.DelayFrame(10).ContinueWith(() => cts.Cancel()).Forget();

            bool occur = false;
            try
            {
                // 使用 UniTask.DelayFrame 方法等待100帧，同时传入 CancellationToken
                await UniTask.DelayFrame(100, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                occur = true;
            }
            // 断言是否捕获到 OperationCanceledException 异常
            occur.Should().BeTrue();
        });

        IEnumerator ToaruCoroutineEnumerator()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

        //[UnityTest]
        //public IEnumerator ExceptionUnobserved1() => UniTask.ToCoroutine(async () =>
        //{
        //    bool calledEx = false;
        //    Action<Exception> action = exx =>
        //    {
        //        calledEx = true;
        //        exx.Message.Should().Be("MyException");
        //    };

        //    UniTaskScheduler.UnobservedTaskException += action;

        //    var ex = InException1();
        //    ex = default(UniTask);

        //    await UniTask.DelayFrame(3);

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    GC.Collect();

        //    await UniTask.DelayFrame(1);

        //    calledEx.Should().BeTrue();

        //    UniTaskScheduler.UnobservedTaskException -= action;
        //});

        [UnityTest]
        public IEnumerator ExceptionUnobserved2() => UniTask.ToCoroutine(async () =>
        {
            bool calledEx = false;
            Action<Exception> action = exx =>
            {
                calledEx = true;
                exx.Message.Should().Be("MyException");
            };

            UniTaskScheduler.UnobservedTaskException += action;

            var ex = InException2();
            ex = default(UniTask<int>);

            await UniTask.DelayFrame(3);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            await UniTask.DelayFrame(1);

            calledEx.Should().BeTrue();

            UniTaskScheduler.UnobservedTaskException -= action;
        });

        // can not run on RuntimeUnitTestToolkit so ignore...
        //        [UnityTest]
        //        public IEnumerator ThrowExceptionUnawaited() => UniTask.ToCoroutine(async () =>
        //        {
        //            LogAssert.Expect(LogType.Exception, "Exception: MyException");

        //#pragma warning disable 1998
        //            async UniTask Throw() => throw new Exception("MyException");
        //#pragma warning restore 1998

        //#pragma warning disable 4014
        //            Throw();
        //#pragma warning restore 4014

        //            await UniTask.DelayFrame(3);

        //            GC.Collect();
        //            GC.WaitForPendingFinalizers();
        //            GC.Collect();

        //            await UniTask.DelayFrame(1);
        //        });

        async UniTask InException1()
        {
            await UniTask.Yield();
            throw new Exception("MyException");
        }

        async UniTask<int> InException2()
        {
            await UniTask.Yield();
            throw new Exception("MyException");
        }

        [UnityTest]
        public IEnumerator NextFrame1() => UniTask.ToCoroutine(async () =>
        {
            await UniTask.Yield(PlayerLoopTiming.LastUpdate);
            var frame = Time.frameCount;
            await UniTask.NextFrame();
            Time.frameCount.Should().Be(frame + 1);
        });

        [UnityTest]
        public IEnumerator NextFrame2() => UniTask.ToCoroutine(async () =>
        {
            await UniTask.Yield(PlayerLoopTiming.PreUpdate);
            var frame = Time.frameCount;
            await UniTask.NextFrame();
            Time.frameCount.Should().Be(frame + 1);
        });

        [UnityTest]
        public IEnumerator NestedEnumerator() => UniTask.ToCoroutine(async () =>
        {
            var time = Time.realtimeSinceStartup;

            await ParentCoroutineEnumerator();

            var elapsed = Time.realtimeSinceStartup - time;
            ((int)Math.Round(TimeSpan.FromSeconds(elapsed).TotalSeconds, MidpointRounding.ToEven)).Should().Be(3);
        });

        IEnumerator ParentCoroutineEnumerator()
        {
            yield return ChildCoroutineEnumerator();
        }

        IEnumerator ChildCoroutineEnumerator()
        {
            yield return new WaitForSeconds(3);
        }

        [UnityTest]
        public IEnumerator ToObservable() => UniTask.ToCoroutine(async () =>
        {
            var completedTaskObserver = new ToObservableObserver<AsyncUnit>();
            completedTaskObserver.OnNextCalled.Should().BeFalse();
            completedTaskObserver.OnCompletedCalled.Should().BeFalse();
            completedTaskObserver.OnErrorCalled.Should().BeFalse();
            UniTask.CompletedTask.ToObservable().Subscribe(completedTaskObserver);
            completedTaskObserver.OnNextCalled.Should().BeTrue();
            completedTaskObserver.OnCompletedCalled.Should().BeTrue();
            completedTaskObserver.OnErrorCalled.Should().BeFalse();

            var delayFrameTaskObserver = new ToObservableObserver<AsyncUnit>();
            UniTask.DelayFrame(1).ToObservable().Subscribe(delayFrameTaskObserver);
            delayFrameTaskObserver.OnNextCalled.Should().BeFalse();
            delayFrameTaskObserver.OnCompletedCalled.Should().BeFalse();
            delayFrameTaskObserver.OnErrorCalled.Should().BeFalse();
            await UniTask.DelayFrame(1);
            delayFrameTaskObserver.OnNextCalled.Should().BeTrue();
            delayFrameTaskObserver.OnCompletedCalled.Should().BeTrue();
            delayFrameTaskObserver.OnErrorCalled.Should().BeFalse();
        });

        class ToObservableObserver<T> : IObserver<T>
        {
            public bool OnNextCalled { get; private set; }
            public bool OnCompletedCalled { get; private set; }
            public bool OnErrorCalled { get; private set; }

            public void OnNext(T value) => OnNextCalled = true;
            public void OnCompleted() => OnCompletedCalled = true;
            public void OnError(Exception error) => OnErrorCalled = true;
        }


#endif
#endif
    }
}

#endif
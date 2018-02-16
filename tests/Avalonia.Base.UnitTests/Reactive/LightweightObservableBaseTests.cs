using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using Avalonia.Reactive;
using Xunit;

namespace Avalonia.Base.UnitTests.Reactive
{
    public class LightweightObservableBaseTests
    {
        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Subscriber_Is_Notified_Of_Value(ISubject<string> target)
        {
            var result = new List<string>();

            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            target.OnNext("bar");

            Assert.Equal(new[] { "foo", "bar" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Subscriber_Is_Notified_Of_Completion(ISubject<string> target)
        {
            var result = new List<string>();

            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            target.OnNext("bar");
            target.OnCompleted();

            Assert.Equal(new[] { "foo", "bar", "completed" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Subscriber_Is_Notified_Of_Error(ISubject<string> target)
        {
            var result = new List<string>();

            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            target.OnNext("bar");
            target.OnError(new Exception("error"));

            Assert.Equal(new[] { "foo", "bar", "error" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Subscribing_After_Completion_Notifies_Completion(ISubject<string> target)
        {
            var result = new List<string>();

            target.OnCompleted();
            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            Assert.Equal(new[] { "completed" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Subscribing_After_Error_Notifies_Error(ISubject<string> target)
        {
            var result = new List<string>();

            target.OnError(new Exception("error"));
            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            Assert.Equal(new[] { "error" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Disposing_Subscription_Stops_Notifications(ISubject<string> target)
        {
            var result = new List<string>();

            var subscription = target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            subscription.Dispose();
            target.OnNext("bar");
            target.OnError(new Exception("error"));

            Assert.Equal(new[] { "foo" }, result);
        }

        [Theory]
        [MemberData(nameof(GetTargets))]
        public void Disposing_Later_Subscription_Stops_Notification_Of_Next_Value(ISubject<string> target)
        {
            var result = new List<string>();
            IDisposable subscription2 = null;

            var subscription1 = target.Subscribe(
                x => { result.Add(x); subscription2.Dispose(); },
                ex => result.Add(ex.Message),
                () => result.Add("completed"));
            subscription2 = target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            target.OnNext("bar");

            Assert.Equal(new[] { "foo", "foo", "bar" }, result);
        }

        public static IEnumerable<object[]> GetTargets()
        {
            yield return new[] { new Subject<string>() };
            yield return new[] { new TestSubject() };
        }

        private class TestSubject : LightweightObservableBase<string>, ISubject<string>
        {
            public void OnNext(string value) => PublishNext(value);
            public void OnCompleted() => PublishCompleted();
            public void OnError(Exception ex) => PublishError(ex);

            protected override void Initialize()
            {
            }

            protected override void Deinitialize()
            {
            }
        }
    }
}

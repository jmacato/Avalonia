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
        [Fact]
        public void Subscriber_Is_Notified_Of_Value()
        {
            var target = new TestSubject();
            var result = new List<string>();

            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            target.OnNext("foo");
            target.OnNext("bar");

            Assert.Equal(new[] { "foo", "bar" }, result);
        }

        [Fact]
        public void Subscriber_Is_Notified_Of_Completion()
        {
            var target = new TestSubject();
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

        [Fact]
        public void Subscriber_Is_Notified_Of_Error()
        {
            var target = new TestSubject();
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

        [Fact]
        public void Subscribing_After_Completion_Notifies_Completion()
        {
            var target = new TestSubject();
            var result = new List<string>();

            target.OnCompleted();
            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            Assert.Equal(new[] { "completed" }, result);
        }

        [Fact]
        public void Subscribing_After_Error_Notifies_Error()
        {
            var target = new TestSubject();
            var result = new List<string>();

            target.OnError(new Exception("error"));
            target.Subscribe(
                x => result.Add(x),
                ex => result.Add(ex.Message),
                () => result.Add("completed"));

            Assert.Equal(new[] { "error" }, result);
        }

        [Fact]
        public void Disposing_Subscription_Stops_Notifications()
        {
            var target = new TestSubject();
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

        private class TestSubject : LightweightObservableBase<string>
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

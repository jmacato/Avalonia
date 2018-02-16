using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Threading;

namespace Avalonia.Reactive
{
    /// <summary>
    /// Lightweight base class for observable implementations.
    /// </summary>
    /// <typeparam name="T">The observable type.</typeparam>
    /// <remarks>
    /// <see cref="ObservableBase{T}"/> is rather heavyweight in terms of allocations and memory
    /// usage. This class provides a more lightweight base for some internal observable types
    /// in the Avalonia framework.
    /// </remarks>
    public abstract class LightweightObservableBase<T> : IObservable<T>
    {
        private Exception _error;
        private List<IObserver<T>> _observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Contract.Requires<ArgumentNullException>(observer != null);
            Dispatcher.UIThread.VerifyAccess();

            if (_observers == null)
            {
                if (_error != null)
                {
                    observer.OnError(_error);
                }
                else
                {
                    observer.OnCompleted();
                }

                return Disposable.Empty;
            }

            var first = _observers.Count == 0;

            lock (_observers)
            {
                _observers.Add(observer);
            }

            if (first)
            {
                Initialize();
            }

            Subscribed(observer, first);

            return Disposable.Create(() =>
            {
                if (_observers != null)
                {
                    lock (_observers)
                    {
                        _observers?.Remove(observer);

                        if (_observers?.Count == 0)
                        {
                            Deinitialize();
                            _observers.TrimExcess();
                        }
                    }
                }
            });
        }

        protected abstract void Initialize();
        protected abstract void Deinitialize();

        protected void PublishNext(T value)
        {
            if (_observers != null)
            {
                IObserver<T>[] observers = null;
                int count;

                try
                {
                    lock (_observers)
                    {
                        count = _observers.Count;
                        observers = ArrayPool<IObserver<T>>.Shared.Rent(count);
                        _observers.CopyTo(observers);
                    }

                    for (var i = 0; i < count; ++i)
                    {
                        observers[i].OnNext(value);
                    }
                }
                finally
                {
                    ArrayPool<IObserver<T>>.Shared.Return(observers);
                }
            }
        }

        protected void PublishCompleted()
        {
            if (_observers != null)
            {
                IObserver<T>[] observers = null;
                int count;

                try
                {
                    lock (_observers)
                    {
                        count = _observers.Count;
                        observers = ArrayPool<IObserver<T>>.Shared.Rent(count);
                        _observers.CopyTo(observers);
                        _observers = null;
                    }

                    for (var i = 0; i < count; ++i)
                    {
                        observers[i].OnCompleted();
                    }
                }
                finally
                {
                    ArrayPool<IObserver<T>>.Shared.Return(observers);
                }

                Deinitialize();
            }
        }

        protected void PublishError(Exception error)
        {
            if (_observers != null)
            {
                IObserver<T>[] observers = null;
                int count;

                try
                {
                    lock (_observers)
                    {
                        count = _observers.Count;
                        observers = ArrayPool<IObserver<T>>.Shared.Rent(count);
                        _observers.CopyTo(observers);
                        _observers = null;
                    }

                    for (var i = 0; i < count; ++i)
                    {
                        observers[i].OnError(error);
                    }
                }
                finally
                {
                    ArrayPool<IObserver<T>>.Shared.Return(observers);
                }

                _error = error;
                Deinitialize();
            }
        }

        protected virtual void Subscribed(IObserver<T> observer, bool first)
        {
        }
    }
}

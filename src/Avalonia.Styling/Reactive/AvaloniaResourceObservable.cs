using System;
using Avalonia.Controls;

namespace Avalonia.Reactive
{
    internal class AvaloniaResourceObservable : 
        LightweightObservableBase<object>,
        IDescription
    {
        private readonly WeakReference<IResourceNode> _target;
        private readonly string _key;

        public AvaloniaResourceObservable(
            IResourceNode target,
            string key)
        {
            _target = new WeakReference<IResourceNode>(target);
            _key = key;
        }

        public string Description => $"{_target.GetType().Name}.Resources[{_key}]";

        protected override void Initialize()
        {
            if (_target.TryGetTarget(out var target))
            {
                target.ResourcesChanged += ResourcesChanged;
            }
        }

        protected override void Deinitialize()
        {
            if (_target.TryGetTarget(out var target))
            {
                target.ResourcesChanged -= ResourcesChanged;
            }
        }

        protected override void Subscribed(IObserver<object> observer, bool first)
        {
            if (_target.TryGetTarget(out var target))
            {
                observer.OnNext(target.FindResource(_key));
            }
        }

        private void ResourcesChanged(object sender, ResourcesChangedEventArgs e)
        {
            var node = (IResourceNode)sender;
            PublishNext(node.FindResource(_key));
        }
    }
}

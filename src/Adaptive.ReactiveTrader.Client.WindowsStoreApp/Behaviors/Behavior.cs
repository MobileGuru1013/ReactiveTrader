using System.ComponentModel;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Adaptive.ReactiveTrader.Client.Behaviors
{
    public abstract class Behavior<T> : DependencyObject, IBehavior
        where T : DependencyObject
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public T AssociatedObject { get; private set; }

        DependencyObject IBehavior.AssociatedObject
        {
            get { return AssociatedObject; }
        }


        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = (T) associatedObject;
            OnAttached();
        }

        public void Detach()
        {
            OnDetaching();
            AssociatedObject = null;
        }

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetaching()
        {
        }
    }
}

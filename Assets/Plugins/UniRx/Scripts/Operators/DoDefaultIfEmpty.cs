using System;
using UniRx.Operators;

namespace UniRx.Operators
{
    internal class DoDefaultIfEmptyObservable<T> : OperatorObservableBase<T>
    {
        readonly IObservable<T> source;
        readonly Func<T> doDefaultValue;

        public DoDefaultIfEmptyObservable(IObservable<T> source, Func<T> doDefaultValue)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.doDefaultValue = doDefaultValue;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            return source.Subscribe(new DoDefaultIfEmpty(this, observer, cancel));
        }

        class DoDefaultIfEmpty : OperatorObserverBase<T, T>
        {
            readonly DoDefaultIfEmptyObservable<T> parent;
            bool hasValue;

            public DoDefaultIfEmpty(DoDefaultIfEmptyObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
                this.hasValue = false;
            }

            public override void OnNext(T value)
            {
                hasValue = true;
                observer.OnNext(value);
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                if (!hasValue)
                {
                    observer.OnNext(parent.doDefaultValue());
                }

                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }
}
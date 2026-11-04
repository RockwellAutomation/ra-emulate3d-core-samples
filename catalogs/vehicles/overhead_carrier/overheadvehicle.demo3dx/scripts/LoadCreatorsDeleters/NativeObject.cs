using System;

namespace Demo3D.Visuals {
    [Auto] public abstract class NativeObject : IDisposable {
        [Auto] static protected IBuilder app;
        [Auto] static protected Document document;
        [Auto] static protected PrintDelegate print;
        [Auto] static protected VectorDelegate vector;

        protected Visual Visual;

        protected NativeObject() { }
        protected NativeObject(Visual sender) {
            // This will force the binding of all [Auto] members now instead of after the constructor completes
            sender.SetNativeObject(this);
            Visual = sender;
        }

        public virtual void Dispose() { }
    }
}
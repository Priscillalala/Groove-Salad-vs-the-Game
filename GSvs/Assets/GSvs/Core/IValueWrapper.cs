using System;

namespace GSvs.Core
{
    public interface IValueWrapper<out T>
    {
        public T Value { get; }
        public event Action ValueChanged;
    }
}
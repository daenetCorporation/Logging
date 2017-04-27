using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{
    public class EventHubLogScope
    {
        private readonly string m_Name;
        private readonly object m_State;

        internal EventHubLogScope(string name, object state)
        {
            m_Name = name;
            m_State = state;
        }

        public EventHubLogScope Parent { get; private set; }

#if NET451
        private static readonly string FieldKey = $"{typeof(ConsoleLogScope).FullName}.Value.{AppDomain.CurrentDomain.Id}";
        public static ConsoleLogScope Current
        {
            get
            {
                var handle = CallContext.LogicalGetData(FieldKey) as ObjectHandle;
                if (handle == null)
                {
                    return default(ConsoleLogScope);
                }

                return (ConsoleLogScope)handle.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData(FieldKey, new ObjectHandle(value));
            }
        }
#else
        private static AsyncLocal<EventHubLogScope> m_Current = new AsyncLocal<EventHubLogScope>();

        public static EventHubLogScope Current
        {
            set
            {
               
                m_Current.Value = value;
            }
            get
            {
                return m_Current.Value;
            }
        }
#endif

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new EventHubLogScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return m_State?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}

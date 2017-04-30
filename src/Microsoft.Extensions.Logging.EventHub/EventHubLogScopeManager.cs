using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{
    internal class EventHubLogScopeManager
    {
        internal readonly AsyncLocal<List<DisposableScope>> m_AsyncSopes;

        private object m_State;

        internal EventHubLogScopeManager(object state)
        {
            m_AsyncSopes = new AsyncLocal<List<DisposableScope>>();

            m_State = state;
        }

        public string Current
        {
            get
            {
                StringBuilder sb = new StringBuilder(); 
                foreach (var item in this.m_AsyncSopes.Value)
                {
                    sb.Append($"/{item}");
                }

                return sb.ToString();
            }
        }


       // public EventHubLogScope Parent { get; private set; }



        public IDisposable Push(object state)
        {
            lock ("scope")
            {
                string scopeName = Guid.NewGuid().ToString();

                var newScope = new DisposableScope(scopeName, this);

                this.m_AsyncSopes.Value.Add(newScope);

                return newScope;
            }
        }

        public override string ToString()
        {
            return m_State?.ToString();
        }

        internal class DisposableScope : IDisposable
        {
            private EventHubLogScopeManager m_ScopeMgr;

            private string m_ScopeName;

            public DisposableScope(string scopeName, EventHubLogScopeManager scopeMgr)
            {
                this.m_ScopeName = scopeName;
                this.m_ScopeMgr = scopeMgr;
            }

            public void Dispose()
            {
                lock ("scope")
                {
                    var me = m_ScopeMgr.m_AsyncSopes.Value.FirstOrDefault(s => s == this);
                    if (me == null)
                    {
                        throw new InvalidOperationException("This should never happen!");
                    }

                    m_ScopeMgr.m_AsyncSopes.Value.Remove(me);
                }
            }
        }
    }
}

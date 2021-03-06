﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.EventHub
{
    /// <summary>
    /// Handles scopes.
    /// </summary>
    internal class EventHubLogScopeManager
    {
        internal static readonly AsyncLocal<List<DisposableScope>> m_AsyncSopes = new AsyncLocal<List<DisposableScope>>();
        private object m_State;

        internal EventHubLogScopeManager(object state)
        {
            m_State = state;
        }

        public string Current
        {
            get
            {
                StringBuilder sb = new StringBuilder(); 
                foreach (var item in m_AsyncSopes.Value)
                {
                    sb.Append($"/{item}");
                }

                return sb.ToString();
            }
        }

        public IDisposable Push(object state)
        {
            //lock ("scope")
            {
                var newScope = new DisposableScope(state.ToString(), this);

                if(m_AsyncSopes.Value == null)
                    m_AsyncSopes.Value = new List<DisposableScope>();

                m_AsyncSopes.Value.Add(newScope);

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
                m_ScopeName = scopeName;
                m_ScopeMgr = scopeMgr;
            }

            public void Dispose()
            {
                //lock ("scope")
                {
                    var me = m_AsyncSopes.Value.FirstOrDefault(s => s == this);
                    if (me == null)
                    {
                        throw new InvalidOperationException("This should never happen!");
                    }

                    m_AsyncSopes.Value.Remove(me);
                }
            }

            public override string ToString()
            {
                return m_ScopeName;
            }
        }
    }
}

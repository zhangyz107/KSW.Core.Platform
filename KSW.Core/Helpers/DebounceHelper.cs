using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Helpers
{
    public class DebounceHelper
    {
        private DateTime _lastEventTime = DateTime.MinValue;
        private readonly TimeSpan _debounceInterval;

        public DebounceHelper(TimeSpan debounceInterval)
        {
            _debounceInterval = debounceInterval;
        }

        public bool CanProceed()
        {
            var now = DateTime.Now;
            if (now - _lastEventTime < _debounceInterval)
            {
                return false; // 间隔过短，阻止执行
            }
            _lastEventTime = now;
            return true; // 允许执行
        }
    }
}

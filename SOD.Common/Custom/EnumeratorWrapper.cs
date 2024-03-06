using System;
using System.Collections;

namespace SOD.Common.Custom
{
    /// <summary>
    /// A wrapper for enumerators to easily allow patching them.
    /// <br>Usage:</br>
    /// <br>internal static void Postfix(ref IEnumerator __result)</br>
    /// <br>{</br> 
    /// <br>__result = EnumeratorWrapper.Wrap(__result, () => { prefixActionHere }, () => { postfixActionHere });</br>
    /// <br>}</br>
    /// </summary>
    public class EnumeratorWrapper : IEnumerable
    {
        private readonly IEnumerator _enumerator;
        private readonly Action _prefixAction;
        private readonly Action _postfixAction;
        private readonly Action<object> _preYieldReturnAction;
        private readonly Action<object> _postYieldReturnAction;
        private readonly Func<object, object> _yieldReturnAction;

        private EnumeratorWrapper(IEnumerator original,
            Action prefixAction = null,
            Action postfixAction = null,
            Action<object> preYieldReturnAction = null,
            Action<object> postYieldReturnAction = null,
            Func<object, object> yieldReturnAction = null)
        {
            _enumerator = original;
            _prefixAction = prefixAction;
            _postfixAction = postfixAction;
            _preYieldReturnAction = preYieldReturnAction;
            _postYieldReturnAction = postYieldReturnAction;
            _yieldReturnAction = yieldReturnAction;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator GetEnumerator()
        {
            _prefixAction?.Invoke();
            while (_enumerator.MoveNext())
            {
                var item = _enumerator.Current;
                _preYieldReturnAction?.Invoke(item);
                yield return _yieldReturnAction?.Invoke(item) ?? item;
                _postYieldReturnAction?.Invoke(item);
            }
            _postfixAction?.Invoke();
        }

        /// <summary>
        /// Creates a new enumerator that wraps the original with additional actions.
        /// </summary>
        /// <param name="original">The original enumerator</param>
        /// <param name="prefixAction">Action executed before enumerator</param>
        /// <param name="postfixAction">Action executed after enumerator</param>
        /// <param name="preYieldReturnAction">Action executed before each yield return</param>
        /// <param name="postYieldReturnAction">Action executed after each yield return</param>
        /// <param name="yieldReturnAction">Function to modify the yield return value</param>
        /// <returns></returns>
        public static IEnumerator Wrap(IEnumerator original,
            Action prefixAction = null,
            Action postfixAction = null,
            Action<object> preYieldReturnAction = null,
            Action<object> postYieldReturnAction = null,
            Func<object, object> yieldReturnAction = null)
        {
            return new EnumeratorWrapper(original,
                prefixAction,
                postfixAction,
                preYieldReturnAction,
                postYieldReturnAction,
                yieldReturnAction)
                .GetEnumerator();
        }
    }
}

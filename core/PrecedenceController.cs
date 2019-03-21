using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    public class PrecedenceController : IDisposable
    {
        private readonly StringBuilder _result;
        private readonly List<JavascriptOperationTypes> _operandTypes;
        private readonly JavascriptOperationTypes _op;

        public PrecedenceController(StringBuilder result, List<JavascriptOperationTypes> operandTypes, JavascriptOperationTypes op)
        {
            _result = result;
            _operandTypes = operandTypes;
            _op = op;
            operandTypes.Add(op);
            WritePrecedenceCharIfNeeded('(');
        }

        public void Dispose()
        {
            WritePrecedenceCharIfNeeded(')');
            _operandTypes.RemoveAt(_operandTypes.Count - 1);
        }

        private void WritePrecedenceCharIfNeeded(char ch)
        {
            if (_op != 0 && !CurrentHasPrecedence())
            {
                // The current operator does not have precedence
                // over it's parent operator. We need to
                // force the current operation precedence,
                // using the given precedence operator.
                _result.Append(ch);
            }
        }

        private bool CurrentHasPrecedence()
        {
            var cnt = _operandTypes.Count;

            if (cnt < 2)
                return true;

            var current = _operandTypes[cnt - 1];
            var parent = _operandTypes[cnt - 2];

            return JsOperationHelper.CurrentHasPrecedence(current, parent);
        }
    }
}
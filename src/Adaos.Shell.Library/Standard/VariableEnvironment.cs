﻿using Adaos.Shell.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adaos.Shell.Interface;
using Adaos.Shell.Interface.SyntaxAnalysis;
using Adaos.Shell.Interface.Execution;
using Adaos.Shell.Interface.Exceptions;
using Adaos.Shell.SyntaxAnalysis.ASTs;
using Adaos.Common.Extenders;
using Adaos.Shell.Core.Extenders;
using Adaos.Shell.Library.AdHoc;

namespace Adaos.Shell.Library.Standard
{
    static class VariableEnvironmentVirtualMachineExtender
    {
        public static IEnvironmentContext GetVariableEnvironmentContext(this IVirtualMachine self)
        {
            return self?.EnvironmentContainer?.EnabledEnvironments?.FirstOrDefault(x => x?.Name == "variable");
        }
    }

    class VariableEnvironment : BaseEnvironment
    {
        private int _scope;
        private Stack<ScopeEnvironment> _scopes;
        private IEnvironment _global;

        public override string Name => "variable";
        virtual protected IVirtualMachine _vm { get; private set; }

        public VariableEnvironment(IVirtualMachine vm, IEnvironment global) : base(true)
        {
            _vm = vm;
            _global = global;
            vm.ShellExecutor.ScopeOpened += OnScopeOpen;
            vm.ShellExecutor.ScopeClosed += OnScopeClose;
            _scopes = new Stack<ScopeEnvironment>();
            Bind(DeclareVariable, "var");
            Bind(DeleteVariable, "delete");
        }

        public override Command Retrieve(string commandName)
        {
            int temp;
            if (int.TryParse(commandName, out temp))
            {
                return args => { return new[] { new DummyArgument(commandName) }.Then(args.Flatten()); };
            }
            else
            {
                var cmd = base.Retrieve(commandName);
                if(cmd == null)
                foreach(var child in _vm.GetVariableEnvironmentContext().ChildEnvironments)
                {
                    cmd = child.Retrieve(commandName);
                    if (cmd != null) break;
                }
                return cmd;
            }
        }

        private IEnumerable<IArgument> DeclareVariable(IEnumerable<IArgument> arguments)
        {
            var name = arguments.First();

            if(CustomEnvironment.Retrieve(name.Value) != null)
            {
                throw new SemanticException(name.Position, $"Variable '{name.Value}' already exists");
            }

            if (arguments.Skip(1).Any())
            {
                return SetVariable(name.Value, arguments.Skip(1), true, name.Position, CustomEnvironment);
            }
            else
            {
                return SetVariable(name.Value, new [] { new DummyArgument("=") }, true, name.Position, CustomEnvironment);
            }
        }

        private IEnumerable<IArgument> DeleteVariable(IEnumerable<IArgument> args)
        {
            if (CustomEnvironment.AllowUnbinding)
            {
                foreach (var arg in args)
                {
                    CustomEnvironment.Unbind(arg.Value);
                }
            }
            else
            {
                throw new SemanticException(-1, "Custom environment does not allow unbinding");
            }
            yield break;
        }

        private IEnvironment CustomEnvironment
        {
            get
            {
                if (_scopes.Any())
                    return _scopes.Peek();
                return _global;
            }
        }

        private IEnumerable<IArgument> SetVariable(string variable, IEnumerable<IArgument> arguments, bool newVariable, int position, IEnvironment environment)
        {
            var op = arguments.First();
            var values = arguments.Skip(1);

            if (op.Value != "=")
            {
                throw new SemanticException(op.Position, "To assign a variable use '='");
            }

            Command command;
            if (values.Count() == 1 && values.First() is IArgumentExecutable)
            {
                command = (args) =>
                {
                    try
                    {
                        var arg = values.First() as IArgumentExecutable;
                        return _vm.ShellExecutor.Execute(arg.ExecutionSequence, args,_vm);
                    }
                    catch (AdaosException e)
                    {
                        throw new SemanticException(-1, $"Error during execution of function '{variable}'", e);
                    }
                };
            }
            else
            {
                command = (args) => 
                {
                    if (args.Flatten().Any())
                    {
                        return HandleVariable(variable, values, args.Flatten(), position, environment);
                    }
                    return values;
                };
            }
            if(!newVariable)
                environment.Unbind(variable);
            environment.Bind(command, variable);

            yield break;
        }

        private IEnumerable<IArgument> HandleVariable(string variable, IEnumerable<IArgument> values, IEnumerable<IArgument> arguments, int position, IEnvironment environment)
        {
            var operatorArg = arguments.FirstOrDefault();
            if (operatorArg == null)
            {
                return values;
            }

            if (operatorArg.Value == "=")
            {
                return SetVariable(variable, arguments, false, position, environment);
            }
            else if (operatorArg.Value == "+")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x + y);
            }
            else if (operatorArg.Value == "-")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x - y);
            }
            else if (operatorArg.Value == "*")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x * y);
            }
            else if (operatorArg.Value == "/")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x / y);
            }
            else if (operatorArg.Value == "==")
            {
                return VariableFunctionBool(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x == y);
            }
            else if (operatorArg.Value == "!=")
            {
                return VariableFunctionBool(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x != y);
            }
            else if (operatorArg.Value == ">")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x > y);
            }
            else if (operatorArg.Value == "<")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x < y);
            }
            else if (operatorArg.Value == ">=")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x >= y);
            }
            else if (operatorArg.Value == "<=")
            {
                return VariableFunction(arguments.Skip(1).First().Value, values, arguments.Skip(2), position, (x, y) => x <= y);
            }
            else
            {
                throw new SemanticException(operatorArg.Position, $"Unknown operator '{operatorArg.Value}' for handling variable '{variable}'");
            }
        }

        private IEnumerable<IArgument> VariableFunction<TReturn>(string variable, IEnumerable<IArgument> values, IEnumerable<IArgument> arguments, int position, Func<int,int, TReturn> func)
        {
            var rightHandSide = CustomEnvironment.Retrieve(variable)?.Invoke(new[] { arguments });
            foreach (var pair in values.Zip(rightHandSide, (l,r) => Tuple.Create(l,r)))
            {
                int leftHand = 0;
                int rightHand = 0;
                if (!pair.Item1.TryParseTo(out leftHand))
                {
                    throw new SemanticException(position, $"Variable '{variable}' does not contain integers");
                }
                if (!pair.Item2.TryParseTo(out rightHand))
                {
                    throw new SemanticException(position, $"Variable '{pair.Item2.Name}' does not contain integers");
                }
                yield return new DummyArgument((func(leftHand,rightHand)).ToString());
            }
        }

        private IEnumerable<IArgument> VariableFunctionBool(string variable, IEnumerable<IArgument> values, IEnumerable<IArgument> arguments, int position, Func<string, string, bool> func)
        {
            var rightHandSide = CustomEnvironment.Retrieve(variable)?.Invoke(new[] { arguments });
            foreach (var pair in values.Zip(rightHandSide, (l, r) => Tuple.Create(l, r)))
            {
                yield return new DummyArgument((func(pair.Item1.Value, pair.Item2.Value)).ToString());
            }
        }

        private void OnScopeOpen(IExecutionSequence programSequence)
        {
            _scope++;
            if (_scope > 1)
            {
                var newScope = new ScopeEnvironment($"{_scope}");
                _scopes.Push(newScope);
                _vm.EnvironmentContainer.LoadEnvironment(newScope, _vm.GetVariableEnvironmentContext());
            }
        }

        private void OnScopeClose(IExecutionSequence programSequence)
        {
            if (_scopes.Any())
            {
                var scopeToRemove = _scopes.Pop();
                _vm.EnvironmentContainer.UnloadEnvironment(scopeToRemove, _vm.GetVariableEnvironmentContext());
            }
            _scope--;
        }
    }
}

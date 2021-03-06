﻿using System.Collections.Generic;
using Adaos.Shell.Interface.SyntaxAnalysis;

namespace Adaos.Shell.Interface.Execution
{
    /// <summary>
    /// An interface describing the command resolver, used to "translate" Adaos commands 
    /// in the parse tree/concrete syntax tree into executable commands within an Adaos environment.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Get the environment associated with a given command, found within the collection of 
        /// <see cref="IEnvironmentContext"/>s.
        /// </summary>
        /// <param name="command">The command to find associated environment for.</param>
        /// <param name="environments">The collection of environments to search for the command in.</param>
        /// <returns>The environment containing the command.</returns>
        IEnvironment GetEnvironmentOf(IExecution command, IEnumerable<IEnvironmentContext> environments);

        /// <summary>
        /// Resolve a given <see cref="IExecution"/> in the Adaos parse tree/concrete syntax tree 
        /// as a executable command within an Adaos environment. 
        /// </summary>
        /// <param name="command">The Adaos parse tree command to resolve.</param>
        /// <param name="environments">The collection of environments to search for the command in.</param>
        /// <returns>An executable adaos command.</returns>
        Command Resolve(IExecution command, IEnumerable<IEnvironmentContext> environments);
    }
}

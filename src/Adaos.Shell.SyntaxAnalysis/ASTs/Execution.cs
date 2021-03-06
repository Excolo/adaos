﻿using System.Collections.Generic;
using Adaos.Shell.Interface;
using Adaos.Shell.Interface.SyntaxAnalysis;

namespace Adaos.Shell.SyntaxAnalysis.ASTs
{
    /// <summary>
    /// An abstract class defining a <see cref="IExecution"/> in the Adaos AST.
    /// </summary>
    public abstract class Execution : AST, IExecution
    {
        /// <summary>
        /// The default constructor for the Command class.
        /// </summary>
        protected Execution() { }

        /// <summary>
        /// A constructor for the Command class.
        /// </summary>
        /// <param name="position">The position of the first character of the command.</param>
        protected Execution(int position) : base(position) 
        {
            RelationToPrevious = CommandRelation.Separated;
        }

        /// <summary>
        /// Get the name of the command.
        /// </summary>
        public abstract string CommandName { get; }

        /// <summary>
        /// Enumerates the <see cref="IArgument"/> belonging to the Command node.
        /// </summary>
        public abstract IEnumerable<IArgument> Arguments { get; }

        /// <summary>
        /// Enumerates the environment names of the Command node.
        /// </summary>
        public abstract IEnumerable<string> EnvironmentNames { get; }

        /// <summary>
        /// Get the type of relationship between this command and the previous command (if any).
        /// </summary>
        public CommandRelation RelationToPrevious { get; internal set; }
    }
}

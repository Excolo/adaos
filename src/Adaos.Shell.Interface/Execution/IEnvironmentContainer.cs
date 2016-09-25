﻿using System.Collections.Generic;

namespace Adaos.Shell.Interface.Execution
{
    /// <summary>
    /// An interface describing a container used to hold environments. 
    /// </summary>
    public interface IEnvironmentContainer
    {
        /// <summary>
        /// Get the loaded environments ordered by promotions. Promoted environments first.
        /// </summary>
        IEnumerable<IEnvironmentContext> EnabledEnvironments { get; }

        /// <summary>
        /// Get the unloaded environments ordered by promotions. Promoted environments first.
        /// </summary>
        IEnumerable<IEnvironmentContext> DisabledEnvironments { get; }

        /// <summary>
        /// Load the given environment into the container at the root.
        /// </summary>
        /// <param name="environment">The environment to load.</param>
        void LoadEnvironment(IEnvironment environment);

        /// <summary>
        /// Unload the given environment from the container at the root.
        /// </summary>
        /// <param name="environment">The environment to unload.</param>
        void UnloadEnvironment(IEnvironment environment);

        /// <summary>
        /// Move context to the front of contexts when getting loaded/unloaded environments.
        /// </summary>
        /// <param name="context">The context to move to the front.</param>
        void PromoteEnvironment(IEnvironmentContext context);

        /// <summary>
        /// Move context to the back of contexts when getting loaded/unloaded environments
        /// </summary>
        /// <param name="context">The context to to the back.</param>
        void DemoteEnvironment(IEnvironmentContext context);
    }

    public static class EnvironmentContainerExtender
    {
        public static void LoadEnvironments(this IEnvironmentContainer self, IEnumerable<IEnvironment> environments)
        {
            foreach(var environment in environments)
            {
                self.LoadEnvironment(environment);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adaos.Shell.Interface;
using System.IO;
using Adaos.Shell.SyntaxAnalysis.Exceptions;

namespace Adaos.Shell.Executer.Environments
{
    class IOEnvironment : Environment
    {
        virtual protected StreamWriter _output {get; private set;}
        virtual protected StreamWriter _logStream { get; private set; }

        public override string Name
        {
            get { return "io"; } 
        }

        public IOEnvironment(StreamWriter output, StreamWriter log)
        {
            _output = output;
            _logStream = log;
            Bind(Echo, "echo");
            Bind(ClearScreen, "clearscreeen", "clear", "cls");
            Bind(Log, "log");
        }

        private IEnumerable<IArgument> Echo(IEnumerable<IArgument> args)
        {
            foreach (var arg in args)
            {
                _output.Write(arg.Value + " ");
            }
            _output.WriteLine();
            yield break;
        }

        private IEnumerable<IArgument> Log(IEnumerable<IArgument> args)
        {
            _logStream.Write("[" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "] : ");
            foreach (var arg in args)
            {
                _logStream.Write(arg.Value + " ");
            }
            _logStream.WriteLine();
            yield break;
        }

        private IEnumerable<IArgument> ClearScreen(IEnumerable<IArgument> args)
        {
            _output.Flush();
            for (int i = 0; i < 100; ++i )
                _output.WriteLine();
            yield break;
        }
    }
}

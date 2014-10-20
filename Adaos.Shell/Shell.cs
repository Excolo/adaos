﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adaos.Shell.Interface;
using Adaos.Shell.Executer;
using System.IO;

namespace Adaos.Shell
{
    public class Shell : IShell
    {
        public Shell(StreamReader input, StreamWriter output, StreamWriter log = null)
        {
            if (log == null) log = new StreamWriter(Stream.Null);
            if (output == null) output = new StreamWriter(Stream.Null);
            VirtualMachine = new VirtualMachine(output, log);
            Input = input;
        }

        public Shell(IVirtualMachine virtualMachine)
        {
            if (virtualMachine == null) throw new ArgumentNullException("virtualMachine");
            VirtualMachine = virtualMachine;
        }

        public StreamWriter Output
        {
            get
            {
                return VirtualMachine.Output;
            }
            set
            {
                VirtualMachine.Output = value;
            }
        }

        public StreamWriter Log
        {
            get
            {
                return VirtualMachine.Log;
            }
            set
            {
                VirtualMachine.Log = value;
            }
        }


        public IVirtualMachine VirtualMachine
        {
            get;
            set;
        }

        public StreamReader Input
        {
            get;
            set;
        }


        public void Start()
        {
            string cmd;
            bool firstException = true;
            Output.AutoFlush = true;
            VirtualMachine.HandleError = x =>
            {
                if (x.Position >= 0)
                {
                    if (firstException)
                    {
                        string toWrite = "-";
                        for (int i = 0; i < x.Position; ++i)
                        {
                            toWrite += '-';
                        }
                        Output.WriteLine(toWrite + '^');
                    }
                    else
                    {
                        Output.WriteLine("Error occured at position: " + x.Position);
                    }
                }
                Output.WriteLine(x.Message);
                firstException = false;
            };
            try
            {
                Running = true;
                while ((cmd = Input.ReadLine()) != null && Running)
                {
                    if (cmd.Trim() != string.Empty)
                    {
                        firstException = true;
                        VirtualMachine.Execute(cmd);
                    }
                }
            }
            catch (ExitShellException)
            { }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            Running = false;
            Log.Flush();
        }

        public bool Running
        {
            get;
            private set;
        }
    }
}

﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace workspacer
{
    public class PipeServer : IDisposable
    {
        public Process WatcherProcess { get; private set; }

        private SemaphoreSlim _semaphore;

        public PipeServer()
        {
            WatcherProcess = new Process();
            WatcherProcess.StartInfo.FileName = "workspacer.Watcher.exe";
            WatcherProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            _semaphore = new SemaphoreSlim(1);
        }

        public void Start()
        {
            WatcherProcess.StartInfo.UseShellExecute = false;
            WatcherProcess.StartInfo.RedirectStandardInput = true;
            WatcherProcess.Start();
        }

        public void Dispose()
        {
            WatcherProcess.Close();
            WatcherProcess.WaitForExit();
        }

        public void SendResponse(string response)
        {
            Task.Run(() => Enqueue(() => WatcherProcess.StandardInput.WriteLineAsync(response)));
        }

        private async Task Enqueue(Func<Task> taskGenerator)
        {
            await _semaphore.WaitAsync();
            try
            {
                await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SharpChat.Events {
    public class EventDispatcher : IEventDispatcher {
        private Queue<(object sender, IEvent evt)> EventQueue { get; } = new();
        private object SyncQueue { get; } = new();

        private HashSet<IEventHandler> EventHandlers { get; } = new();
        private object SyncHandlers { get; } = new();

        private HashSet<IEventHandler> PreventDelete { get; } = new();
        private object SyncPrevent { get; } = new();

        private bool IsRunning = false;
        private bool RunUntilEmpty = false;
        private Thread ProcessThread = null;

        [Conditional(@"DEBUG")]
        private static void WithDebugColour(string str, ConsoleColor colour) {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Logger.Debug(str);
            Console.ForegroundColor = prev;
        }

        public void DispatchEvent(object sender, IEvent evt) {
            lock(SyncQueue) {
                WithDebugColour($@"+ {evt} <- {sender}.", ConsoleColor.Red);
                EventQueue.Enqueue((sender, evt));
            }
        }

        public void AddEventHandler(IEventHandler handler) {
            if(handler == null)
                throw new ArgumentNullException(nameof(handler));
            lock(SyncHandlers)
                EventHandlers.Add(handler);
        }

        internal void ProtectEventHandler(IEventHandler handler) {
            lock(SyncPrevent)
                PreventDelete.Add(handler);
        }

        public void RemoveEventHandler(IEventHandler handler) {
            if(handler == null)
                throw new ArgumentNullException(nameof(handler));
            // prevent asshattery
            lock(SyncPrevent)
                if(PreventDelete.Contains(handler))
                    return;
            lock(SyncHandlers)
                EventHandlers.Remove(handler);
        }

        public bool ProcessNextQueue() {
            (object sender, IEvent evt) queued;

            lock(SyncQueue) {
                if(!EventQueue.TryDequeue(out queued))
                    return false;
                WithDebugColour($@"~ {queued.evt} <- {queued.sender}.", ConsoleColor.Green);
            }

            lock(SyncHandlers)
                foreach(IEventHandler handler in EventHandlers)
                    handler.HandleEvent(queued.sender, queued.evt);

            return true;
        }

        public void StartProcessing() {
            if(IsRunning)
                return;
            IsRunning = true;

            ProcessThread = new Thread(() => {
                bool hadEvent;
                do {
                    hadEvent = ProcessNextQueue();
                    if(RunUntilEmpty && !hadEvent)
                        StopProcessing();
                    else
                        Thread.Sleep(1);
                } while(IsRunning);
            });
            ProcessThread.Start();
        }

        public void FinishProcessing() {
            RunUntilEmpty = true;
            ProcessThread.Join();
        }

        public void StopProcessing() {
            IsRunning = false;
            RunUntilEmpty = false;
        }
    }
}

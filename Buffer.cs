using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Assignment4_CS_GUI
{
    internal class Buffer
    {
        public enum BufferStatus { Empty, Processed, New}
        public event EventHandler<string> WriteEvent;
        public event EventHandler<string> ModifyEvent;
        public event EventHandler<string> ReadEvent;
        public event EventHandler OnReadComplete;
        private string[] bufferArray;
        private BufferStatus[] stateArray;
        private int writeIndex;
        private int readIndex;
        private int modifyIndex;
        private const int bufferCapacity = 25;
        private readonly object syncLock = new object();
        public string[] Contents => bufferArray;

        

        public Buffer()
        {
            bufferArray=new string[bufferCapacity];
            stateArray=new BufferStatus[bufferCapacity];
            writeIndex=0;
            readIndex=0;
            modifyIndex=0;

            for (int i = 0; i < bufferCapacity; i++)
            {
                stateArray[i]=BufferStatus.Empty;
            }
        }
        public void TriggerWriteEvent(string message)
        {
            WriteEvent?.Invoke(this, message);
        }
        public void TriggerModifyEvent(string message)
        {
            ModifyEvent?.Invoke(this, message);
        }
        public void TriggerReadEvent(string message)
        {
            ReadEvent?.Invoke(this, message);
        }
        public void TriggerReadComplete()
        {
            OnReadComplete?.Invoke(this, EventArgs.Empty);
        }

        public void WriteLine(string line)
        {
            Monitor.Enter(syncLock);
            try
            {
                while (stateArray[writeIndex] != BufferStatus.Empty)
                {
                    Monitor.Wait(syncLock);
                }
                bufferArray[writeIndex] = line;
                stateArray[writeIndex] = BufferStatus.New;
                writeIndex = (writeIndex + 1) % bufferCapacity;
                Monitor.PulseAll(syncLock);
            }
            finally
            {
                Monitor.Exit(syncLock);
            }
        }


        public void ModifyLine(string find, string replace)
        {
            Monitor.Enter(syncLock);
            try
            {
                while (stateArray[modifyIndex] != BufferStatus.New)
                {
                    Monitor.Wait(syncLock);
                }
                var currentLine = bufferArray[modifyIndex];
                string modifyLine = currentLine.Replace(find, replace);
                if (modifyLine != currentLine)
                {
                    bufferArray[modifyIndex] = modifyLine;
                }
                stateArray[modifyIndex] = BufferStatus.Processed;
                modifyIndex = (modifyIndex + 1) % bufferCapacity;
            }
            finally
            {
                Monitor.PulseAll(syncLock);
                Monitor.Exit(syncLock);
            }
        }

        public void ReadLine()
        {
            Monitor.Enter(syncLock);
            try
            {
                while (stateArray[readIndex] != BufferStatus.Processed)
                {
                    Monitor.Wait(syncLock);
                }
                stateArray[readIndex] = BufferStatus.Empty;
                TriggerReadComplete();
                readIndex = (readIndex + 1) % bufferCapacity;
            }
            finally
            {
                Monitor.PulseAll(syncLock);
                Monitor.Exit(syncLock);
            }

        }

        public void Print()
        {
            Console.WriteLine("Buffer contents:");
            for(int i = 0; i < bufferCapacity; i++)
            {
                Console.WriteLine($"Position {i}: {bufferArray[i]}");
                Console.WriteLine($"Status {i}: {stateArray[i]}");
            }
        }

    }
}

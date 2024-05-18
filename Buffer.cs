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
        // Enum to represent the status of buffer elements
        public enum BufferStatus { Empty, Processed, New }

        // Events for buffer operations
        public event EventHandler<string> WriteEvent;
        public event EventHandler<string> ModifyEvent;
        public event EventHandler<string> ReadEvent;
        public event EventHandler OnReadComplete;

        // Buffer properties and fields
        private string[] bufferArray;
        private BufferStatus[] stateArray;
        private int writeIndex;
        private int readIndex;
        private int modifyIndex;
        private const int bufferCapacity = 25; // Maximum capacity of the buffer
        private readonly object syncLock = new object(); // Object for synchronization

        // Property to get the contents of the buffer
        public string[] Contents => bufferArray;

        // Constructor to initialize the buffer
        public Buffer()
        {
            bufferArray = new string[bufferCapacity];
            stateArray = new BufferStatus[bufferCapacity];
            writeIndex = 0;
            readIndex = 0;
            modifyIndex = 0;

            // Initialize all buffer states to Empty
            for (int i = 0; i < bufferCapacity; i++)
            {
                stateArray[i] = BufferStatus.Empty;
            }
        }

        // Method to trigger the WriteEvent
        public void TriggerWriteEvent(string message)
        {
            WriteEvent?.Invoke(this, message);
        }

        // Method to trigger the ModifyEvent
        public void TriggerModifyEvent(string message)
        {
            ModifyEvent?.Invoke(this, message);
        }

        // Method to trigger the ReadEvent
        public void TriggerReadEvent(string message)
        {
            ReadEvent?.Invoke(this, message);
        }

        // Method to trigger the ReadComplete event
        public void TriggerReadComplete()
        {
            OnReadComplete?.Invoke(this, EventArgs.Empty);
        }

        // Method to write a line to the buffer
        public void WriteLine(string line)
        {
            Monitor.Enter(syncLock);
            try
            {
                // Wait until there's space in the buffer
                while (stateArray[writeIndex] != BufferStatus.Empty)
                {
                    Monitor.Wait(syncLock);
                }
                // Write the line to the buffer
                bufferArray[writeIndex] = line;
                stateArray[writeIndex] = BufferStatus.New;
                writeIndex = (writeIndex + 1) % bufferCapacity;
                Monitor.PulseAll(syncLock); // Notify waiting threads
            }
            finally
            {
                Monitor.Exit(syncLock);
            }
        }

        // Method to modify a line in the buffer
        public void ModifyLine(string find, string replace)
        {
            Monitor.Enter(syncLock);
            try
            {
                // Wait until there's a new line to modify
                while (stateArray[modifyIndex] != BufferStatus.New)
                {
                    Monitor.Wait(syncLock);
                }
                // Modify the line in the buffer
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
                Monitor.PulseAll(syncLock); // Notify waiting threads
                Monitor.Exit(syncLock);
            }
        }

        // Method to read a line from the buffer
        public void ReadLine()
        {
            Monitor.Enter(syncLock);
            try
            {
                // Wait until there's a processed line to read
                while (stateArray[readIndex] != BufferStatus.Processed)
                {
                    Monitor.Wait(syncLock);
                }
                // Mark the line as empty and trigger read complete event
                stateArray[readIndex] = BufferStatus.Empty;
                TriggerReadComplete();
                readIndex = (readIndex + 1) % bufferCapacity;
            }
            finally
            {
                Monitor.PulseAll(syncLock); // Notify waiting threads
                Monitor.Exit(syncLock);
            }

        }

        // Method to print the buffer contents and states
        public void Print()
        {
            Console.WriteLine("Buffer contents:");
            for (int i = 0; i < bufferCapacity; i++)
            {
                Console.WriteLine($"Position {i}: {bufferArray[i]}");
                Console.WriteLine($"Status {i}: {stateArray[i]}");
            }
        }

    }
}

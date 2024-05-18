using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment4_CS_GUI
{
    internal class TextEditor
    {
        // Fields and properties
        public Buffer buffer;
        public MainForm mainForm;
        Mutex lineMutex = new Mutex();
        int currentWriteIndex = 0;
        int currentReadIndex = 0;
        int currentModifyIndex = 0;
        bool isWriterActive = true;
        bool isModifierActive = true;
        bool isReaderActive = true;

        // Constructor
        public TextEditor(MainForm mainForm)
        {
            buffer = new Buffer();
            this.mainForm = mainForm;
            AllEvents(); // Subscribe to buffer events
        }

        // Method to launch writer, modifier, and reader threads
        public void LaunchThreads()
        {
            // Launch writer threads
            for (int i = 0; i < 3; i++)
            {
                Thread writeThread = new Thread(WriteThread);
                writeThread.Start();
            }
            // Launch modifier threads
            for (int i = 0; i < 4; i++)
            {
                Thread modifyThread = new Thread(ModifyThread);
                modifyThread.Start();
            }
            // Launch reader thread
            Thread readThread = new Thread(ReadThread);
            readThread.Start();
        }

        // Writer thread method
        private void WriteThread()
        {
            while (isWriterActive)
            {
                lineMutex.WaitOne();
                if (currentWriteIndex < mainForm.Lines.Length)
                {
                    // Write line to buffer
                    buffer.WriteLine(mainForm.Lines[currentWriteIndex]);
                    string action = $"Writer wrote: '{mainForm.Lines[currentWriteIndex]}' at Index {currentWriteIndex}";
                    buffer.TriggerWriteEvent(action);
                    currentWriteIndex++;
                }
                else
                {
                    isWriterActive = false; // Stop writer thread
                    buffer.Print(); // Print buffer contents
                }
                lineMutex.ReleaseMutex();
            }
        }

        // Modifier thread method
        private void ModifyThread()
        {
            while (isModifierActive)
            {
                if (currentModifyIndex < mainForm.Lines.Length)
                {
                    // Get find and replace inputs
                    string input = mainForm.txtFind.Text;
                    string output = mainForm.txtReplace.Text;
                    // Modify line in buffer
                    buffer.ModifyLine(input, output);
                    string action = $"Modifier modified: Replaced '{input}' with '{output}' at Index {currentModifyIndex}";
                    buffer.TriggerModifyEvent(action);
                    currentModifyIndex++;
                }
                else
                {
                    isModifierActive = false; // Stop modifier thread
                }
            }

        }

        // Reader thread method
        private void ReadThread()
        {
            while (isReaderActive)
            {
                if (currentReadIndex < mainForm.Lines.Length)
                {
                    // Read line from buffer
                    buffer.ReadLine();
                    string action = $"Reader read line '{mainForm.Lines[currentReadIndex]}' at Index {currentWriteIndex}";
                    buffer.TriggerReadEvent(action);
                    currentReadIndex++;
                }
                else
                {
                    isReaderActive = false; // Stop reader thread
                }
            }
        }

        // Method to update the status ListBox on the main form
        private void UpdateStatusListBox(string action)
        {
            // Assuming listStatus is your ListBox control
            if (mainForm.listStatus.InvokeRequired)
            {
                mainForm.listStatus.Invoke(new Action(() => mainForm.listStatus.Items.Add(action)));
            }
            else
            {
                mainForm.listStatus.Items.Add(action);
            }
        }

        // Method to subscribe to buffer events
        public void AllEvents()
        {
            buffer.WriteEvent += (sender, action) => UpdateStatusListBox(action);
            buffer.ModifyEvent += (sender, action) => UpdateStatusListBox(action);
            buffer.ReadEvent += (sender, action) => UpdateStatusListBox(action);
        }
    }
}

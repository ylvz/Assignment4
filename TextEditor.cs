using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment4_CS_GUI
{
    internal class TextEditor
    {
        public Buffer buffer;
        public MainForm mainForm;
        Mutex lineMutex = new Mutex();
        int currentWriteIndex = 0;
        int currentReadIndex = 0;
        int currentModifyIndex = 0;
        bool isWriterActive = true;
        bool isModifierActive = true;
        bool isReaderActive = true;

        public TextEditor(MainForm mainForm)
        {
            buffer=new Buffer();
            this.mainForm = mainForm;
            AllEvents();
        }

        public void LaunchThreads()
        {
            for(int i = 0; i < 3; i++)
            {
                Thread writeThread = new Thread(WriteThread);
                writeThread.Start();
            }
            for(int i = 0; i < 4; i++)
            {
                Thread modifyThread = new Thread(ModifyThread);
                modifyThread.Start();
            }
            Thread readThread = new Thread(ReadThread);
            readThread.Start();
        }

        private void WriteThread()
        {
            while (isWriterActive)
            {
                lineMutex.WaitOne();
                if (currentWriteIndex < mainForm.Lines.Length)
                {
                    buffer.WriteLine(mainForm.Lines[currentWriteIndex]);
                    string action = $"Writer wrote: '{mainForm.Lines[currentWriteIndex]}' at Index {currentWriteIndex}";
                    buffer.TriggerWriteEvent(action);
                    currentWriteIndex++;
                }
                else
                {
                    isWriterActive = false;
                    buffer.Print();
                }
                lineMutex.ReleaseMutex();
            }
        }
        private void ModifyThread()
        {
            while (isModifierActive)
            {
                if (currentModifyIndex < mainForm.Lines.Length)
                {
                    string input = mainForm.txtFind.Text;
                    string output = mainForm.txtReplace.Text;
                    buffer.ModifyLine(input, output);
                    string action = $"Modifier modified: Replaced '{input}' with '{output}' at Index {currentModifyIndex}";
                    buffer.TriggerModifyEvent(action);
                    currentModifyIndex++;
                }
                else 
                { 
                    isModifierActive = false; 
                }
            }

        }

        private void ReadThread()
        {
            while (isReaderActive)
            {
                if (currentReadIndex < mainForm.Lines.Length)
                {
                    buffer.ReadLine();
                    string action = $"Reader read line '{mainForm.Lines[currentReadIndex]}' at Index {currentWriteIndex}";
                    buffer.TriggerReadEvent(action);
                    currentReadIndex++;
                }
                else { isReaderActive = false; }
            }
        }
        private void UpdateStatusListBox(string action)
        {
            // Assuming lstStatus is your ListBox control
            if (mainForm.listStatus.InvokeRequired)
            {
                mainForm.listStatus.Invoke(new Action(() => mainForm.listStatus.Items.Add(action)));
            }
            else
            {
                mainForm.listStatus.Items.Add(action);
            }
        }

        public void AllEvents()
        {
            buffer.WriteEvent += (sender, action) => UpdateStatusListBox(action);
            buffer.ModifyEvent += (sender, action) => UpdateStatusListBox(action);
            buffer.ReadEvent += (sender, action) => UpdateStatusListBox(action);
        }
    }
}

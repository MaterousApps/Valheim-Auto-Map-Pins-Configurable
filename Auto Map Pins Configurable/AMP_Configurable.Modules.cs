using AMP_Configurable.Commands;
using AMP_Configurable.Patches;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AMP_Configurable.Modules
{
    internal class ModConsoleOpt : AMPBaseModule
    {
        private string consoleLastMessage = string.Empty;
        private History consoleHistory = new History();


        public ModConsoleOpt()
        {
            this.ModuleName = "CC Controller";
            this.Loading();
        }

        public void Start()
        {
            AMP_Commands.ConsoleOpt = this;
            Ready();
        }

        public void HandleConsole()
        {
            if (!(Console.instance != null))
                return;
            if (Console.instance.m_chatWindow.gameObject.activeInHierarchy)
            {
                string text = Console.instance.m_input.text;
                if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
                    consoleLastMessage = string.Empty;
                if (!text.Equals(string.Empty) && !text.Equals(consoleLastMessage))
                    consoleLastMessage = text;
                if (Input.GetKeyDown(KeyCode.Return) && text.Equals(string.Empty) && !consoleLastMessage.Equals(string.Empty))
                {
                    consoleHistory.Add(consoleLastMessage);
                    AMP_Commands.ProcessCommands(consoleLastMessage);
                    this.consoleLastMessage = string.Empty;
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    Console.instance.m_input.text = consoleHistory.Fetch(text, true);
                    Console.instance.m_input.caretPosition = Console.instance.m_input.text.Length;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    Console.instance.m_input.text = consoleHistory.Fetch(text, false);
            }
            if (!Input.GetKeyDown(KeyCode.Slash) || Console.IsVisible() || (Chat.instance.IsChatDialogWindowVisible() || TextInput.IsVisible()))
                return;
            Console.instance.m_chatWindow.gameObject.SetActive(true);
            Console.instance.m_input.caretPosition = Console.instance.m_input.text.Length;
        }



        private void Update()
        {
            HandleConsole();

        }

        private void OnDestroy()
        {
            if (AMPCommandPatcher.Harmony == null)
                return;
            AMPCommandPatcher.Harmony.UnpatchSelf();
        }



        private class History
        {
            private List<string> history = new List<string>();
            private int index;
            private string current;

            public void Add(string item)
            {
                this.history.Add(item);
                this.index = 0;
            }

            public string Fetch(string current, bool next)
            {
                if (index == 0)
                    this.current = current;
                if (history.Count == 0)
                    return current;
                index += !next ? 1 : -1;
                if (history.Count + index >= 0 && history.Count + index <= history.Count - 1)
                    return history[history.Count + index];
                index = 0;
                return this.current;
            }
        }
    }

    internal class AMPBaseModule : MonoBehaviour
    {
        internal string ModuleName = "UNNAMED";

        public void RemoveModule() => Destroy(this);

        internal ResourceUtils.Status ModuleStatus { get; set; }

        internal void Ready() => ModuleStatus = ResourceUtils.Status.Ready;

        internal void Loading() => ModuleStatus = ResourceUtils.Status.Loading;

        internal void Error() => ModuleStatus = ResourceUtils.Status.Error;

        internal void Unload() => ModuleStatus = ResourceUtils.Status.Unload;
    }

    internal interface IModule
    {
        void BeginMenu();

        void Start();
    }
}
